using System;
using UnityEngine.UIElements;


public class LobbyCreationUI : PopupScreenData
{

    TextField lobbyNameField;
    SliderInt maxPlayersSlider;
    Toggle privateToggle;
    Button createButton;
    Action<LobbyCreationData> _onCreatLobby;
    public LobbyCreationUI(Action<LobbyCreationData> onCreatLobby) : base()
    {
        _onCreatLobby = onCreatLobby;
    }





    public override void BodySetUp(VisualElement body)
    {
        lobbyNameField = new TextField("Lobby Name:");
        lobbyNameField.AddToClassList("lobby-name-field");
        body.Add(lobbyNameField);

        privateToggle = new Toggle("Private Lobby:");
        privateToggle.AddToClassList("private-toggle");
        body.Add(privateToggle);


        maxPlayersSlider = new SliderInt("Max Players:", 1, 12);
        maxPlayersSlider.AddToClassList("max-players-slider");
        body.Add(maxPlayersSlider);

        createButton = new Button();
        createButton.text = "Create Lobby";
        createButton.AddToClassList("create-lobby-button");
        createButton.clicked += () => OnCreateButtonPressed();
        body.Add(createButton);

    }

    public override void FooterSetUp(VisualElement footer)
    {
        
    }

    public override string GetTitle()
    {
        return "Create Lobby";
    }

    public override void OnClosed()
    {
       
    }

    public override void OnOpened()
    {
       
    }

    private void OnCreateButtonPressed()
    {
        LobbyCreationData lobbyCreationData = new LobbyCreationData
        {
            lobbyName = lobbyNameField.text,
            maxPlayers = maxPlayersSlider.value,
            isPrivate = privateToggle.value
        };

        _onCreatLobby?.Invoke(lobbyCreationData);

    }
}
public class LobbyCreationData
{
    public string lobbyName;
    public int maxPlayers;
    public bool isPrivate;
}