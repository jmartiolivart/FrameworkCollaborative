using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode.Components;

public class SingleToMultiplayer : MonoBehaviour
{
    public string Adress = "127.0.0.1";
    public ushort Port = 7777;
    [Header("Prefab")]
    public GameObject Jugador;

    [Header("Per proves locals, es millor deixar desactivada l'opció, per evitar\n exposar els ports del teu dispositu\n")]
    public bool PermetreConnexionsRemotes = false;
    public ProtocolType Protocol;

    private GameObject networkManager;
    private NetworkManager networkManagerComponent;
    private UnityTransport unityTransportComponent;

    private void Awake()
    {
        networkManager = new GameObject("NetworkManager");
        networkManagerComponent = networkManager.AddComponent<NetworkManager>();
        unityTransportComponent = networkManager.AddComponent<UnityTransport>();
    }

    void Start()
    {
        StartCoroutine(InitializeNetworkManager());
    }

    private IEnumerator InitializeNetworkManager()
    {
        // Configurem el UnityTransport
        unityTransportComponent = settingUpUnityTransport(unityTransportComponent);

        // Creem un nou objecte NetworkConfig i l'assignem a NetworkManager
        networkManagerComponent.NetworkConfig = new NetworkConfig();
        networkManagerComponent.NetworkConfig.NetworkTransport = (NetworkTransport)unityTransportComponent;

        // Assegurar-nos que el prefab té el NetworkObject
        EnsurePlayerPrefabHasNetworkObject();

        // Esperem un frame per assegurar-nos que el prefab està completament inicialitzat
        yield return null;

        // Configuerem el NetworkManager
        networkManagerComponent = settingUpNetworkManager(networkManagerComponent);

        // Component NetworkConnect
        NetworkConnect networkConnect = GetComponent<NetworkConnect>();
        if (networkConnect == null)
        {
            networkConnect = networkManager.AddComponent<NetworkConnect>();
        }
        networkConnect.transport = unityTransportComponent; // Assigna el transport

        // Agafem el valor indicat per l'usuari per si es vol el UnityTransport tipus DIRECTE o tipus RELAY
        if (Protocol == ProtocolType.UnityTransport)
        {
            directConfiguration();
        }

        // RELAY (Fent servir el RELAY)
        if (Protocol == ProtocolType.RelayUnityTransport)
        {
        }

        yield break; // Assegurem que la coroutine sempre retorna un valor
    }

    // Codi del NetworkManager revisat per assegurar que el prefab del jugador està correctament assignat
    private void EnsurePlayerPrefabHasNetworkObject()
    {
        if (Jugador != null)
        {
            NetworkObject networkObject = Jugador.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Jugador.AddComponent<NetworkObject>();
            }

            // Assegurar-nos que NetworkTransform també està present
            NetworkTransformClient networkTransform = Jugador.GetComponent<NetworkTransformClient>();
            if (networkTransform == null)
            {
                Jugador.AddComponent<NetworkTransformClient>();
            }
        }
        else
        {
            Debug.LogError("Jugador prefab no està assignat!");
        }
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
            NetworkObject networkObject = Jugador.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("El prefab Jugador no té un component NetworkObject assignat.");
            }
            else
            {
                networkManagerComponent.NetworkConfig.PlayerPrefab = Jugador;
            }
        }

        return networkManagerComponent;
    }

    public void directConfiguration() { }

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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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

        private void Update()
        {
            if (timerPing > 15)
            {
                timerPing = 0;
                if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                }
            }
            timerPing += Time.deltaTime;
        }
    }
}
