using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class LocalPlayer : MonoBehaviour
{
    PlayerData _playerData;
    PlayerInput _playerInput;
    BaseVesselController _Vessle;

    [SerializeField] Camera _playerCamera;
    [SerializeField] float _gamepadLookSensitivityMultiplier  = 50f;
    CameraData _cameraData;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.onActionTriggered += OnActionTriggered;
        BaseVesselController.OnVesselOwnershipChanged += OnVesselOwnershipChanged;

    }

    private void OnVesselOwnershipChanged(BaseVesselController controller, Vector2Int controllerID)
    {
       Debug.Log($"Vessel {controller.name} ownership changed to Player {controllerID}");

        if (controller == _Vessle) 
        {
            Debug.Log($"Player {controllerID} released control of vessel {controller.name}");
            _Vessle = null;
        }

        if (controllerID == GetPlayerIndex())
        {
            Debug.Log($"Player {controllerID} took control of vessel {controller.name}");
            _Vessle = controller;
           _Vessle.CameraSetup(_cameraData);
        }
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {

     

        switch (context.action.name)
        {
            case "Move":
                Vector2 direction = context.ReadValue<Vector2>();
                Debug.Log($"Move Input: {direction}");
                _Vessle?.MoveInput(direction);
                break;
            case "Look":


                Vector2 delta = context.ReadValue<Vector2>();
                if (context.control.device.variants.Contains("Gamepad"))
                {
                    delta = delta * _gamepadLookSensitivityMultiplier;
                }
                _Vessle?.LookInput(delta);
                break;
            case "Jump":
                bool isJumping = context.performed;
                _Vessle?.JumpInput(isJumping);
                break;
            case "Interact":
                bool isUsing = context.performed;
                _Vessle?.InteractInput(isUsing);
                break;
            default:
                break;
        }
        debug = GetPlayerIndex();
    }

    private void SetIsLeaving(bool performed)
    {
        
    }

    public void SetPlayerData(PlayerData playerData)
    {
        int cameraIndex = playerData.PlayerIndex + 1;
        _playerData = playerData;
        

        switch (cameraIndex) 
        {
            case 1:
                _cameraData = new CameraData(cameraIndex, _playerCamera, (OutputChannels)2,true);
                break;
            case 2:
                _cameraData = new CameraData(cameraIndex, _playerCamera, (OutputChannels)4, true);
                break;
            case 3:
                _cameraData = new CameraData(cameraIndex, _playerCamera, (OutputChannels)8, true);
                break;
            case 4:
                _cameraData = new CameraData(cameraIndex, _playerCamera, (OutputChannels)16, true);
                break;
            default :
                _cameraData = new CameraData(cameraIndex, _playerCamera, (OutputChannels)0, true);
                break;

        }
      
        CameraManager.Instance.RegisterCamera(_cameraData);
    }

    [SerializeField] Vector2 debug;

    public Vector2Int GetPlayerIndex()
    {
        return new Vector2Int(NetworkPlayer.OwnerId, _playerData.PlayerIndex);
    }
}
