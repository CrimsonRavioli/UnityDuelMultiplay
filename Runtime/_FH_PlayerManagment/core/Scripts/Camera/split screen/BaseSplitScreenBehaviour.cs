using UnityEngine;

public abstract class BaseSplitScreenBehaviour  : MonoBehaviour
{
    public abstract void UpdateSplitScreen(CameraData cameraA);
    public abstract void UpdateSplitScreen(CameraData cameraA, CameraData cameraB);
    public abstract void UpdateSplitScreen(CameraData cameraA, CameraData cameraB, CameraData cameraC);
    public abstract void UpdateSplitScreen(CameraData cameraA, CameraData cameraB, CameraData cameraC, CameraData cameraD);
} 
