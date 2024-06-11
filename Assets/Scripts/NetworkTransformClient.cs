/*using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

public class NetworkTransformClient : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
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
            Debug.LogError("Parent NetworkObject is null in OnNetworkObjectParentChanged");
            return;
        }

        base.OnNetworkObjectParentChanged(parentNetworkObject);
    }
}*/
