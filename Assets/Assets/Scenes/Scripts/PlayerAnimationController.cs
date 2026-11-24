using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    public PlayerMovement playerMovement;

    [Header("Animator Settings")]
    [Tooltip("The exact name of the Float parameter in your Animator.")]
    public string speedParameter = "Speed";

    void Awake()
    {
        // Auto-find Animator (looks on this object and children)
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        // Auto-find PlayerMovement (looks on this exact object)
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (animator == null || playerMovement == null) return;

        // Get the exact input magnitude (0 = idle, 1 = moving)
        float currentSpeed = playerMovement.CurrentInput.magnitude;

        // Send it to the Animator
        animator.SetFloat(speedParameter, currentSpeed);
    }
}