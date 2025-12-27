using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using WebSocketSharp;

public class NetcodeUtilityManager : MonoBehaviour
{

    public static NetcodeUtilityManager Instance;

    LobbyUtility _lobbyUtility;
    RelayUtility _relayUtility;

    bool isusingRelay = false;

    public UnityTransport unityTransport;
    public UnityTransport unityRelayTransport;

    private async void Awake()
    {
      //  SetUseRelay(false);

        _lobbyUtility = new LobbyUtility();
        _relayUtility = new RelayUtility();
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LobbyUtility.LobbyCallbacks.LobbyChanged += _lobbyUtility_OnLobbyChanged;

        

    }



    private void _lobbyUtility_OnLobbyChanged(ILobbyChanges changes)
    {
      
        Debug.Log("Lobby data changed.");
        // Check if the lobby data has changed and contains the Relay code
        if (changes.Data.Value != null && changes.Data.Value.TryGetValue("RelayCode", out var relayCodeValue))
        {
            var relayCode = relayCodeValue.Value.Value; // Access the value of RelayCode
            Debug.Log($"Relay code updated: {relayCode}");
            if (_lobbyUtility.GetIsHost())
            {
                // Host does not need to join the relay
                return;
            } 
          SetUseRelay(true);
          _ = _relayUtility.JoinRelay(relayCode);

        }
        else
        {
            Debug.Log("No Relay code found in lobby data.");
        }
    }

    private async void Start()
    {

        // Initialize Unity Services
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Sign in if not already signed in
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SignIn();
        }
    }
    public async void SignIn() 
    {  
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    internal async Task<IEnumerable<object>> GetAvailableLobbies(QueryLobbiesOptions queryLobbiesOptions)
    {
       return await _lobbyUtility.GetAvailableLobbies(queryLobbiesOptions);
    }

    internal void LeaveLobby()
    {
        _lobbyUtility.LeaveLobby();
    }

    internal async Task CreateLobby(LobbyCreationData data)
    {
        await _lobbyUtility.CreateLobby(data);

    }

    internal List<PlayerDisplayData> GetCurrentPlayers()
    {
        return _lobbyUtility.GetPlayers();
    }


    internal async Task JoinLobbyByID(string id)
    {
        await _lobbyUtility.JoinLobbyById(id);

        string code = _lobbyUtility.GetRelayCode();
        if (!code.IsNullOrEmpty()) 
        {
         //   _ = _relayUtility.JoinRelay(code);
        }
    }

    internal async Task StartGame()
    {
      
        if (_lobbyUtility.GetIsHost()) 
        {
            SetUseRelay(true);
            Debug.Log("Starting game...");
            string relayCode = await _relayUtility.CreateRelay(_lobbyUtility.GetNetworkPlayerCount() - 1);
            Debug.Log($"Starting realay {relayCode}");
            NetworkManager.Singleton.StartHost();
            await _lobbyUtility.SetRelayCode(relayCode);
        }
        else
        {
            Debug.LogWarning("Only the host can start the game.");
        }
    }

    public bool IsInLobby()
    {
        return _lobbyUtility.GetLobbyConnexionStatus() != LobbyConnexionStatus.None;
    }
    public LobbyConnexionStatus GetLobbyConnexionStatus()
    {
        return _lobbyUtility.GetLobbyConnexionStatus();
    }

    public void SetUseRelay(bool useRelay) 
    {
        isusingRelay = useRelay;

        if (isusingRelay) 
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
            unityTransport.enabled = true;
            unityRelayTransport.enabled = false;
        }
        else
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityRelayTransport;
            unityTransport.enabled = false;
            unityRelayTransport.enabled = true;
        }


    }


}
