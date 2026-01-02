using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;


[RequireComponent(typeof(UIDocument))]
public class NetworkUI : MonoBehaviour
{

    UIDocument uiDocument;

    UIPopupScreen popupScreen;

    LobbyListUI lobbyListUI;

    LobbyCreationUI lobbyCreationUI;

    LobbyDetailsScreen detailsScreen;

    VisualElement _hostButtons, _joinedButtons ,_disconnectedButtons;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        LobbyUtility.OnLobbyConnexionStatusChanged += UpdateButtons;
        
      
    }

    private void HideUI()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void UpdateButtons(LobbyConnexionStatus status)
    {
        UpdateButtons();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientStarted += HideUI;
        lobbyListUI = new LobbyListUI(JoinLobby);
        lobbyCreationUI = new LobbyCreationUI(CreateLobby);
        detailsScreen = new LobbyDetailsScreen();

        var root = uiDocument.rootVisualElement;
        var menuButtons = root.Query<Button>().ToList();

        _hostButtons = root.Q<VisualElement>("Host");
        _joinedButtons = root.Q<VisualElement>("Joined");
        _disconnectedButtons = root.Q<VisualElement>("Offline");


        popupScreen = root.Q<UIPopupScreen>();

        if (popupScreen == null)
        {
            popupScreen = new UIPopupScreen();
            root.Add(popupScreen);
        }
        foreach (var button in menuButtons)
        {
           button.RegisterCallback<ClickEvent>(OnAnyButtonPressed);
        }

        UpdateButtons();
        ClosePopupWindow();
    }


    private void UpdateButtons() 
    {
        _hostButtons.style.display = DisplayStyle.None;
        _joinedButtons.style.display = DisplayStyle.None;
        _disconnectedButtons.style.display = DisplayStyle.None;


        switch (NetcodeUtilityManager.Instance.GetLobbyConnexionStatus())
        {
            case LobbyConnexionStatus.None:
                _disconnectedButtons.style.display = DisplayStyle.Flex;
                break;
            case LobbyConnexionStatus.Client:
                _joinedButtons.style.display = DisplayStyle.Flex;
                break;
            case LobbyConnexionStatus.Host:
                _hostButtons.style.display = DisplayStyle.Flex;
                break;
            default:
                break;
        }


    }
    
    


    private void OnAnyButtonPressed(ClickEvent clickEvent)
    {
        VisualElement target = clickEvent.currentTarget as VisualElement; 

        switch (target.name)
        {
            case "LocalGame":
                if (clickEvent.altKey)
                    StartLocalClient();
                else
                    StartLocalHost();
                ClosePopupWindow();
                break;
            case "HostLobby":
                OpenHostWindow();
                break;
            case "JoinLobby":
                OpenJoinWindow();
                break;
            case "ClosePopupButton":
                ClosePopupWindow();
            break;
                case "StartGame":
                StartGame(clickEvent);
                break;
                case "Disconnect":
                    break;
            case "ViewLobby":
                popupScreen.Open(detailsScreen);
                break;

            default:
                
            break;
        }
    }

    private void HostGame(ClickEvent clickEvent)
    {
        OpenHostWindow();
    }

    private void StartGame(ClickEvent clickEvent)
    {
        switch (NetcodeUtilityManager.Instance.GetLobbyConnexionStatus())
        {
            case LobbyConnexionStatus.None:
                if (clickEvent.altKey)
                    StartLocalClient();
                else
                    StartLocalHost();
                ClosePopupWindow();
                break;
            case LobbyConnexionStatus.Client:

                break;
            case LobbyConnexionStatus.Host:
                _ = NetcodeUtilityManager.Instance.StartGame();
                break;
            default:
                break;
        }



    }

    private void OpenHostWindow()
    {
        popupScreen.Open(lobbyCreationUI);
    }

    private void OpenJoinWindow()
    {
        popupScreen.Open(lobbyListUI);
    }

    void StartLocalHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    void StartLocalClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void ClosePopupWindow()
    {
       popupScreen.Close();
    }
    async void CreateLobby(LobbyCreationData data) 
    {
        await NetcodeUtilityManager.Instance.CreateLobby(data);
        popupScreen.Open(detailsScreen);
    }
    async void JoinLobby(Lobby join)
    {  
        await NetcodeUtilityManager.Instance.JoinLobbyByID(join.Id);
        popupScreen.Open(detailsScreen);
    }

}