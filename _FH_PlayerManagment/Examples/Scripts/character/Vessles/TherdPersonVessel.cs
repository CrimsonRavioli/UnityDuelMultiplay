using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class TherdPersonVessel : BaseVesselController
{

    [Header("References")]
    [SerializeField] ClientNetworkAnimator _animator;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] CinemachineOrbitalFollow _cinemachineCamera;
    
    Transform _cameraTransform;
    
    [Header("Stats")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _rotationSpeed = 10f;
    [SerializeField] float _acceleration = 10;
    [SerializeField] float _impulseJumpForce = 5f;
    [SerializeField] float _continuousJumpForce = 5f;
    [SerializeField] float _maxJumpingTime;
    [SerializeField] float _coyoteTime = 0.2f;
    [SerializeField] float _jumpBufferTime = 0.2f;

    [Header("Grounding")]
    [SerializeField] LayerMask _groundLayer;


    Vector2 _inputDirection;
    float _deadZone = 0.3f;
    float _timeSinceLastJump = 0f;
    float _timeSinceLastGrounded  = 0f;
    float _jumpingTime;
    bool _isJumping = false;
    bool _shouldJumpEnd = false;




    public override void CameraSetup(CameraData cameraData)
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraData.SetIsProximitysplitscreen(true);

        // Try to use the Camera provided by CameraData, otherwise fallback to Camera.main
        if (cameraData != null)
        {  
            cameraData.SetTarget(transform);
            _cinemachineCamera.VirtualCamera.OutputChannel = cameraData.getOutputChannel();
            var cam = cameraData.GetCamera();
            if (cam != null)
            {
                _cameraTransform = cam.transform;
                return;
            }
        }

        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
            return;
        }

        // As a last resort, try to use the serialized Cinemachine camera (if it has a transform)
        if (_cinemachineCamera is Component cmpt)
        {
            _cameraTransform = cmpt.transform;
        }

        _cinemachineCamera.transform.parent = null;
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

        _cinemachineCamera.VerticalAxis.Value += delta.y / 50;
        _cinemachineCamera.HorizontalAxis.Value += delta.x /50;




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

       

        // Build camera relative movement
        Vector3 cameraForward = (_cameraTransform != null) ? _cameraTransform.forward : transform.forward;
        Vector3 cameraRight = (_cameraTransform != null) ? _cameraTransform.right : transform.right;

        Vector3 MoveDirection = cameraForward * _inputDirection.y;
        MoveDirection += cameraRight * _inputDirection.x;
        MoveDirection.y = 0f;
      
        // handle rotation 

        if (MoveDirection.sqrMagnitude >= _deadZone * _deadZone)
        {
            transform.forward = Vector3.Slerp(transform.forward, MoveDirection.normalized, _rotationSpeed * Time.fixedDeltaTime);
        }

        // handle movement

        Vector3 targetVelocity = ((MoveDirection.normalized + transform.forward) / 2) * _moveSpeed;
        
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
       Collider[] hits = Physics.OverlapSphere(transform.position - new Vector3(0, 1, 0), 0.1f , _groundLayer);

        if (hits.Length > 0)
        {
            return true;
        }
        return false;
    }


}


