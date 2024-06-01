using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections;

using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections;

public class PlayerVRPrefabToNetwork : MonoBehaviour
{
    public Transform root;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;
    public GameObject[] gameObjectsToDisable;

    private NetworkPlayer networkPlayer;

    void Awake()
    {
        Debug.Log("PlayerVRPrefabToNetwork Awake");

        // Assegurar-nos que el prefab té NetworkObject
        if (GetComponent<NetworkObject>() == null)
        {
            Debug.Log("Afegint NetworkObject");
            gameObject.AddComponent<NetworkObject>();
        }

        // Substituir NetworkTransform per NetworkTransformClient
        NetworkTransformClient networkTransformClient = GetComponent<NetworkTransformClient>();
        if (networkTransformClient == null)
        {
            Debug.Log("Substituint NetworkTransform per NetworkTransformClient");
            NetworkTransform networkTransform = GetComponent<NetworkTransform>();
            if (networkTransform != null)
            {
                DestroyImmediate(networkTransform);
            }
            networkTransformClient = gameObject.AddComponent<NetworkTransformClient>();
            networkTransformClient.SyncScaleX = false;
            networkTransformClient.SyncScaleY = false;
            networkTransformClient.SyncScaleZ = false;
        }

        // Afegir NetworkPlayer
        networkPlayer = GetComponent<NetworkPlayer>();
        if (networkPlayer == null)
        {
            networkPlayer = gameObject.AddComponent<NetworkPlayer>();
        }

        StartCoroutine(WaitForVRRigReference());
    }

    private IEnumerator WaitForVRRigReference()
    {
        while (VRRigReferenceCurrent.Singleton == null)
        {
            Debug.Log("Esperant a que VRRigReferenceCurrent.Singleton sigui inicialitzat...");
            yield return null; // Espera un frame
        }

        Debug.Log("VRRigReferenceCurrent.Singleton inicialitzat correctament");

        if (root == null) root = VRRigReferenceCurrent.Singleton.root;
        if (head == null) head = VRRigReferenceCurrent.Singleton.head;
        if (rightHand == null) rightHand = VRRigReferenceCurrent.Singleton.rightHand;
        if (leftHand == null) leftHand = VRRigReferenceCurrent.Singleton.leftHand;

        if (root == null || head == null || rightHand == null || leftHand == null)
        {
            Debug.LogError("Una o més referències no estan assignades correctament després de la inicialització de VRRigReferenceCurrent.");
        }
        else
        {
            networkPlayer.SetReferences(root, head, rightHand, leftHand, gameObjectsToDisable);
            Debug.Log("Referències de VRRigReference assignades correctament");
        }
    }
}

public class NetworkTransformClient : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false; // Defineix que aquest NetworkTransform és client-authoritative
    }

    private void Awake()
    {
        SyncScaleX = false;
        SyncScaleY = false;
        SyncScaleZ = false;
    }

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (parentNetworkObject == null)
        {
            Debug.LogError("Parent NetworkObject és null a OnNetworkObjectParentChanged");
            return;
        }

        Debug.Log("OnNetworkObjectParentChanged: Parent NetworkObject no és null");
        base.OnNetworkObjectParentChanged(parentNetworkObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("NetworkTransformClient OnNetworkSpawn");
    }
}


public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private Transform root;

    [SerializeField]
    private Transform head;

    [SerializeField]
    private Transform rightHand;

    [SerializeField]
    private Transform leftHand;

    [SerializeField]
    private GameObject[] gameObjectsToDisable;

    private bool isVRRigReferenceInitialized = false;

    private void Start()
    {
        Debug.Log("NetworkPlayer Start");
        StartCoroutine(WaitForVRRigReference());
    }

    private IEnumerator WaitForVRRigReference()
    {
        while (VRRigReferenceCurrent.Singleton == null)
        {
            Debug.LogError("Esperant a que VRRigReferenceCurrent.Singleton sigui inicialitzat...");
            yield return null; // Espera un frame
        }

        Debug.Log("VRRigReferenceCurrent.Singleton inicialitzat correctament");
        isVRRigReferenceInitialized = true;

        if (root == null || head == null || rightHand == null || leftHand == null)
        {
            Debug.LogError("Una o més referències són null a WaitForVRRigReference");
            yield break;
        }

        Debug.Log("Referències de VRRigReference assignades a NetworkPlayer");
    }

    void Update()
    {
        if (!isVRRigReferenceInitialized)
        {
            Debug.LogWarning("VRRigReference no està inicialitzat");
            return;
        }

        if (IsOwner)
        {
            if (root == null || head == null || rightHand == null || leftHand == null)
            {
                Debug.LogError("Una o més referències són null a Update");
                return; // Sortim de la funció Update per evitar l'error
            }

            root.position = VRRigReferenceCurrent.Singleton.root.position;
            root.rotation = VRRigReferenceCurrent.Singleton.root.rotation;

            head.position = VRRigReferenceCurrent.Singleton.head.position;
            head.rotation = VRRigReferenceCurrent.Singleton.head.rotation;

            rightHand.position = VRRigReferenceCurrent.Singleton.rightHand.position;
            rightHand.rotation = VRRigReferenceCurrent.Singleton.rightHand.rotation;

            leftHand.position = VRRigReferenceCurrent.Singleton.leftHand.position;
            leftHand.rotation = VRRigReferenceCurrent.Singleton.leftHand.rotation;
        }
    }

    public void SetReferences(Transform root, Transform head, Transform rightHand, Transform leftHand, GameObject[] gameObjectsToDisable)
    {
        Debug.Log("SetReferences called in NetworkPlayer");
        this.root = root;
        this.head = head;
        this.rightHand = rightHand;
        this.leftHand = leftHand;
        this.gameObjectsToDisable = gameObjectsToDisable;
    }
}