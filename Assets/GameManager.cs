using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign this in the inspector
    public Transform spawnPoint;    // Assign a spawn point transform in the inspector

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(SpawnPlayer(clientId));
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        // Ensure NetworkManager is fully initialized
        yield return null; // Wait for one frame 

        var playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Ensure the player has a NetworkObject
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("Player prefab is missing a NetworkObject component!");
            yield break; // Exit coroutine if NetworkObject is missing
        }

        // Spawn the player as a NetworkObject
        networkObject.SpawnAsPlayerObject(clientId, true);

        Debug.Log($"Player spawned with Client ID: {clientId}");
    }
}
