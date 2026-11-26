using UnityEngine;

[AddComponentMenu("Player/Player Controller")]
public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    public PlayerMovement playerMovement;

    [Header("Animator Settings")]
    [Tooltip("The exact name of the Float parameter in your Animator (e.g., 'Speed').")]
    public string speedParameter = "Speed";

    void Awake()
    {
        // Auto-find Animator on this object or its children
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        // Auto-find PlayerMovement on this object
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (animator == null || playerMovement == null) return;

        // Get the current input magnitude (0 = idle, 1 = moving)
        float currentSpeed = playerMovement.CurrentInput.magnitude;

        // Send the speed to the Animator
        animator.SetFloat(speedParameter, currentSpeed);
    }
}