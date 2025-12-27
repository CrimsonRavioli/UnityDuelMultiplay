using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseVesselController : NetworkBehaviour
{

    public static Action<BaseVesselController, Vector2Int> OnVesselOwnershipChanged;

    Vector2Int ControllerID;

    public void SetControllerID(Vector2Int id)
    {
        ControllerID = id;
        OnVesselOwnershipChanged?.Invoke(this, ControllerID);
    }

    public abstract void CameraSetup(CameraData cameraData);

    public abstract void MoveInput(Vector2 direction);

    public abstract void LookInput(Vector2 delta);

    public abstract void JumpInput(bool isJumping);

    public abstract void InteractInput(bool isUsing);
 
}

