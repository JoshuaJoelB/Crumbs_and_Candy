using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player animations by linking movement/input data with the Animator component.
/// Works with both Blend Trees (Float parameters) and State Transitions (Bool parameters).
/// Can be attached directly to the Player root GameObject or to the child model containing the Animator.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Animation Controller")]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The Animator component on this object or in its children/parents.")]
    [SerializeField] private Animator animator;
    [Tooltip("Reference to the PlayerMovement component. Auto-detected if left empty.")]
    [SerializeField] private PlayerMovement playerMovement;
    [Tooltip("Reference to the CharacterController component. Auto-detected if left empty.")]
    [SerializeField] private CharacterController characterController;

    [Header("Animator Parameter Names (Matches Animator Controller)")]
    [Tooltip("Float parameter for movement speed (0 = Idle, 1 = Walk, >1 = Run).")]
    [SerializeField] private string speedParameter = "Speed";
    [Tooltip("Bool parameter for movement state (true = Moving/Walking, false = Idle).")]
    [SerializeField] private string isMovingParameter = "isMoving";
    [Tooltip("Bool parameter alternate name for walking state.")]
    [SerializeField] private string isWalkingParameter = "isWalking";
    [Tooltip("Float parameter for horizontal strafe movement (-1 = Left, 1 = Right).")]
    [SerializeField] private string horizontalParameter = "Horizontal";
    [Tooltip("Float parameter for forward/backward movement (-1 = Backward, 1 = Forward).")]
    [SerializeField] private string verticalParameter = "Vertical";
    [Tooltip("Bool parameter for grounded state.")]
    [SerializeField] private string isGroundedParameter = "isGrounded";

    [Header("Smoothing & Thresholds")]
    [Tooltip("Damp time for float parameter transitions (prevents abrupt animation snaps).")]
    [SerializeField] private float dampTime = 0.1f;
    [Tooltip("Minimum movement magnitude to consider the player moving.")]
    [SerializeField] private float movementThreshold = 0.05f;

    [Header("Live Debug Status (Read-Only)")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private bool currentIsMoving = false;
    [SerializeField] private Vector2 currentDirection = Vector2.zero;
    [SerializeField] private bool currentIsGrounded = true;

    // Parameter ID hashes for fast Animator access
    private int speedHash;
    private int isMovingHash;
    private int isWalkingHash;
    private int horizontalHash;
    private int verticalHash;
    private int isGroundedHash;

    // Track which parameters actually exist in the assigned Animator to avoid console warnings
    private readonly Dictionary<int, AnimatorControllerParameterType> cachedParameters = new Dictionary<int, AnimatorControllerParameterType>();

    public Animator Animator => animator;
    public float CurrentSpeed => currentSpeed;
    public bool IsMoving => currentIsMoving;

    private void Awake()
    {
        InitializeComponents();
        CacheParameterHashes();
    }

    private void Start()
    {
        // Re-check references in case objects were instantiated at runtime
        InitializeComponents();
        RefreshAnimatorParameters();
    }

    private void OnEnable()
    {
        RefreshAnimatorParameters();
    }

    private void OnDisable()
    {
        // Reset animation states to Idle when component or movement is disabled
        ResetAnimationState();
    }

    /// <summary>
    /// Auto-finds required components on self, children, or parents if not manually assigned.
    /// </summary>
    public void InitializeComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = GetComponentInParent<Animator>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null) playerMovement = GetComponentInParent<PlayerMovement>();
            if (playerMovement == null) playerMovement = GetComponentInChildren<PlayerMovement>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null) characterController = GetComponentInParent<CharacterController>();
            if (characterController == null) characterController = GetComponentInChildren<CharacterController>();
        }
    }

    private void CacheParameterHashes()
    {
        speedHash = Animator.StringToHash(speedParameter);
        isMovingHash = Animator.StringToHash(isMovingParameter);
        isWalkingHash = Animator.StringToHash(isWalkingParameter);
        horizontalHash = Animator.StringToHash(horizontalParameter);
        verticalHash = Animator.StringToHash(verticalParameter);
        isGroundedHash = Animator.StringToHash(isGroundedParameter);
    }

    /// <summary>
    /// Reads and caches all parameter names present on the Animator controller.
    /// This ensures we only set parameters that exist, preventing Unity warning logs.
    /// </summary>
    public void RefreshAnimatorParameters()
    {
        cachedParameters.Clear();

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        foreach (var param in animator.parameters)
        {
            cachedParameters[param.nameHash] = param.type;
        }
    }

    private void Update()
    {
        if (animator == null || !animator.enabled)
            return;

        UpdateMovementValues();
        ApplyAnimatorParameters();
    }

    /// <summary>
    /// Gathers input and movement data from PlayerMovement, CharacterController, or direct input.
    /// </summary>
    private void UpdateMovementValues()
    {
        Vector2 inputDir = Vector2.zero;
        bool isMoving = false;
        float speed = 0f;
        bool isGrounded = true;

        // 1. If PlayerMovement component is attached and active, prioritize its data
        if (playerMovement != null && playerMovement.enabled)
        {
            Vector3 moveInput = playerMovement.CurrentInput;
            inputDir = new Vector2(moveInput.x, moveInput.z);
            isMoving = moveInput.sqrMagnitude > (movementThreshold * movementThreshold);
            speed = moveInput.magnitude;

            if (characterController != null)
            {
                isGrounded = characterController.isGrounded;
            }
        }
        else if (playerMovement != null && !playerMovement.enabled)
        {
            // Player movement is paused (e.g. Blacksmith UI or menu is open) -> force idle
            inputDir = Vector2.zero;
            isMoving = false;
            speed = 0f;
            isGrounded = true;
        }
        // 2. Fallback: read CharacterController velocity or direct keyboard input
        else
        {
            inputDir = ReadKeyboardInput();
            isMoving = inputDir.sqrMagnitude > (movementThreshold * movementThreshold);
            speed = inputDir.magnitude;

            if (characterController != null)
            {
                isGrounded = characterController.isGrounded;
                Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
                if (horizontalVelocity.sqrMagnitude > (movementThreshold * movementThreshold))
                {
                    isMoving = true;
                    speed = Mathf.Max(speed, horizontalVelocity.magnitude);
                }
            }
        }

        currentDirection = inputDir;
        currentIsMoving = isMoving;
        currentSpeed = speed;
        currentIsGrounded = isGrounded;
    }

    /// <summary>
    /// Safely writes updated values to the Animator based on cached parameters.
    /// </summary>
    private void ApplyAnimatorParameters()
    {
        // 1. Float Speed parameter (Used in Blend Trees: 0 = Idle, 1 = Walk)
        if (HasParameter(speedHash, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(speedHash, currentSpeed, dampTime, Time.deltaTime);
        }

        // 2. Bool IsMoving parameter (Used in standard Idle <-> Walk transitions)
        if (HasParameter(isMovingHash, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(isMovingHash, currentIsMoving);
        }

        // 3. Bool isWalking parameter (Alternative common name)
        if (HasParameter(isWalkingHash, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(isWalkingHash, currentIsMoving);
        }

        // 4. Directional Floats for 2D Strafe/Walk Blend Trees
        if (HasParameter(horizontalHash, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(horizontalHash, currentDirection.x, dampTime, Time.deltaTime);
        }
        if (HasParameter(verticalHash, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(verticalHash, currentDirection.y, dampTime, Time.deltaTime);
        }

        // 5. Grounded state
        if (HasParameter(isGroundedHash, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(isGroundedHash, currentIsGrounded);
        }
    }

    /// <summary>
    /// Resets all animation parameters back to Idle state.
    /// </summary>
    public void ResetAnimationState()
    {
        currentSpeed = 0f;
        currentIsMoving = false;
        currentDirection = Vector2.zero;

        if (animator == null || !animator.isInitialized) return;

        if (HasParameter(speedHash, AnimatorControllerParameterType.Float))
            animator.SetFloat(speedHash, 0f);

        if (HasParameter(isMovingHash, AnimatorControllerParameterType.Bool))
            animator.SetBool(isMovingHash, false);

        if (HasParameter(isWalkingHash, AnimatorControllerParameterType.Bool))
            animator.SetBool(isWalkingHash, false);

        if (HasParameter(horizontalHash, AnimatorControllerParameterType.Float))
            animator.SetFloat(horizontalHash, 0f);

        if (HasParameter(verticalHash, AnimatorControllerParameterType.Float))
            animator.SetFloat(verticalHash, 0f);
    }

    /// <summary>
    /// Reads direct input from Keyboard (InputSystem or legacy) as a fallback.
    /// </summary>
    private Vector2 ReadKeyboardInput()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
        }
        else
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
        }

        return Vector2.ClampMagnitude(new Vector2(x, z), 1f);
    }

    private bool HasParameter(int hash, AnimatorControllerParameterType expectedType)
    {
        return cachedParameters.TryGetValue(hash, out var type) && type == expectedType;
    }

    // =========================================================================
    // PUBLIC ACTION TRIGGERS (Can be called from other scripts or Unity Events)
    // =========================================================================

    /// <summary>
    /// Triggers a jump animation if a 'Jump' trigger is defined in the Animator.
    /// </summary>
    public void TriggerJump()
    {
        TriggerAnimation("Jump");
    }

    /// <summary>
    /// Triggers an attack animation if an 'Attack' trigger is defined in the Animator.
    /// </summary>
    public void TriggerAttack()
    {
        TriggerAnimation("Attack");
    }

    /// <summary>
    /// Triggers a hurt / take damage animation if a 'Hurt' or 'Hit' trigger is defined in the Animator.
    /// </summary>
    public void TriggerHurt()
    {
        if (!TriggerAnimation("Hurt"))
        {
            TriggerAnimation("Hit");
        }
    }

    /// <summary>
    /// Safely fires any trigger by name without throwing an error if it doesn't exist.
    /// </summary>
    public bool TriggerAnimation(string triggerName)
    {
        if (animator == null) return false;

        int hash = Animator.StringToHash(triggerName);
        if (HasParameter(hash, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(hash);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Safely sets a custom boolean parameter by name.
    /// </summary>
    public void SetCustomBool(string boolName, bool value)
    {
        if (animator == null) return;
        int hash = Animator.StringToHash(boolName);
        if (HasParameter(hash, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(hash, value);
        }
    }

    /// <summary>
    /// Safely sets a custom float parameter by name.
    /// </summary>
    public void SetCustomFloat(string floatName, float value)
    {
        if (animator == null) return;
        int hash = Animator.StringToHash(floatName);
        if (HasParameter(hash, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(hash, value);
        }
    }
}
