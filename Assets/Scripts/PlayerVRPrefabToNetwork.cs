using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections;

public class VRRigReference : MonoBehaviour
{
    public static VRRigReference Singleton;

    public Transform root;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            Debug.Log("VRRigReference Singleton inicialitzat");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

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
        if (GetComponent<NetworkObject>() == null)
        {
            gameObject.AddComponent<NetworkObject>();
        }

        if (GetComponent<NetworkTransform>() == null)
        {
            NetworkTransformClient networkTransformClient = GetComponent<NetworkTransformClient>();
            if (networkTransformClient == null)
            {
                networkTransformClient = gameObject.AddComponent<NetworkTransformClient>();
            }
            networkTransformClient.SyncScaleX = false;
            networkTransformClient.SyncScaleY = false;
            networkTransformClient.SyncScaleZ = false;
        }

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
        }
    }
}

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
        SyncScaleX = false;
        SyncScaleY = false;
        SyncScaleZ = false;
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
        if (VRRigReferenceCurrent.Singleton != null)
        {
            SetReferences(VRRigReferenceCurrent.Singleton.root, VRRigReferenceCurrent.Singleton.head, VRRigReferenceCurrent.Singleton.rightHand, VRRigReferenceCurrent.Singleton.leftHand, gameObjectsToDisable);
            isVRRigReferenceInitialized = true;
        }
    }

    void Update()
    {
        if (!isVRRigReferenceInitialized) return;

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
        this.root = root;
        this.head = head;
        this.rightHand = rightHand;
        this.leftHand = leftHand;
        this.gameObjectsToDisable = gameObjectsToDisable;
    }
}
