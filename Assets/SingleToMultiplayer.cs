using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class SingleToMultiplayer : MonoBehaviour
{

    // Assignació de valors per defecte
    public string Adress = "127.0.0.1"; // Valor per defecte: "127.0.0.1"
    public ushort Port = 7777; // Valor per defecte: 7777
    public GameObject Jugador;

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

        
        //AFEGIR TOT LO CODI DE NETWORK CONNECT? (i fico per automatitzar en plan WAY no té a veure amb NETCODE al codi ho fico)




    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private UnityTransport settingUpUnityTransport(UnityTransport unityTransportComponent)
    {
        unityTransportComponent.ConnectionData.Address = Adress;
        unityTransportComponent.ConnectionData.Port = Port;

        //AFEGIR LA m_ProtocolType? per si es relay o el altre?

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
}
