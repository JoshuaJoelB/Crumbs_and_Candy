using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look / Rotation")]
    [Tooltip("Sensitivity for horizontal mouse turning.")]
    [SerializeField] private float mouseSensitivity = 2f;
    [Tooltip("Speed for keyboard arrow key turning if used.")]
    [SerializeField] private float keyboardRotationSpeed = 90f;
    [Tooltip("Enable smoothing on mouse rotation.")]
    [SerializeField] private bool smoothRotation = false;
    [SerializeField] private float rotationSmoothTime = 0.05f;

    [Header("Cursor Settings")]
    [SerializeField] private bool autoLockCursor = true;

    private CharacterController controller;
    private float verticalVelocity;

    private float targetYaw;
    private float currentYawVelocity;

    public CharacterController Controller => controller;
    public Vector3 CurrentInput { get; private set; }
    public bool IsMoving => CurrentInput.sqrMagnitude > 0.001f;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        targetYaw = transform.eulerAngles.y;
    }

    private void Start()
    {
        if (autoLockCursor)
        {
            LockCursor();
        }
    }

    private void OnEnable()
    {
        if (autoLockCursor)
        {
            LockCursor();
        }
    }

    private void OnDisable()
    {
        UnlockCursor();
        CurrentInput = Vector3.zero;
    }

    private void Update()
    {
        HandleCursorLock();
        HandleRotation();
        HandleMovement();
    }

    private void HandleCursorLock()
    {
        // Re-lock and hide cursor on left mouse click inside game window
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        // Allow pressing Escape to release the cursor (for menus / editor navigation)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleRotation()
    {
        // Only rotate when the cursor is locked to gameplay
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = 0f;

            if (Mouse.current != null)
            {
                mouseX = Mouse.current.delta.x.ReadValue();
            }
            else
            {
                mouseX = Input.GetAxis("Mouse X");
            }

            // Normalizes sensitivity if previously set to high value (e.g. 150) or normal value (0.1 - 2)
            float effectiveSensitivity = mouseSensitivity > 10f ? (mouseSensitivity * 0.02f) : mouseSensitivity;
            targetYaw += mouseX * effectiveSensitivity;
        }

        // Optional keyboard arrow key rotation
        float keyboardRotation = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed) keyboardRotation -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) keyboardRotation += 1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow)) keyboardRotation -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) keyboardRotation += 1f;
        }
        targetYaw += keyboardRotation * keyboardRotationSpeed * Time.deltaTime;

        // Apply rotation to player orientation
        if (smoothRotation)
        {
            float currentYaw = transform.eulerAngles.y;
            float smoothYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref currentYawVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        }
    }

    private void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
        }
        else
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
        }

        Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
        CurrentInput = input;

        if (controller != null)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            // Movement relative to current mouse orientation (transform.forward and transform.right)
            Vector3 movement = (transform.right * input.x) + (transform.forward * input.z);
            Vector3 velocity = (movement * moveSpeed) + (Vector3.up * verticalVelocity);

            controller.Move(velocity * Time.deltaTime);
        }
    }
}
