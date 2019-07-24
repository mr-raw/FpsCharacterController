using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
[RequireComponent(typeof(CharacterController))]
public class FpsCharacterController : MonoBehaviour
{
    [Header("Look around")]
    [SerializeField] private string mouseXInputName = "Mouse X";
    [SerializeField] private string mouseYInputName = "Mouse Y";
    [SerializeField] private string horizontalInputName = "Horizontal";
    [SerializeField] private string verticalInputName = "Vertical";
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("Move around")]
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float runSpeed = 12.0f;
    [SerializeField] private float runBuildUpSpeed = 4.0f;
    [SerializeField] private AnimationCurve jumpFallOff = new AnimationCurve(new Keyframe(0f, 0.0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));
    [SerializeField] private float jumpMultiplier = 2.5f;
    
    [Header("Keycodes")] 
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    private Camera _playerCamera;
    private CharacterController _charController;
    private bool _isJumping;
    private bool _isSprinting;
    private float _movementSpeed;
    
    private void Start()
    {
        LockCursor();

        _charController = GetComponent<CharacterController>();
        if (_charController == null)
        {
            Debug.LogError("CharacterController is not assigned.");
            Debug.Break();
        }

        // TODO: The camera should be a child to the Player component. Add check for this here.
        if (_playerCamera == null)
        {
            Debug.Log("Main Camera not assigned manually. Trying to find the camera automatically.");
            _playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            if (_playerCamera == null)
            {
                Debug.LogError("The camera could not be found.");
                Debug.Break();
            }
        }
    }

    private void Update()
    {
        CameraRotation();
        PlayerMovement();
    }
    
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void CameraRotation() // Called up to 60 times per second.
    {
        var mouseX = Input.GetAxisRaw(mouseXInputName);
        var mouseY = Input.GetAxisRaw(mouseYInputName);

        var thisTransform = transform;
        var cameraTransform = _playerCamera.transform;

        var newRotationX = thisTransform.localEulerAngles;
        var newRotationY = cameraTransform.localEulerAngles;
        
        newRotationX.y += mouseX * mouseSensitivity; // Left to right movement.
        newRotationY.x -= Mathf.Clamp(mouseY * mouseSensitivity, -90, 90); // Up and down movement.
        
        thisTransform.localEulerAngles = newRotationX;
        cameraTransform.localEulerAngles = newRotationY;
    }

    void PlayerMovement() // Used some tricks from the SciFiDemo i followed on Udemy. Called up to 60 times per second.
    {
        var vertInput = Input.GetAxis(verticalInputName);
        var horizInput = Input.GetAxis(horizontalInputName);
        
        var direction = new Vector3(horizInput, 0, vertInput);
        var velocity = direction * _movementSpeed;
        velocity = transform.TransformDirection(velocity);
        _charController.Move(velocity * Time.deltaTime);
       
        _movementSpeed = Mathf.Lerp(_movementSpeed, Input.GetKey(sprintKey) ? runSpeed : walkSpeed, Time.deltaTime * runBuildUpSpeed);
        // JumpInput();
    }

    void JumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !_isJumping)
        {
            _isJumping = true;
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        _charController.slopeLimit = 90.0f;
        var timeInAir = 0.0f;
        do
        {
            var jumpForce = jumpFallOff.Evaluate(timeInAir);
            _charController.Move(jumpForce * jumpMultiplier * Time.deltaTime * Vector3.up);
            _charController.Move(Vector3.up * Time.deltaTime);
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!_charController.isGrounded && _charController.collisionFlags != CollisionFlags.Above);

        _charController.slopeLimit = 45.0f;
        _isJumping = false;
    }
    
    private void OnGUI()
    {
        GUILayout.Label($"Player: {transform.rotation}, Camera: {_playerCamera.transform.rotation}");
    }
}

/* CHANGELOG
 24.07.19: Removed the OnSlope method all together. Have to implement it at a later time. Jumping is also not working.
 



*/