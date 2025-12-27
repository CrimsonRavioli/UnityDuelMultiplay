using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraData
{

    public Action<CameraData, Transform, Transform> OnTargetChange;
    public Action<CameraData,bool> OnCameraActiveUpdate;
    int _cameraID;
    Camera _camera;
    CinemachineBrain _brain;
    bool _isActive = true;
    bool _proximitysplitscreen; 
    Transform _target;
    OutputChannels _outputChannels;


    public CameraData(int cameraID, Camera camera, OutputChannels outputChannels, bool proximitysplitscreen = false)
    {
        _cameraID = cameraID;
        _camera = camera;
        _outputChannels = outputChannels; 
        _proximitysplitscreen = proximitysplitscreen;
       
        _brain = camera.GetComponent<CinemachineBrain>();

        SetTarget(null);
        SetCameraActive(false);
       
        _brain.ChannelMask = _outputChannels;

      
    }
    public void SetCameraActive(bool enabled)
    {
        OnCameraActiveUpdate?.Invoke(this, enabled);
        if (_isActive == enabled) return;

        _isActive = enabled;
        _camera.enabled = enabled;
        
        
        SetTarget(_target);
      

    }
    public void SetTarget(Transform target)
    {
        OnTargetChange?.Invoke(this,_target,target);
        _target = target;
    }

    public Camera GetCamera()
    {
        return _camera;
    }
    public int GetCameraID()
    {
        return _cameraID;
    }
    public bool GetIsActive()
    {
        return _isActive;
    }
    public bool GetIsProximitysplitscreen()
    {
        return _proximitysplitscreen;
    }

    internal void SetRect(float x, float y, float w, float h)
    {
        _camera.rect = new Rect(x, y,w,h);
    }
    public Transform GetTarget()
    {
        return _target;
    }

    internal OutputChannels getOutputChannel()
    {
        return _outputChannels;
    }

    internal void SetIsProximitysplitscreen(bool proximitysplitscreen)
    {
      _proximitysplitscreen = proximitysplitscreen;
    }
}