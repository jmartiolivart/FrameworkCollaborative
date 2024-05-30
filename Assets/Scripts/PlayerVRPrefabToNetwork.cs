using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

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
            Debug.LogWarning("Més d'una instància de VRRigReference trobada! Destruïnt aquesta instància.");
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

        NetworkPlayer networkPlayer = GetComponent<NetworkPlayer>();
        if (networkPlayer == null)
        {
            networkPlayer = gameObject.AddComponent<NetworkPlayer>();
        }

        if (root != null && head != null && rightHand != null && leftHand != null && gameObjectsToDisable != null)
        {
            networkPlayer.SetReferences(root, head, rightHand, leftHand, gameObjectsToDisable);
        }
        else
        {
            Debug.LogError("Una o més referències no estan assignades en el prefab Jugador.");
            if (root == null) Debug.LogError("root és null");
            if (head == null) Debug.LogError("head és null");
            if (rightHand == null) Debug.LogError("rightHand és null");
            if (leftHand == null) Debug.LogError("leftHand és null");
            if (gameObjectsToDisable == null) Debug.LogError("gameObjectsToDisable és null");
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

    private void Start()
    {
        StartCoroutine(WaitForVRRigReference());
    }

    private IEnumerator WaitForVRRigReference()
    {
        while (VRRigReference.Singleton == null)
        {
            Debug.LogError("Esperant a que VRRigReference.Singleton sigui inicialitzat...");
            yield return null; // Espera un frame
        }
        Debug.Log("VRRigReference.Singleton inicialitzat correctament");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            foreach (var item in gameObjectsToDisable)
            {
                item.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (root == null || head == null || rightHand == null || leftHand == null)
            {
                Debug.LogError("Una o més referències són null a Update");
                if (root == null) Debug.LogError("root és null");
                if (head == null) Debug.LogError("head és null");
                if (rightHand == null) Debug.LogError("rightHand és null");
                if (leftHand == null) Debug.LogError("leftHand és null");
                return; // Sortim de la funció Update per evitar l'error
            }

            if (VRRigReference.Singleton == null)
            {
                Debug.LogError("VRRigReference.Singleton és null a Update");
                return; // Sortim de la funció Update per evitar l'error
            }

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

    public void SetReferences(Transform root, Transform head, Transform rightHand, Transform leftHand, GameObject[] gameObjectsToDisable)
    {
        this.root = root;
        this.head = head;
        this.rightHand = rightHand;
        this.leftHand = leftHand;
        this.gameObjectsToDisable = gameObjectsToDisable;
    }
}
