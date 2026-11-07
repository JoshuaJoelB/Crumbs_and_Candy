
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Rotation")]
    [SerializeField] private float mouseSensitivity = 150f;

    private CharacterController controller;
    private float verticalVelocity;
    private float rotationY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        rotationY = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleMouseRotation();
        HandleMovement();
    }

    private void HandleMouseRotation()
    {
        if (Mouse.current == null)
            return;

        float mouseX = Mouse.current.delta.x.ReadValue();
        rotationY += mouseX * mouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
            return;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.wKey.isPressed) z += 1f;

        Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = transform.right * input.x + transform.forward * input.z;
        Vector3 velocity = movement * moveSpeed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}