using UnityEngine;

/// <summary>
/// Attach this script to the Anvil or an interaction zone near the Anvil.
/// Opens the Blacksmith UI when the player approaches and presses the interact key.
/// </summary>
public class BlacksmithInteraction : MonoBehaviour
{
    [Header("Blacksmith UI Reference")]
    public BlacksmithUI blacksmithUI;

    [Header("Interaction Settings")]
    public float interactionDistance = 3.5f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode secondaryKey = KeyCode.Q;
    public GameObject interactionPrompt;

    private bool playerInTrigger = false;
    private Transform playerTransform;

    private void Start()
    {
        FindPlayer();
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (blacksmithUI == null)
        {
            blacksmithUI = FindObjectOfType<BlacksmithUI>();
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (blacksmithUI != null && blacksmithUI.IsOpen)
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(false);
            }
            return;
        }

        if (playerTransform == null)
        {
            FindPlayer();
        }

        // Check if player is in range via trigger or distance
        bool inRange = playerInTrigger;
        if (!inRange && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= interactionDistance)
            {
                inRange = true;
            }
        }

        // Update interaction prompt visibility
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(inRange);
        }

        // Handle interaction input
        if (inRange)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(secondaryKey))
            {
                if (blacksmithUI != null)
                {
                    blacksmithUI.OpenBlacksmith();
                }
                else
                {
                    Debug.LogWarning("[BlacksmithInteraction] BlacksmithUI reference is missing!");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
