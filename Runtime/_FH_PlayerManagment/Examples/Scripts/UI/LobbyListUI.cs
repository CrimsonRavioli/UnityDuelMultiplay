using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;


public class LobbyListUI : PopupScreenData
{
    ScrollView scrollView;

    QueryLobbiesOptions _queryLobbiesOptions = new();

    Action<Lobby> _action;

    public LobbyListUI(Action<Lobby> action) : base( )
    {
        _queryLobbiesOptions.Count = 25;
        _queryLobbiesOptions.Filters = new System.Collections.Generic.List<QueryFilter>
        {
            new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
        };
        _action = action;
    }

    public override void BodySetUp(VisualElement body)
    {
        scrollView = new ScrollView();
        scrollView.AddToClassList("lobby-list-scrollview");
        body.Add(scrollView);
    }

    public override void FooterSetUp(VisualElement footer)
    {
        
    }

    public override string GetTitle()
    {
        return "Join Lobby";
    }

    public override void OnClosed()
    {
        
    }

    public override void OnOpened()
    {
        _ = RefreshLobbies();
    }

    public async Awaitable RefreshLobbies()
    {
        scrollView.Clear();
        var lobbys = await NetcodeUtilityManager.Instance.GetAvailableLobbies(_queryLobbiesOptions);
        foreach (Lobby lobby in lobbys)
        {
            LobbyDisplay lobbyDisplay = new LobbyDisplay(lobby, _action);
            
            scrollView.Add(lobbyDisplay);
        }
    }
}






public class LobbyDisplay : Button
{
    ScrollView scrollView;
    Lobby _lobby;
    Action<Lobby> _action;
    public LobbyDisplay(Lobby lobby, Action<Lobby> action)
    {
        _lobby = lobby;
        AddToClassList("lobby-display");
        Label nameLabel = new Label(lobby.Name);
        nameLabel.AddToClassList("lobby-name-label");
        Add(nameLabel);

        Label playersLabel = new Label($"{lobby.Players.Count}/{lobby.MaxPlayers}");
        playersLabel.AddToClassList("lobby-players-label");
        Add(playersLabel);

        Debug.Log($"Lobby: {lobby.Name}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}\n " +
        $"Code{lobby.LobbyCode},ID : {lobby.Id}");
        clicked += OnLobbySelected;
        _action = action;

        _action = action;
    }

    private void OnLobbySelected()
    {
        _action.Invoke(_lobby);
        // _ = LobbyManager.Instance.JoinLobbyById(_lobby.Id); 
    }
}
