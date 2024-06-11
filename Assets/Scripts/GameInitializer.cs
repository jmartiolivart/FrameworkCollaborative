using UnityEngine;
using Unity.Netcode;

public class GameInitializer : MonoBehaviour
{
    public GameObject playerNetworkContainerPrefab; // Arrossega el prefab del PlayerNetworkContainer aquí

    private void Awake()
    {
        Debug.Log("GameInitializer Awake called");

        // Instanciar el PlayerNetworkContainer al començament del joc
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("NetworkManager is not null and is server");
            if (playerNetworkContainerPrefab != null)
            {
                GameObject playerNetworkContainer = Instantiate(playerNetworkContainerPrefab);
                playerNetworkContainer.name = "PlayerNetworkContainer"; // Assigna el nom correcte
                playerNetworkContainer.GetComponent<NetworkObject>().Spawn();
                Debug.Log("PlayerNetworkContainer instantiated and spawned");
            }
            else
            {
                Debug.LogError("playerNetworkContainerPrefab is not assigned in the inspector");
            }
        }
    }
}
