using System;
using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionEvent onInteract;

    public void Interact(IInteractor interactor, bool IsInteracting)
    {
        Debug.Log($"{interactor} interacted with {this}");
        onInteract?.Invoke(interactor, IsInteracting);
    }

}

[System.Serializable]
public class InteractionEvent: UnityEvent<IInteractor, bool> { } 
