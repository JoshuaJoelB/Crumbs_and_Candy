using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Player Rotation")]
    [SerializeField] private float rotationSpeed = 180f;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        if (Keyboard.current == null)
            return;

        float rotationInput = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
            rotationInput = -1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            rotationInput = 1f;

        float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;

        transform.Rotate(0f, rotationAmount, 0f);
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
