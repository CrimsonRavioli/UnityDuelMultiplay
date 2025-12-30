using System;
using UnityEngine;
using UnityEngine.UIElements;


partial class PlayerMenuElement: VisualElement
{
    protected PlayerData _playerData;
    Label PlayerNameLabel;
    DropdownField _characterField;

    public PlayerMenuElement()
    {
        AddToClassList("PlayerBox");
        
        


        PlayerNameLabel = new Label("PlayerNameLabel");
        PlayerNameLabel.AddToClassList("PlayerNameLabel");

        _characterField = new DropdownField("Character", new System.Collections.Generic.List<string>() { "thirdPerson", "FirstPerson" }, 0);
        _characterField.AddToClassList("CharacterField");
        _characterField.RegisterCallback<ChangeEvent<string>>((evt) =>
        {
            Debug.Log("Character changed to: " + evt.newValue);
            _playerData.SetValue("Character",_characterField.index);
        });

        _characterField.style.display = DisplayStyle.None;


        Add(PlayerNameLabel);
        Add(_characterField);
        SetUpPlaerBox(false);
    }

    public virtual void SetPlayerData(PlayerData playerData)
    {
        if (_playerData == playerData)
        { return; }
        if (_playerData != null)
        {
            _playerData.OnNameChanged -= OnPlayerNameChanged;
            _playerData.OnColorChanged -= OnPlayerColorChanged;
        }
        _playerData = playerData;

        if (playerData == null)
        {
            SetUpPlaerBox(false);
         
            return;
        }

       
        style.unityBackgroundImageTintColor = new StyleColor(playerData.GetPlayerDisplayData().PlayerColor);
        PlayerNameLabel.text = playerData.GetPlayerDisplayData().PlayerName;

        SetUpPlaerBox(true);
    }

 

    void SetUpPlaerBox(bool active)
    {
        if (active)
        {
            PlayerNameLabel.text = _playerData.GetPlayerDisplayData().PlayerName;
            RemoveFromClassList("Empty");
            AddToClassList("Occupied");
            _characterField.style.display = DisplayStyle.Flex;
        }
        else
        {
            _characterField.style.display = DisplayStyle.None;
            PlayerNameLabel.text = "Join ?";
            RemoveFromClassList("Occupied");
            AddToClassList("Empty");
        }
    }

    private void OnPlayerLeaveTimerChanged(float obj)
    {
       
    }

    private void OnPlayerColorChanged(Color color)
    {
        style.unityBackgroundImageTintColor = color;
    }

    private void OnPlayerNameChanged(string obj)
    {
        PlayerNameLabel.text = obj;
    }

}
