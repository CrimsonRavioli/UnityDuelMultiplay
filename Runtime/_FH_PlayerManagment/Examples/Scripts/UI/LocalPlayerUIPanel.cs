using System;
using UnityEngine;
using UnityEngine.UIElements;

internal class LocalPlayerUIPanel : IDisposable
{
    VisualElement _container;
    PlayerData _localPlayerData;

    Label _nameLabel;
    VisualElement _background;



    public LocalPlayerUIPanel(VisualElement container, PlayerData localPlayerData) 
    {
        _container = container;
        _localPlayerData = localPlayerData;

        GatherReferences();
        UpdateVisual(localPlayerData.GetPlayerDisplayData());
        RegisterEvents();
    }
    void UpdateVisual(PlayerDisplayData displayData)
    {
        UpdateName(displayData.PlayerName);
        UpdateBackground(displayData.PlayerColor);
    }


    void GatherReferences()
    {
        _nameLabel = _container.Q<Label>("PlayName");
        _background = _container.Q<VisualElement>("PlayerElement");
    }

    void RegisterEvents() 
    {
        _localPlayerData.OnNameChanged += UpdateName;
        _localPlayerData.OnColorChanged += UpdateBackground;

    }
    void UnRegisterEvents()
    {
        _localPlayerData.OnNameChanged -= UpdateName;
        _localPlayerData.OnColorChanged -= UpdateBackground;
    }

    public void UpdateName(string newName)
    {
        _nameLabel.text = newName;
    }
    private void UpdateBackground(Color playerColor)
    {
        _background.style.unityBackgroundImageTintColor = playerColor;
    }

    public void Dispose()
    {
        UnRegisterEvents();
        _container.parent.Remove(_container);
    }
}