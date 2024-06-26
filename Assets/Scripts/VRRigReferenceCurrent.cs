﻿using UnityEngine;

public class VRRigReferenceCurrent : MonoBehaviour
{
    public static VRRigReferenceCurrent Singleton;

    public Transform root;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Singleton = this;
        Debug.Log("VRRigReferenceCurrent Singleton inicialitzat");
    }
}