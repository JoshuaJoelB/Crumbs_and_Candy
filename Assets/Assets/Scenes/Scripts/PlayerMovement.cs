using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look / Rotation")]
    [SerializeField] private float mouseSensitivity = 2f;
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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        targetYaw = transform.eulerAngles.y;
    }

    private void Start()
    {
        if (autoLockCursor) LockCursor();
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
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue();
            targetYaw += mouseX * mouseSensitivity;
        }

        if (smoothRotation)
        {
            float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref currentYawVelocity, rotationSmoothTime);
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

        Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
        CurrentInput = input; // THIS is what the Animator reads

        if (controller != null)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 movement = (transform.right * input.x) + (transform.forward * input.z);
            Vector3 velocity = (movement * moveSpeed) + (Vector3.up * verticalVelocity);

            controller.Move(velocity * Time.deltaTime);
        }
    }
}