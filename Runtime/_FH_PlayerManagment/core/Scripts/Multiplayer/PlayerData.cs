
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerData
{ 
    public LocalPlayer LocalPlayInput;
    public Action<string> OnNameChanged { get; internal set; }
    public Action<Color> OnColorChanged { get; internal set; }
    PlayerDisplayData playerDisplayData = new PlayerDisplayData();

    Dictionary<string, object> playerValues = new Dictionary<string, object>();
    public int PlayerIndex;


    public PlayerData(LocalPlayer playerInputint, int playerIndex, string playerName, Color playerColor)
    {
        LocalPlayInput = playerInputint;
        SetPlayerName(playerName);
        SetPlayerColor(playerColor);
        PlayerIndex = playerIndex;
    }
    public void SetPlayerName(string newName)
    {
        playerDisplayData.PlayerName = newName;
        OnNameChanged?.Invoke(newName);
    }
    public void SetPlayerColor(Color newColor)
    {
        playerDisplayData.PlayerColor = newColor;
        OnColorChanged?.Invoke(newColor);
    }

    internal Vector2Int GetPlayerID()
    {
        return LocalPlayInput.GetPlayerIndex();
    }

    public PlayerDisplayData GetPlayerDisplayData()
    {
        return playerDisplayData;
    }

    internal void SetValue(string key, int Value)
    {
        if (playerValues.ContainsKey(key))
        {
            playerValues[key] = Value;
        }
        else
        {
            playerValues.Add(key, Value);
        }
    }
    public T GetValue<T>(string key)
    {
        if (playerValues.ContainsKey(key))
        {
            return (T)playerValues[key];
        }
        return default;
    }
}

[Serializable]
public class PlayerDisplayData
{
    public string PlayerName;
    public Color PlayerColor;
}


public static class PlayerEvents
{
    public static Action<PlayerData> OnPlayerJoined;
    public static Action<PlayerData> OnPlayerLeft;

    static List<PlayerData> CurrentPlayers = new();
    public static void PlayerJoined(PlayerData playerData)
    {
       if (CurrentPlayers.Contains(playerData)) 
       {
            return;
       }
       CurrentPlayers.Add(playerData);
       OnPlayerJoined?.Invoke(playerData);
    }
    public static void PlayerLeft(PlayerData playerData)
    {

        if (!CurrentPlayers.Contains(playerData)) 
        {
            return;
        }
        CurrentPlayers.Remove(playerData);
        OnPlayerLeft?.Invoke(playerData);
    }
    public static List<PlayerData> GetCurrentPlayers() 
    {
        return CurrentPlayers;
    }
    
}