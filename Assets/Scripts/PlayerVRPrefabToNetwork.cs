using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerVRPrefabToNetwork : MonoBehaviour
{
    void Awake()
    {
        // Component NetworkObject
        if (GetComponent<NetworkObject>() == null)
        {
            gameObject.AddComponent<NetworkObject>();
        }

        // Component NetworkTransformClient
        NetworkTransformClient networkTransformClient = GetComponent<NetworkTransformClient>();
        if (networkTransformClient == null)
        {
            networkTransformClient = gameObject.AddComponent<NetworkTransformClient>();
        }
        // Treiem que s'hagi d'actualitzar la escala del GameObject ja que en principi no ha de canviar
        networkTransformClient.SyncScaleX = false;
        networkTransformClient.SyncScaleY = false;
        networkTransformClient.SyncScaleZ = false;

        // Component NetworkPlayer
        NetworkPlayer networkPlayer = GetComponent<NetworkPlayer>();
        if (networkPlayer == null)
        {
            networkPlayer = gameObject.AddComponent<NetworkPlayer>();
        }
    }
}

// La classe NetworkTransformClient amb el comportament personalitzat
public class NetworkTransformClient : NetworkTransform
{
    public bool SyncScaleX = false;
    public bool SyncScaleY = false;
    public bool SyncScaleZ = false;

    protected override bool OnIsServerAuthoritative()
    {
        return false; // Defineix que aquest NetworkTransform és client-authoritative
    }

    private void Awake()
    {
        // Assegura que les propietats tenen els valors desitjats
        SyncScaleX = false;
        SyncScaleY = false;
        SyncScaleZ = false;
    }
}

public class NetworkPlayer : NetworkBehaviour
{
    // Script del Player que s'enviarà les dades a traves la xarxa per veure els moviments
    [SerializeField]
    public Transform root;

    [SerializeField]
    public Transform head;

    [SerializeField]
    public Transform rightHand;

    [SerializeField]
    public Transform leftHand;

    [SerializeField]
    public GameObject[] gameObjectsToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Si és el propietari, per a que no dupliqui les mans borrem i el cap
        if (IsOwner)
        {
            foreach (var item in gameObjectsToDisable)
            {
                item.SetActive(false);
            }
        }
    }

    void Start()
    {
        // Pots afegir codi d'inicialització aquí si cal
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReference.Singleton.root.position;
            root.rotation = VRRigReference.Singleton.root.rotation;

            head.position = VRRigReference.Singleton.head.position;
            head.rotation = VRRigReference.Singleton.head.rotation;

            rightHand.position = VRRigReference.Singleton.rightHand.position;
            rightHand.rotation = VRRigReference.Singleton.rightHand.rotation;

            leftHand.position = VRRigReference.Singleton.leftHand.position;
            leftHand.rotation = VRRigReference.Singleton.leftHand.rotation;
        }
    }
}

public class VRRigReference : MonoBehaviour
{
    public static VRRigReference Singleton;

    public Transform root;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    private void Awake()
    {
        Singleton = this;
    }
}
