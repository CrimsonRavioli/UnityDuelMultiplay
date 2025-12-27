using UnityEngine;
using UnityEngine.UIElements;

public class CircleTimerUi: VisualElement, INotifyValueChanged<float>
{
    // The filled portion
    private VisualElement _fillElement;

    // Internal backing field
    private float _value;

    // Exposed value [0..1]
    public float value
    {
        get => _value;
        set
        {
            SetValueWithoutNotify(value);

            // Notify listeners if changed
            using var evt = ChangeEvent<float>.GetPooled(_value, value);
            evt.target = this;
            SendEvent(evt);
        }
    }

    public void SetValueWithoutNotify(float newValue)
    {
        newValue = Mathf.Clamp01(newValue);

        if (Mathf.Approximately(_value, newValue))
            return;

        _value = newValue;

        // Update the fill bar width
        if (_fillElement != null)
        {
            _fillElement.style.width = Length.Percent(_value * 100f);
        }
    }

    public CircleTimerUi()
    {
        // Style self
        AddToClassList("timer-bar");

        // Create the fill element
        _fillElement = new VisualElement();
        _fillElement.AddToClassList("timer-bar__fill");
        hierarchy.Add(_fillElement);

        // Default state
        SetValueWithoutNotify(0);
    }
}
