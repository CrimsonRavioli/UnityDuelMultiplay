using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UIElements;

public class LobbyDetailsScreen : PopupScreenData
{
    Button StartGameButton;
    VisualElement PlayersList;
  

    public LobbyDetailsScreen()
    {
       
    }

    public override void BodySetUp(VisualElement body)
    {
        PlayersList = new VisualElement();
        PlayersList.AddToClassList("players-list");
        body.Add(PlayersList);
    }

    public override void FooterSetUp(VisualElement footer)
    {
        return;

        StartGameButton = new Button();
        StartGameButton.text = "Start Game";
        StartGameButton.AddToClassList("start-game-button");
        StartGameButton.clicked += () => OnStartGamePressed();
        footer.Add(StartGameButton);
    }

    private void OnStartGamePressed()
    {
      
    }

    public override string GetTitle()
    {
     return "Lobby Details";
    }

    public override void OnClosed()
    {
        LobbyUtility.OnLobbyPlayerUpdate -= OnLobbyPlayersUpdated;

    }

    public override void OnOpened()
    {
        LobbyUtility.OnLobbyPlayerUpdate += OnLobbyPlayersUpdated;
        SetUpPlayerList();
    }

    private void OnLobbyPlayersUpdated(List<PlayerDisplayData> list)
    {
        UpdatePlayerList(list);
    }

    List<PlayerLobbyDisplay> currentPlayers = new List<PlayerLobbyDisplay>();

    public void SetUpPlayerList() 
    {
        UpdatePlayerList(NetcodeUtilityManager.Instance.GetCurrentPlayers());



    }


    public void UpdatePlayerList(List<PlayerDisplayData> playerDisplayDatas)
    {
        PlayersList.Clear();
        currentPlayers = new List<PlayerLobbyDisplay>();

        foreach (var player in playerDisplayDatas)
        {
            var display = new PlayerLobbyDisplay(player);
            PlayersList.Add(display);
            currentPlayers.Add(display);
        }
    }

}

public class PlayerLobbyDisplay : VisualElement
{
    public PlayerLobbyDisplay(PlayerDisplayData players)
    {
        AddToClassList("player-display");
        style.unityBackgroundImageTintColor = players.PlayerColor;
        Label nameLabel = new Label(players.PlayerName);
        nameLabel.AddToClassList("player-name-label");
        Add(nameLabel);

        UnityEngine.Debug.Log($"Player: {players.PlayerName}");
    }
}