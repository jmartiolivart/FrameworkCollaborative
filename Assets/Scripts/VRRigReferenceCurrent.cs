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
        if (Singleton == null)
        {
            Singleton = this;
            Debug.Log("VRRigReferenceCurrent Singleton inicialitzat");
        }
        else
        {
            Debug.LogWarning("M�s d'una inst�ncia de VRRigReferenceCurrent trobada! Destru�nt aquesta inst�ncia.");
            Destroy(gameObject);
        }
    }
}
