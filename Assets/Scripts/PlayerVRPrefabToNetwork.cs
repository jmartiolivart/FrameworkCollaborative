using Unity.Netcode;
using UnityEngine;

public class PlayerVRPrefabToNetwork : MonoBehaviour
{
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;
    public GameObject[] gameObjectsToDisable;
}

public class NetworkPlayer : NetworkBehaviour
{
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
        if (IsOwner)
        {
            // Desactivar objectes innecessaris per al jugador local
            foreach (GameObject obj in gameObjectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Només el propietari actualitza les transformacions

        // Sincronitzar la transformació del contenidor (PlayerNetworkContainer)
        transform.parent.position = VRRigReferenceCurrent.Singleton.root.position;
        transform.parent.rotation = VRRigReferenceCurrent.Singleton.root.rotation;

        // Actualitzar les transformacions locals del jugador (head, hands)
        head.position = VRRigReferenceCurrent.Singleton.head.position;
        head.rotation = VRRigReferenceCurrent.Singleton.head.rotation;

        rightHand.position = VRRigReferenceCurrent.Singleton.rightHand.position;
        rightHand.rotation = VRRigReferenceCurrent.Singleton.rightHand.rotation;

        leftHand.position = VRRigReferenceCurrent.Singleton.leftHand.position;
        leftHand.rotation = VRRigReferenceCurrent.Singleton.leftHand.rotation;
    }
}
