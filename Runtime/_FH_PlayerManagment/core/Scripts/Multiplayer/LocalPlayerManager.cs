using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerManager : MonoBehaviour
{
    [SerializeField] PlayerInputManager playerInputManager;


    public static LocalPlayerManager Instance;


    Dictionary<int, PlayerData> players = new();


    List<string> PlayerNameFist = new List<string>() {"Fred" , "Sam", "Steve","Shames","Rupet","Evel Bob","Bob!" } ;
    List<string> PlayerNameSecend= new List<string>() {"Dickhead","Herld", "The Destroyer", "Taler", "Jobs" };

    string GetRandomName() 
    {
        string first = PlayerNameFist[UnityEngine.Random.Range(0, PlayerNameFist.Count)];
        string second = PlayerNameSecend[UnityEngine.Random.Range(0, PlayerNameSecend.Count)];
        return first + " " + second;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

    }


    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += PlayerInputManager_onPlayerJoined;
        playerInputManager.onPlayerLeft += PlayerInputManager_onPlayerLeft;
    }
    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= PlayerInputManager_onPlayerJoined;
        playerInputManager.onPlayerLeft -= PlayerInputManager_onPlayerLeft;
    }

    private void PlayerInputManager_onPlayerLeft(PlayerInput input)
    {
        LocalPlayer player = input.GetComponent<LocalPlayer>(); 
        PlayerEvents.PlayerLeft(players[input.playerIndex]);
        players.Remove(input.playerIndex);
       
    }

    private void PlayerInputManager_onPlayerJoined(PlayerInput input)
    {
        LocalPlayer player = input.GetComponent<LocalPlayer>();
        player.transform.parent = gameObject.transform;
        Color color = UnityEngine.Random.ColorHSV();
        color.a = 1;
        PlayerData playerData = new PlayerData(player,input.playerIndex, GetRandomName(), color);
        player.SetPlayerData(playerData);
        PlayerEvents.PlayerJoined(playerData);
        players.Add(input.playerIndex, playerData);
    }

    public List<PlayerData> LocalPlayerDatas()
    {
         return new List<PlayerData>(players.Values);
    }

    public void SetCanJoin(bool canJoin)
    {
        if (canJoin) 
        {
         playerInputManager.EnableJoining();
        }
        else 
        {
        playerInputManager.DisableJoining();
        }
    }

}
