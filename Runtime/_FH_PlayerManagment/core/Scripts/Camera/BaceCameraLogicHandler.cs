using System.Collections.Generic;
using UnityEngine;

public abstract class BaceCameraLogicHandler : MonoBehaviour
{
    public abstract void UpdateCameraLogic(Dictionary<int, CameraData> camreras);
    public abstract void camera_OnTargetChange(CameraData data, Transform previousTarget, Transform newTarget);
    public abstract void camera_OnCameraActiveUpdate(CameraData data, bool isActive);

}



