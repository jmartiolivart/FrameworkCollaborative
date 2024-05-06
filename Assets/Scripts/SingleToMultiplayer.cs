using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;


public class SingleToMultiplayer : MonoBehaviour
{

    // Assignació de valors per defecte
    public string Adress = "127.0.0.1"; // Valor per defecte: "127.0.0.1"
    public ushort Port = 7777; // Valor per defecte: 7777
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
            relayConfiguration();
            Debug.LogError(Protocol + "ITS RELAYYYY");
        }
        //unityTransportComponent.SetRelayServerData(new RelayServerData(allocation, "dtls"));




        //AFEGIR TOT LO CODI DE NETWORK CONNECT? 


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

    public async void relayConfiguration()
    {

        //Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
        //unityTransportComponent.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
        //       allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
    }


    public enum ProtocolType
    {
        UnityTransport = 0,
        RelayUnityTransport = 1
    }


}
