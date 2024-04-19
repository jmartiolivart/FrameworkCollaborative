using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class NetworkConnect : MonoBehaviour
{
    // Classe per decidir si crear la sala o unir-se a una.
    public int maxConnection = 10;
    public UnityTransport transport;

    private Lobby currentLobby;
    private float timerPing;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinOrCreate();

    }

    public async void JoinOrCreate()
    {
        try
        {
            currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = currentLobby.Data["JOIN_KEY"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);


            NetworkManager.Singleton.StartClient();
        }
        catch
        {
            Create();
        }
    }

    public async void Create()
    {

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.LogError(joinCode);

        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
        lobbyOptions.IsPrivate = false;
        lobbyOptions.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, joinCode);
        lobbyOptions.Data.Add("JOIN_KEY", dataObject);

        currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);

        NetworkManager.Singleton.StartHost();
    }



    private void Update()
    {
        if (timerPing > 15)
        {
            timerPing = 0;
            if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
            timerPing += Time.deltaTime;
        }
    }
}
