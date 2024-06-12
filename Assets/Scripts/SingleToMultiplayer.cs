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
using System.Collections;
using Unity.Netcode.Components;

public class SingleToMultiplayer : NetworkBehaviour
{
    public string Address = "127.0.0.1";
    public ushort Port = 7777;
    [Header("Prefab")]
    public GameObject Jugador;
    public GameObject playerNetworkContainerPrefab;

    [Header("Per proves locals, es millor deixar desactivada l'opció, per evitar exposar els ports del teu dispositiu")]
    public bool PermetreConnexionsRemotes = false;
    public ProtocolType Protocol;

    private GameObject networkManager;
    private NetworkManager networkManagerComponent;
    private UnityTransport unityTransportComponent;

    async void Awake()
    {
        Debug.Log("SingleToMultiplayer Awake called");
        await UnityServices.InitializeAsync();

        networkManager = new GameObject("NetworkManager");
        networkManagerComponent = networkManager.AddComponent<NetworkManager>();
        unityTransportComponent = networkManager.AddComponent<UnityTransport>();

        unityTransportComponent.ConnectionData.Address = Address;
        unityTransportComponent.ConnectionData.Port = Port;

        networkManagerComponent.NetworkConfig = new NetworkConfig();
        networkManagerComponent.NetworkConfig.NetworkTransport = (NetworkTransport)unityTransportComponent;

        networkManagerComponent.NetworkConfig.PlayerPrefab = Jugador;

        if (Protocol == ProtocolType.RelayUnityTransport)
        {
            Debug.LogError(Protocol + " ITS RELAYYYY");
            NetworkConnect networkConnect = networkManager.AddComponent<NetworkConnect>();
            networkConnect.transport = unityTransportComponent; // Assigna el transport
        }
    }

    private void Start()
    {
        Debug.Log("SingleToMultiplayer Start called");

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

        if (Protocol == ProtocolType.UnityTransport)
        {
            NetworkManager.Singleton.StartHost();
        }
    }

    private void OnServerStartedHandler()
    {
        Debug.Log("OnServerStartedHandler called");

        if (NetworkManager.Singleton.IsServer)
        {
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
                        Debug.Log("PlayerNetworkContainer instantiated and spawned manually");
                    }
                    else
                    {
                        Debug.LogError("NetworkObject no està assignat al prefab PlayerNetworkContainer!");
                    }
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
            StartCoroutine(DelayedJoinOrCreate());
        }

        private IEnumerator DelayedJoinOrCreate()
        {
            yield return null;
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
                Debug.Log("Iniciant creació d'allocació de Relay...");
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
                Debug.Log("Allocació de Relay creada correctament.");

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Join code obtingut: " + joinCode);

                Debug.Log("Verificant que transport no és null...");
                if (transport == null)
                {
                    Debug.LogError("transport és null!");
                    return;
                }

                Debug.Log("Configurant dades de Relay...");
                transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                Debug.Log("Creant lobby...");
                CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "JOIN_KEY", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                    }
                };

                currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);
                Debug.Log("Lobby creat correctament: " + currentLobby.Id);

                Debug.Log("Iniciant Host...");
                NetworkManager.Singleton.StartHost();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Error creant lobby: " + e.Message);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Error creant allocació de Relay: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("Error inesperat: " + e.Message);
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
