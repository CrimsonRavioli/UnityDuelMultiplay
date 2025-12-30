using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance;

    [SerializeField] BaseSplitScreenBehaviour _splitScreenBehaviour;
    [SerializeField] List<BaceCameraLogicHandler> _cameraLogicHandler;

    const int Default_Camera_ID = 0;

    Dictionary<int, CameraData> camreras = new();

    List<CameraData> activeCams = new();
    List<CameraData> inactiveCams = new();
    List<BaseVesselController> following = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        RegisterCamera(new CameraData(Default_Camera_ID, Camera.main, (OutputChannels)1));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        InvokeRepeating(nameof(UpdateCameraLogic), .23f, 1f);
        camreras[Default_Camera_ID].SetCameraActive(true);
    }


    private void UpdateCameraLogic()
    {
        foreach (var handler in _cameraLogicHandler)
        {
            handler.UpdateCameraLogic(camreras);
        }
        UpdateSplitScreen();
    }


    private void UpdateSplitScreen()
    {
        switch (activeCams.Count) 
        {
            case 1:
                _splitScreenBehaviour.UpdateSplitScreen(activeCams[0]);
            break;
            case 2:
                _splitScreenBehaviour.UpdateSplitScreen(activeCams[0], activeCams[1]);
                break;
            case 3:
                _splitScreenBehaviour.UpdateSplitScreen(activeCams[0], activeCams[1], activeCams[2]);
                break;
            case 4:
                _splitScreenBehaviour.UpdateSplitScreen(activeCams[0], activeCams[1], activeCams[2], activeCams[3]);
                break;
            case 5:
                _splitScreenBehaviour.UpdateSplitScreen(activeCams[1], activeCams[2], activeCams[3], activeCams[4]);
                break;
            default:
                break;
        }
    }

  
    public void RegisterCamera(CameraData camera)
    {
        if (camreras.ContainsKey(camera.GetCameraID()))
        {
            return;
        }
        
        camera.OnTargetChange += camera_OnTargetChange;
        camera.OnCameraActiveUpdate += camera_OnCameraActiveUpdate; 

        camreras.Add(camera.GetCameraID(), camera);

        if (camera.GetIsActive())
        {
            activeCams.Add(camera);
        }
        else
        {
            inactiveCams.Add(camera);
        }
    }

    private void camera_OnCameraActiveUpdate(CameraData data, bool isActive)
    {

       

        if (isActive)
        {
            if (!activeCams.Contains(data))
            {
                activeCams.Add(data);
            }
            if (inactiveCams.Contains(data))
            {
                inactiveCams.Remove(data);
            }
        }
        else
        {
            if (!inactiveCams.Contains(data))
            {
                inactiveCams.Add(data);
            }
            if (activeCams.Contains(data))
            {
                activeCams.Remove(data);
            }
        }


        foreach (var handler in _cameraLogicHandler)
        {
            handler.camera_OnCameraActiveUpdate(data, isActive);
        }

    }

    private void camera_OnTargetChange(CameraData data,Transform previousTarget , Transform newTarget)
    {
        foreach (var handler in _cameraLogicHandler)
        {
            handler.camera_OnTargetChange(data,previousTarget, newTarget);
        }
    }
}



