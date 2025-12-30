using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUtility
{
    private static Lobby _CurentLobby;
    public static readonly LobbyEventCallbacks LobbyCallbacks = new LobbyEventCallbacks();
    private bool _isHeartbeatActive = false;
    private LobbyConnexionStatus _connexionStatus = LobbyConnexionStatus.None;
    public static Action<LobbyConnexionStatus> OnLobbyConnexionStatusChanged;




    public LobbyUtility()
    {
        LobbyCallbacks.LobbyChanged += LobbyCallbacks_LobbyChanged;


        PlayerEvents.OnPlayerJoined += UpdateLocalPlayers;
        PlayerEvents.OnPlayerLeft += UpdateLocalPlayers;
        _ = HartBeat();
       // _ = PullLobbyUpdates();
    }



    #region heartbeat 

    public async Awaitable HartBeat()
    {
        while (true)
        {
            await Task.Delay(15000);
            if (_isHeartbeatActive)
            {
                SendHeartbeat();
            }
        }
    }
    private async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_CurentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Heartbeat failed: {e.Message}");
            _isHeartbeatActive = false;
        }
    }

    #endregion

    #region PullLobbyUpdates

    public async Awaitable PullLobbyUpdates()
    {
        while (true)
        {
            await Task.Delay(3500);
            if (_CurentLobby != null)
            {
                try
                {
                   Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(_CurentLobby.Id);
                   UpdateCurentLobby(updatedLobby);
                
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Failed to pull lobby updates: {e.Message}");
                }
                Debug.Log("Pulled lobby updates");
            }
        }
    }


    #endregion

    Dictionary<string, PlayerDataObject> MakePlayerData()
    {
        var newPlayerData = new Dictionary<string, PlayerDataObject>();
        newPlayerData.Add("Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId));

        foreach (var item in PlayerEvents.GetCurrentPlayers())
        {
            string key = $"Player_{item.PlayerIndex}_Data";
            string value = JsonUtility.ToJson(item.GetPlayerDisplayData());
            newPlayerData.Add(key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, value));
        }

        return newPlayerData;
    }


    #region create/join/leave lobby

    public async Task CreateLobby(LobbyCreationData data)
    {
        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = data.isPrivate,
            Player = new Player { Data = MakePlayerData()},
            Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "")}
                }
        };
        SetLobbyConnexionStatus(LobbyConnexionStatus.Connecting);

        UpdateCurentLobby(await LobbyService.Instance.CreateLobbyAsync(data.lobbyName, data.maxPlayers, lobbyOptions));


        SetLobbyConnexionStatus(LobbyConnexionStatus.Host);
        _isHeartbeatActive = true;
    }
    public async Task JoinLobbyByCode(string lobbyCode)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = new Player
            {
                Data = MakePlayerData()
            }
        };
        SetLobbyConnexionStatus(LobbyConnexionStatus.Connecting);
        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
        SetLobbyConnexionStatus(LobbyConnexionStatus.Client);
        Debug.Log($"Joined lobby: {_CurentLobby.Name}");
    }
  
    public async Task JoinLobbyById(string lobbyId)
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
        {
            Player = new Player
            {
                Data = MakePlayerData()
            }
        };
        SetLobbyConnexionStatus(LobbyConnexionStatus.Connecting);
        UpdateCurentLobby(await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options));
        _connexionStatus = LobbyConnexionStatus.Client;
    
        SetLobbyConnexionStatus(LobbyConnexionStatus.Client);
        Debug.Log($"Joined lobby: {_CurentLobby.Name}");
    }
    public void LeaveLobby()
    {
        try
        {
            if (_CurentLobby != null)
            {
                if (_CurentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    // If we're the host, delete the lobby
                    LobbyService.Instance.DeleteLobbyAsync(_CurentLobby.Id);
                    Debug.Log("Deleted lobby as host");
                }
                else
                {
                    // If we're a player, just leave
                    LobbyService.Instance.RemovePlayerAsync(_CurentLobby.Id, AuthenticationService.Instance.PlayerId);
                    Debug.Log("Left lobby as player");
                }
                UpdateCurentLobby(null);
                _isHeartbeatActive = false;
                // Disconnect from Netcode
                if (NetworkManager.Singleton != null)
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error leaving lobby: {e.Message}");
        }
    }
    private async void UpdateCurentLobby(Lobby updatedLobby)
    {
        _CurentLobby = updatedLobby;

        if (_CurentLobby != null)
        {
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(_CurentLobby.Id, LobbyCallbacks);
        }
    }
    #endregion

    #region playerManagement 

    public void UpdateLocalPlayers(PlayerData playerData)
    {
        if (_CurentLobby == null) { return; }
        var newPlayerData = new Dictionary<string, PlayerDataObject>();
        newPlayerData.Add("Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId));
        foreach (var item in PlayerEvents.GetCurrentPlayers())
        {
            string key = $"Player_{item.PlayerIndex}_Data";
            string value = JsonUtility.ToJson(item.GetPlayerDisplayData());
            newPlayerData.Add(key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, value));
        }

        var updateOptions = new UpdatePlayerOptions
        {
            Data = newPlayerData
        };

        LobbyService.Instance.UpdatePlayerAsync(_CurentLobby.Id, AuthenticationService.Instance.PlayerId, updateOptions);
    }
    public bool GetIsHost()
    {
        if (_CurentLobby != null)
        {
            return _CurentLobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        return false;
    }

    List<PlayerDisplayData> playerDisplayDatas;
    public static Action<List<PlayerDisplayData>> OnLobbyPlayerUpdate;
    public List<PlayerDisplayData> GetPlayers()
    {
        UpdatePlayers();
        return playerDisplayDatas;
    }
    public void UpdatePlayers()
    {
        playerDisplayDatas = new List<PlayerDisplayData>();
        foreach (var player in _CurentLobby.Players)
        {
            for (int i = 0; i < 4; i++)
            {
                string key = $"Player_{i}_Data";
                if (player.Data.TryGetValue(key, out PlayerDataObject playerDataObject))
                {
                    string playerDataJson = playerDataObject.Value;
                    PlayerDisplayData displayData = JsonUtility.FromJson<PlayerDisplayData>(playerDataJson);
                    playerDisplayDatas.Add(displayData);
                }
            }
        }
        OnLobbyPlayerUpdate?.Invoke(playerDisplayDatas);

    }

    #endregion

    #region LobbyQuerying 
    public async Task<List<Lobby>> GetAvailableLobbies(QueryLobbiesOptions queryLobbiesOptions = null)
    {
        try
        {
            // Use default options if none provided
            if (queryLobbiesOptions == null)
            {
                queryLobbiesOptions = new QueryLobbiesOptions
                {
                    Count = 5,
                };
            }
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Failed to query lobbies: {e.Message}");
            return new List<Lobby>();
        }
    }


    public LobbyConnexionStatus GetLobbyConnexionStatus()
    {
        return _connexionStatus;
    }

    void SetLobbyConnexionStatus(LobbyConnexionStatus status)
    {
        _connexionStatus = status;
        OnLobbyConnexionStatusChanged?.Invoke(_connexionStatus);
    }



    #endregion

    #region Events

    private void LobbyCallbacks_LobbyChanged(ILobbyChanges changes)
    {
        if (changes == null) { return; }
        if (_CurentLobby == null) { return; }
        changes.ApplyToLobby(_CurentLobby);
        UpdatePlayers();
    }


    #endregion

    #region relayCodeManagement


    public string GetRelayCode()
    {
            if (_CurentLobby.Data.TryGetValue("RelayCode", out DataObject relayCodeData))
            {
                return relayCodeData.Value;
        }
            else
            {
               return string.Empty;
        }
       
    }





    internal async Task SetRelayCode(string code)
    {
        if (_CurentLobby != null)
        {
            var updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, code)}
                }
            };

            await LobbyService.Instance.UpdateLobbyAsync(_CurentLobby.Id, updateOptions);
        }

    }

    internal int GetNetworkPlayerCount()
    {
        if (_CurentLobby != null)
        {
            return _CurentLobby.MaxPlayers;
        }
        return 0;
    }

    #endregion

}

public enum LobbyConnexionStatus
{
    None,Connecting,Host,Client
}