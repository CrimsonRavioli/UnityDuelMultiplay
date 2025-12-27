using Unity.Cinemachine;
using UnityEngine;

public class FirstPersonVessel : BaseVesselController
{

    [Header("References")]
    [SerializeField] ClientNetworkAnimator _animator;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] CinemachineCamera _cinemachineCamera;

   

    [Header("Stats")]
    [SerializeField] float _moveSpeed = 5f;
 
    [SerializeField] float _acceleration = 10;
    [SerializeField] float _impulseJumpForce = 5f;
    [SerializeField] float _continuousJumpForce = 5f;
    [SerializeField] float _maxJumpingTime;
    [SerializeField] float _coyoteTime = 0.2f;
    [SerializeField] float _jumpBufferTime = 0.2f;

    [Header("Look Settings")]
    [SerializeField] Vector2 _lookSensitivity;
    [SerializeField] float MaxLookAngle = 50f;
    [SerializeField] float MinLookAngle = -50f;

    float _currentLookX = 0f;

    [Header("Grounding")]
    [SerializeField] LayerMask _groundLayer;


    Vector2 _inputDirection;
    Vector2 _inputLook;
    float _deadZone = 0.3f;
    float _timeSinceLastJump = 0f;
    float _timeSinceLastGrounded = 0f;
    float _jumpingTime;
    bool _isJumping = false;
    bool _shouldJumpEnd = false;

    public override void CameraSetup(CameraData cameraData)
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraData.SetIsProximitysplitscreen(false);

        cameraData.SetCameraActive(true);
        // Try to use the Camera provided by CameraData, otherwise fallback to Camera.main
        if (cameraData != null)
        {
            cameraData.SetTarget(transform);
            _cinemachineCamera.OutputChannel = cameraData.getOutputChannel();
         
            
        }

       
        
    }

    public override void InteractInput(bool isUsing)
    {
        if (!IsOwner) return;

    }

    public override void JumpInput(bool isJumping)
    {
        if (!IsOwner) return;

        _shouldJumpEnd = !isJumping;
        if (isJumping)
        {
            _timeSinceLastJump = 0f;
        }
        else
        {
            _timeSinceLastJump = Mathf.Infinity;
        }


    }

    public override void LookInput(Vector2 delta)
    {
        if (!IsOwner) return;

         _inputLook = delta * _lookSensitivity;


    }



    public override void MoveInput(Vector2 direction)
    {
        if (!IsOwner) return;

        _inputDirection = direction;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (_rigidbody == null) return;

        _currentLookX -= _inputLook.y;

        _currentLookX = Mathf.Clamp(_currentLookX, MinLookAngle, MaxLookAngle);

         transform.Rotate(Vector3.up, _inputLook.x);
        _cinemachineCamera.transform.localRotation = Quaternion.Euler(_currentLookX, 0, 0);

        Vector3 MoveDirection = transform.forward * _inputDirection.y;
        MoveDirection += transform.right * _inputDirection.x;
        MoveDirection.y = 0f;

        Vector3 targetVelocity = MoveDirection.normalized * _moveSpeed;

        if (MoveDirection.sqrMagnitude < _deadZone * _deadZone)
        {
            targetVelocity = Vector3.zero;
        }

        targetVelocity.y = _rigidbody.linearVelocity.y;

        Vector3 velocity = Vector3.Lerp(_rigidbody.linearVelocity, targetVelocity, _acceleration * Time.fixedDeltaTime);



        _rigidbody.linearVelocity = velocity;


        _animator.SetBool("IsJumping", _isJumping);

        if (IsGrounded())
        {
            velocity.y = 0f;
            _animator.SetFloat("Speed", _inputDirection.sqrMagnitude);
            _animator.SetBool("IsGrounded", true);
        }
        else
        {
            velocity.x = 0f;
            velocity.z = 0f;
            _animator.SetFloat("Speed", velocity.sqrMagnitude / _moveSpeed * _moveSpeed);
            _animator.SetBool("IsGrounded", false);
        }
        // handle Jumping
        HandleJumping();



    }

    void HandleJumping()
    {


        if (IsGrounded())
        {
            _timeSinceLastGrounded = 0f;
          
        }
        else
        {
            _timeSinceLastGrounded += Time.fixedDeltaTime;
           
        }
        if (_timeSinceLastGrounded < _jumpBufferTime && _timeSinceLastJump < _jumpBufferTime)
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _jumpingTime = 0f;
               
                _rigidbody.AddForce(Vector3.up * _impulseJumpForce, ForceMode.VelocityChange);
                _timeSinceLastGrounded = Mathf.Infinity; // prevent double jump
            }

        }
        else if (_isJumping)
        {
            _rigidbody.AddForce(Vector3.up * _continuousJumpForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            _jumpingTime += Time.fixedDeltaTime;
        }
        if (_jumpingTime > _maxJumpingTime)
        {
            _shouldJumpEnd = true;
        }

        if (_shouldJumpEnd)
        {
            _isJumping = false;

        }
        _timeSinceLastJump += Time.fixedDeltaTime;
    }

    public bool IsGrounded()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position - new Vector3(0, 1, 0), 0.1f, _groundLayer);

        if (hits.Length > 0)
        {
            return true;
        }
        return false;
    }


}


