using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Tooltip("Acceleration rate when speeding up.")]
    [SerializeField] private float acceleration = 12f;

    [Tooltip("Deceleration rate when stopping.")]
    [SerializeField] private float deceleration = 14f;

    [Tooltip("Gravity acceleration.")]
    [SerializeField] private float gravity = -9.81f;

    [Header("A/D Pressure & Hold Duration")]
    [Tooltip("Scale movement distance and speed based on how long A or D is pressed.")]
    [SerializeField] private bool usePressureMovement = true;

    [Tooltip("Starting minimum pressure on initial tap (e.g., 0.25 = 25% speed for small quick nudges).")]
    [Range(0.05f, 1f)]
    [SerializeField] private float initialTapPressure = 0.25f;

    [Tooltip("Duration in seconds of holding A or D to ramp up to 100% full speed and stride.")]
    [SerializeField] private float timeToFullPressure = 0.5f;

    [Tooltip("How fast pressure resets when A or D is released.")]
    [SerializeField] private float pressureReleaseRate = 5f;

    [Tooltip("Also apply hold duration pressure ramping to W and S keys.")]
    [SerializeField] private bool applyToForwardBack = false;

    [Header("Rotation Settings")]
    [Tooltip("Smoothly rotate towards movement direction.")]
    [SerializeField] private bool smoothRotation = true;

    [Tooltip("Time in seconds to smoothly turn towards the target direction.")]
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Header("Camera Alignment")]
    [Tooltip("If true, W/A/S/D moves relative to the camera view. If false, moves relative to world axes.")]
    [SerializeField] private bool useCameraRelative = true;

    [Header("Cursor Settings")]
    [SerializeField] private bool autoLockCursor = false;

    private CharacterController controller;
    private float verticalVelocity;
    private float currentYawVelocity;
    private float targetYaw;
    private Vector3 currentHorizontalVelocity;
    private float currentHorizontalPressure;
    private float currentVerticalPressure;

    public CharacterController Controller => controller;
    public Vector3 CurrentInput { get; private set; }
    public float MoveSpeed => moveSpeed;
    public bool IsMoving => currentHorizontalVelocity.sqrMagnitude > 0.01f;
    public float HorizontalPressure => currentHorizontalPressure;

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

    private void OnDisable()
    {
        CurrentInput = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
        currentHorizontalPressure = 0f;
        currentVerticalPressure = 0f;
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

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Read input from Keyboard or Gamepad
        float rawX = 0f;
        float rawZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) rawX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) rawX += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) rawZ -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) rawZ += 1f;
        }
        else
        {
            rawX = Input.GetAxisRaw("Horizontal");
            rawZ = Input.GetAxisRaw("Vertical");
        }

        // Optional Gamepad analog stick support
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.01f)
            {
                rawX = stick.x;
                rawZ = stick.y;
            }
        }

        // 2. Apply pressure / hold-duration scaling
        float effectiveX = rawX;
        float effectiveZ = rawZ;

        if (usePressureMovement)
        {
            // A and D hold duration pressure
            effectiveX = CalculatePressure(rawX, ref currentHorizontalPressure);

            if (applyToForwardBack)
            {
                effectiveZ = CalculatePressure(rawZ, ref currentVerticalPressure);
            }
        }

        Vector3 processedInput = Vector3.ClampMagnitude(new Vector3(effectiveX, 0f, effectiveZ), 1f);

        // 3. Determine movement direction
        Vector3 moveDirection = Vector3.zero;
        Camera mainCam = Camera.main;

        if (useCameraRelative && mainCam != null)
        {
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;

            if (camForward.sqrMagnitude > 0.001f && camRight.sqrMagnitude > 0.001f)
            {
                camForward.Normalize();
                camRight.Normalize();
                moveDirection = (camRight * processedInput.x) + (camForward * processedInput.z);
            }
            else
            {
                moveDirection = new Vector3(processedInput.x, 0f, processedInput.z);
            }
        }
        else
        {
            moveDirection = new Vector3(processedInput.x, 0f, processedInput.z);
        }

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        // 4. Smooth Acceleration and Deceleration
        Vector3 targetVelocity = moveDirection * moveSpeed;
        float rate = (moveDirection.sqrMagnitude > 0.001f) ? acceleration : deceleration;
        currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, rate * Time.deltaTime);

        // CurrentInput is exposed for the Animator (smooth transition 0 to 1)
        CurrentInput = (moveSpeed > 0.01f)
            ? currentHorizontalVelocity / moveSpeed
            : processedInput;

        // 5. Smooth Rotation (Face movement direction, remember last facing direction when stopped)
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            targetYaw = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        }

        if (smoothRotation)
        {
            float smoothYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetYaw,
                ref currentYawVelocity,
                rotationSmoothTime > 0f ? rotationSmoothTime : 0.05f
            );
            transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        }

        // 6. Apply Movement & Gravity via CharacterController
        if (controller != null && controller.enabled)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 finalVelocity = currentHorizontalVelocity + (Vector3.up * verticalVelocity);
            controller.Move(finalVelocity * Time.deltaTime);
        }
    }

    private float CalculatePressure(float targetInput, ref float currentPressure)
    {
        if (Mathf.Abs(targetInput) > 0.01f)
        {
            float targetDir = Mathf.Sign(targetInput);

            // If switching directions (e.g. from A to D immediately), reset cleanly
            if (currentPressure != 0f && Mathf.Sign(currentPressure) != targetDir)
            {
                currentPressure = 0f;
            }

            // On initial press/tap, start with initial tap pressure so it responds immediately
            if (Mathf.Abs(currentPressure) < initialTapPressure)
            {
                currentPressure = targetDir * initialTapPressure;
            }

            // Smoothly ramp up pressure to 1.0 based on hold duration
            float rampRate = (timeToFullPressure > 0.01f) ? (1f / timeToFullPressure) : 100f;
            currentPressure = Mathf.MoveTowards(currentPressure, targetDir, rampRate * Time.deltaTime);
        }
        else
        {
            // Released: smoothly decay back to 0
            currentPressure = Mathf.MoveTowards(currentPressure, 0f, pressureReleaseRate * Time.deltaTime);
        }

        return currentPressure;
    }
}
