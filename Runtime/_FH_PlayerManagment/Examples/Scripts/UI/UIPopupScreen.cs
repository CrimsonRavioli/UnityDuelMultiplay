using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class UIPopupScreen : VisualElement
{
    PopupScreenData windowData;

    protected Button _xClose;
    protected VisualElement _header;
    protected VisualElement _body;
    protected VisualElement _footer;

    Label _titleLabel;

    public UIPopupScreen()
    {
        AddToClassList("Popup");

        _header = new VisualElement();
        _header.AddToClassList("Popup-header");
        Add(_header);

        _xClose = new Button();
        _xClose.text = "";
        _xClose.AddToClassList("Popup-close-button");
        _xClose.clicked += Close;
        _header.Add(_xClose);

        _titleLabel = new Label();
        _titleLabel.AddToClassList("Popup-title");
        _header.Add(_titleLabel);


        _body = new VisualElement();
        _body.AddToClassList("Popup-body");
        Add(_body);
        _footer = new VisualElement();
        _footer.AddToClassList("Popup-footer");
        Add(_footer);

        Close();
    }

    void SetWindowData(PopupScreenData data)
    {
        if (windowData != null)
        {
            windowData.OnClosed();
        }
        _body.Clear();
        _footer.Clear();
        if (data == null)
        {
            return;
        }
        windowData = data;  
        _titleLabel.text = data.GetTitle();
        windowData.BodySetUp(_body);
        windowData.FooterSetUp(_footer);
        windowData.OnOpened();
    }


    public void Open(PopupScreenData data)
    {
        if (data == null)
        {
            Close();
            return;
        }
        if (windowData != data)
        {
            Debug.Log("Opening popup: " + data.GetTitle());
            SetWindowData(data);
        }

        
        style.display = DisplayStyle.Flex;
    }

    public void Close()
    {
        style.display = DisplayStyle.None;
        if (windowData != null)
        {
            windowData.OnClosed();
            windowData = null;
        }
    }
}


public abstract class PopupScreenData 
{
    public abstract void OnClosed();

    public abstract string GetTitle();
    public abstract void OnOpened();


    public abstract void BodySetUp(VisualElement body);

    public abstract void FooterSetUp(VisualElement footer);




}


