using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System;
using Unity.Netcode.Components;

public class SingleToMultiplayer : NetworkBehaviour
{
    public string Adress = "127.0.0.1";
    public ushort Port = 7777;
    [Header("Prefab")]
    public GameObject Jugador;
    public GameObject playerNetworkContainerPrefab;

    [Header("Per proves locals, es millor deixar desactivada l'opció, per evitar exposar els ports del teu dispositu")]
    public bool PermetreConnexionsRemotes = false;
    public ProtocolType Protocol;

    private GameObject networkManager;
    private NetworkManager networkManagerComponent;
    private UnityTransport unityTransportComponent;

    async void Awake()
    {
        await UnityServices.InitializeAsync();
        networkManager = new GameObject("NetworkManager");
        networkManagerComponent = networkManager.AddComponent<NetworkManager>();
        unityTransportComponent = networkManager.AddComponent<UnityTransport>();
        Debug.Log("SingleToMultiplayer Awake called");
    }

    private void Start()
    {
        Debug.Log("SingleToMultiplayer Start called");
        unityTransportComponent = settingUpUnityTransport(unityTransportComponent);

        networkManagerComponent.NetworkConfig = new NetworkConfig();
        networkManagerComponent.NetworkConfig.NetworkTransport = (NetworkTransport)unityTransportComponent;

        networkManagerComponent = settingUpNetworkManager(networkManagerComponent);

        if (Jugador != null)
        {
            NetworkObject networkObject = Jugador.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Jugador.AddComponent<NetworkObject>();
            }

            NetworkTransform networkTransform = Jugador.GetComponent<NetworkTransform>();
            if (networkTransform == null)
            {
                Jugador.AddComponent<NetworkTransform>();
            }
        }
        else
        {
            Debug.LogError("Jugador prefab no està assignat!");
        }

        networkManagerComponent.NetworkConfig.PlayerPrefab = Jugador;

        if (Protocol == ProtocolType.UnityTransport)
        {
            NetworkManager.Singleton.StartHost();
        }
        else if (Protocol == ProtocolType.RelayUnityTransport)
        {
            Debug.LogError(Protocol + " ITS RELAYYYY");
            NetworkConnect networkConnect = GetComponent<NetworkConnect>();
            if (networkConnect == null)
            {
                networkConnect = networkManager.AddComponent<NetworkConnect>();
            }
        }

        NetworkManager.Singleton.OnServerStarted += OnServerStartedHandler;
    }

    private UnityTransport settingUpUnityTransport(UnityTransport unityTransportComponent)
    {
        unityTransportComponent.ConnectionData.Address = Adress;
        unityTransportComponent.ConnectionData.Port = Port;
        return unityTransportComponent;
    }

    private NetworkManager settingUpNetworkManager(NetworkManager networkManagerComponent)
    {
        if (Jugador == null)
        {
            Debug.LogError("Es necessari afegir un prefab de Jugador a la variable Jugador.");
        }
        else
        {
            networkManagerComponent.NetworkConfig.PlayerPrefab = Jugador;
        }
        return networkManagerComponent;
    }

    private void OnServerStartedHandler()
    {
        Debug.Log("OnServerStartedHandler called");
        if (NetworkManager.Singleton.IsServer)
        {
            if (Jugador == null)
            {
                Debug.LogError("Jugador prefab no està assignat!");
                return;
            }

            GameObject playerNetworkContainer = GameObject.Find("PlayerNetworkContainer");
            if (playerNetworkContainer == null)
            {
                Debug.LogError("PlayerNetworkContainer no està present en la escena! Intentant instanciar-lo manualment...");
                if (playerNetworkContainerPrefab != null)
                {
                    playerNetworkContainer = Instantiate(playerNetworkContainerPrefab);
                    playerNetworkContainer.name = "PlayerNetworkContainer";
                    NetworkObject containerNetworkObject = playerNetworkContainer.GetComponent<NetworkObject>();
                    if (containerNetworkObject != null)
                    {
                        containerNetworkObject.Spawn();
                    }
                    Debug.Log("PlayerNetworkContainer instantiated and spawned manually");
                }
                else
                {
                    Debug.LogError("playerNetworkContainerPrefab is not assigned in the inspector");
                    return;
                }
            }

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject == null)
                {
                    GameObject playerInstance = Instantiate(Jugador);
                    NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        networkObject.SpawnAsPlayerObject(clientId);
                        Debug.Log("Instantiated Player");

                        Debug.Log("NetworkObject del PlayerNetworkContainer:");
                        Debug.Log("  IsSpawned: " + networkObject.IsSpawned);
                        Debug.Log("  IsOwner: " + networkObject.IsOwner);
                        Debug.Log("  OwnerClientId: " + networkObject.OwnerClientId);
                    }
                    else
                    {
                        Debug.LogError("NetworkObject no està assignat al prefab Jugador!");
                    }
                }
            }
        }
    }

    public enum ProtocolType
    {
        UnityTransport = 0,
        RelayUnityTransport = 1
    }

    public class NetworkConnect : MonoBehaviour
    {
        public int maxConnection = 10;
        public UnityTransport transport;

        private Lobby currentLobby;
        private float timerPing;

        private async void Awake()
        {
            Debug.Log("NetworkConnect Awake called");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            JoinOrCreate();
        }

        public async void JoinOrCreate()
        {
            try
            {
                currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                string relayJoinCode = currentLobby.Data["JOIN_KEY"].Value;
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
                NetworkManager.Singleton.StartClient();
            }
            catch
            {
                Create();
            }
        }

        public async void Create()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.LogError(joinCode);

                transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
            {
                { "JOIN_KEY", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            }
                };

                currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);

                NetworkManager.Singleton.StartHost();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error creating Relay allocation: " + ex.Message);
            }
        }

        private void Update()
        {
            if (timerPing > 15)
            {
                timerPing = 0;
                if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                }
                timerPing += Time.deltaTime;
            }
        }
    }
}
