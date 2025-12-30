using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProximitySplitScreenHandler : BaceCameraLogicHandler
{
    [SerializeField] float SplitDistance = 0f;
    [SerializeField] CinemachineTargetGroup targetGroup;
    CameraData mainCameraData;

    bool IsScreenSplit = false;
    bool CanSplitScreen = false;

    List<Transform> _targets = new();



    public override void camera_OnCameraActiveUpdate(CameraData data, bool isActive)
    {

        if (!_targets.Contains(data.GetTarget()))
        {
            return;
        }

        foreach (var t in targetGroup.Targets)
        {
            if (t.Object == data.GetTarget())
            {
                if (!isActive)
                {
                    t.Weight = .1f;
                }
                else
                {
                    t.Weight = 1f;
                }
            }
        }
    }

    public override void camera_OnTargetChange(CameraData data, Transform previousTarget, Transform newTarget)
    {
        if (!data.GetIsProximitysplitscreen())
        {
            return;
        }
        UpdateTargetGroup(previousTarget, false);
        UpdateTargetGroup(newTarget, true);
    }



    void UpdateTargetGroup(Transform transform, bool add)
    {
        if (targetGroup == null) { return; }
        if (transform == null) { return; }


        if (add)
        {
            _targets.Add(transform);
            targetGroup.AddMember(transform, 1f, 2f);
        }
        else
        {
            _targets.Remove(transform);
            targetGroup.RemoveMember(transform);
        }
    }


    public override void UpdateCameraLogic(Dictionary<int, CameraData> camreras)
    {
        if (SplitDistance <= 0) { return; }

        mainCameraData = camreras[0];

        int activeed = 0;
        List<CameraData> ProximityCameras = new(); 

        bool outsideCam = false;

        foreach (CameraData cameraData in camreras.Values)
        {
            if (cameraData == mainCameraData)
            {
                continue;
            }
            if (cameraData.GetIsProximitysplitscreen())
            {
                ProximityCameras.Add(cameraData);
            }
            else
            {
                outsideCam = true;
            }
        }


        if (ProximityCameras.Count == 0) 
        {
            SetIsScreenSplit(outsideCam);
            return;
        }
        if (ProximityCameras.Count == 1)
        {
            ProximityCameras[0].SetCameraActive(true);
            SetIsScreenSplit(true);
            return;
        }




        foreach (CameraData cameraData in ProximityCameras)
        {
            float distance = 0f;
            if (cameraData.GetTarget() != null)
            {
                distance = Vector3.Distance(cameraData.GetTarget().position, targetGroup.transform.position);
            }
            if (distance <= SplitDistance)
            {
                cameraData.SetCameraActive(false);
            }
            else
            {
                activeed++;
                cameraData.SetCameraActive(true);
            }
        }
      
        SetIsScreenSplit(activeed > 0);
    }


    void SetIsScreenSplit(bool isSplit)
    {
        mainCameraData.SetCameraActive(!isSplit);
    }


}




