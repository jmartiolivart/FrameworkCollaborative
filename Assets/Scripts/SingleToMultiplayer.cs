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

public class SingleToMultiplayer : MonoBehaviour
{

    // Assignació de valors per defecte
    public string Adress = "127.0.0.1"; // Valor per defecte: "127.0.0.1"
    public ushort Port = 7777; // Valor per defecte: 7777
    [Header("Prefab")]
    public GameObject Jugador;
    
    
    //Actualment no operatius ja que no s'hi pot accedir via codi
    [Header("Per proves locals, es millor deixar desactivada l'opció, per evitar\n exposar els ports del teu dispositu\n")]
    public bool PermetreConnexionsRemotes = false ;
    public ProtocolType Protocol;
    


    private GameObject networkManager;
    private NetworkManager networkManagerComponent;
    private UnityTransport unityTransportComponent;

    private void Awake()
    {
        // Creem un 2 components anomenats "NetworkManager" i "UnityTransport" i els afegim al nou GameObject NetworkManager
        networkManager = new GameObject("NetworkManager");
        networkManagerComponent = networkManager.AddComponent<NetworkManager>();
        unityTransportComponent = networkManager.AddComponent<UnityTransport>();
    }
    void Start()
    { 
        //Configurem el UnityTransport
        unityTransportComponent  = settingUpUnityTransport(unityTransportComponent);

        // Creem un nou objecte NetworkConfig i l'assignem a NetworkManager i creem NetworkManager se li passa unityTransport creat
        networkManagerComponent.NetworkConfig = new NetworkConfig();
        networkManagerComponent.NetworkConfig.NetworkTransport = (NetworkTransport) unityTransportComponent;

        //Configuerem el NetworkManager
        networkManagerComponent = settingUpNetworkManager(networkManagerComponent);


        //Agafem el valor indicat per l'usuari per si es vol el UnityTransport tipus DIRECTE o tipus RELAY
        if (Protocol == ProtocolType.UnityTransport)
        {
            directConfiguration();
            Debug.LogError(Protocol + "ITS DIRECT");
        }

        //RELAY (Fent servir el RELAY)
        if (Protocol == ProtocolType.RelayUnityTransport)
        {
            Debug.LogError(Protocol + "ITS RELAYYYY");
            // Component NetworkConnect
            NetworkConnect networkConnect = GetComponent<NetworkConnect>();
            if (networkConnect == null)
            {
                networkConnect = networkManager.AddComponent<NetworkConnect>();
            }
        }
        //unityTransportComponent.SetRelayServerData(new RelayServerData(allocation, "dtls"));



    }

    private UnityTransport settingUpUnityTransport(UnityTransport unityTransportComponent)
    {
        unityTransportComponent.ConnectionData.Address = Adress;
        unityTransportComponent.ConnectionData.Port = Port;
        //unityTransportComponent.ConnectionData.AllowRemoteConnection = PermetreConnexionsRemotes;


        


        return unityTransportComponent;
    }


    private NetworkManager settingUpNetworkManager(NetworkManager networkManagerComponent)
    {
        // Comprovem si la variable Jugador és null
        if (Jugador == null)
        {
            // Mostrem un missatge d'advertència a la consola de Unity
            Debug.LogError("Es necessari afegir un prefab de Jugador a la variable Jugador.");
        }
        else
        {
            networkManagerComponent.NetworkConfig.PlayerPrefab = Jugador;


        }

        return networkManagerComponent;
    }


    public void directConfiguration()
    {

    }


    public enum ProtocolType
    {
        UnityTransport = 0,
        RelayUnityTransport = 1
    }

    /**********************************************************************************************/
    /**********************************************************************************************/
    /************************************CODI DE RELAY*********************************************/
    /**********************************************************************************************/
    /**********************************************************************************************/
    /**********************************************************************************************/
    /**********************************************************************************************/

    public class NetworkConnect : MonoBehaviour
    {
        // Classe per decidir si crear la sala o unir-se a una.
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

            Debug.LogError(joinCode);

            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>();
            DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, joinCode);
            lobbyOptions.Data.Add("JOIN_KEY", dataObject);

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
                timerPing += Time.deltaTime;
            }
        }
    }

}
