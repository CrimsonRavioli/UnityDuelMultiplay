using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
partial class PlayerJoinUI : VisualElement, IDisposable
{
    [SerializeField] int MaxPlayers = 4;
    PlayerMenuElement nextPlayerMenuElement;
    List<PlayerMenuElement> playerMenus;

   

    public PlayerJoinUI()
    {
        playerMenus = new();

        AddToClassList("PlayerJoinUIContaner");


        nextPlayerMenuElement = new PlayerMenuElement();
        nextPlayerMenuElement.SetPlayerData(null);
        Add(nextPlayerMenuElement);

        PlayerEvents.OnPlayerJoined += PlayerJoined;
        PlayerEvents.OnPlayerLeft += PlayerLeft;
    }

    public void Dispose()
    {
        PlayerEvents.OnPlayerJoined -= PlayerJoined;
        PlayerEvents.OnPlayerLeft -= PlayerLeft;
    }

    private void PlayerJoined(PlayerData data)
    {
        
        nextPlayerMenuElement.SetPlayerData(data);
        playerMenus.Add(nextPlayerMenuElement);
        Add(nextPlayerMenuElement);

        if (playerMenus.Count < MaxPlayers) 
        {
            nextPlayerMenuElement = new PlayerMenuElement();
            nextPlayerMenuElement.SetPlayerData(null);
            Add(nextPlayerMenuElement);
        }
        else
        {
            nextPlayerMenuElement = null;
        }
    }

    private void PlayerLeft(PlayerData data) 
    {
        foreach (var playerMenu in playerMenus)
        {
            if (playerMenu != null && playerMenu.Equals(data))
            {
                playerMenus.Remove(nextPlayerMenuElement);
                Remove(nextPlayerMenuElement);
                playerMenu.SetPlayerData(null);
                nextPlayerMenuElement = playerMenu;
                break;
            }
        }
    }
}
