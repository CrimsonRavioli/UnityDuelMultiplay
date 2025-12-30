using UnityEngine;

public interface IInteractable
{
    public void Interact(IInteractor interactor, bool IsInteracting );
}
public interface IInteractor 
{
    ulong GetClientId();
   


}
