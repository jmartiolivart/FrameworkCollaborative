/*
using UnityEngine;
using Unity.Netcode;
using System.Collections;

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
        StartCoroutine(WaitForVRRigReference());
    }

    private IEnumerator WaitForVRRigReference()
    {
        while (VRRigReferenceCurrent.Singleton == null)
        {
            Debug.LogError("Waiting for VRRigReferenceCurrent.Singleton to initialize...");
            yield return null; // Wait for one frame
        }
        Debug.Log("VRRigReferenceCurrent.Singleton initialized correctly");
        isVRRigReferenceInitialized = true;
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
        if (!isVRRigReferenceInitialized) return;

        if (IsOwner)
        {
            if (root == null || head == null || rightHand == null || leftHand == null)
            {
                Debug.LogError("One or more references are null in Update");
                return; // Exit Update to avoid errors
            }

            if (VRRigReferenceCurrent.Singleton == null)
            {
                Debug.LogError("VRRigReferenceCurrent.Singleton is null in Update");
                return; // Exit Update to avoid errors
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
*/