using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRigReferenceCurrent : MonoBehaviour
{
    public static VRRigReferenceCurrent Singleton;


    public Transform root;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    private void Awake()
    {
        Singleton = this;
    }
}
