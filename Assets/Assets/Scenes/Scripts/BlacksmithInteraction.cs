using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this script to the Anvil (or any child/parent of the anvil).
/// Makes the Blacksmith interactive:
/// - Clicking the Anvil with Left Mouse Button opens the Blacksmith UI
/// - Pressing interact key (E or Q) when nearby or looking at the Anvil opens the Blacksmith UI
/// - Displays interaction prompt when hovering or in range
/// </summary>
public class BlacksmithInteraction : MonoBehaviour
{
    [Header("Blacksmith UI Reference")]
    [Tooltip("If not assigned, automatically finds BlacksmithUI in the scene.")]
    public BlacksmithUI blacksmithUI;

    [Header("Interaction Settings")]
    [Tooltip("Max distance the player can be from the anvil to interact via keys.")]
    public float interactionDistance = 6f;

    [Tooltip("Max distance for raycasting when clicking the anvil.")]
    public float maxRaycastDistance = 100f;

    [Tooltip("If true, clicking the anvil opens the UI regardless of distance (great for mouse clicking in free view).")]
    public bool allowClickAnyDistance = true;

    [Header("Keybindings")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode secondaryKey = KeyCode.Q;

    [Header("Optional Prompt UI")]
    public GameObject interactionPrompt;

    private bool playerInTrigger = false;
    private Transform playerTransform;
    private bool isHovered = false;
    private Collider anvilCollider;

    private void Awake()
    {
        EnsureCollider();
    }

    private void Start()
    {
        FindPlayer();
        AutoFindUI();

        // Safeguard: Never allow the Canvas or UI GameObject to be treated as an interaction prompt
        if (interactionPrompt != null && (interactionPrompt == gameObject || 
            interactionPrompt.GetComponent<Canvas>() != null || 
            interactionPrompt.GetComponent<BlacksmithUI>() != null))
        {
            Debug.LogWarning("[BlacksmithInteraction] interactionPrompt was incorrectly assigned to Canvas/UI! Clearing reference.");
            interactionPrompt = null;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void EnsureCollider()
    {
        anvilCollider = GetComponent<BoxCollider>();
        if (anvilCollider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                box.center = mf.sharedMesh.bounds.center;
                box.size = mf.sharedMesh.bounds.size * 1.3f;
            }
            else
            {
                box.size = new Vector3(3f, 3f, 3f);
            }
            anvilCollider = box;
        }
    }

    private void AutoFindUI()
    {
        if (blacksmithUI == null)
        {
            blacksmithUI = FindObjectOfType<BlacksmithUI>(true);
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
            return;
        }

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null)
        {
            playerTransform = pm.transform;
            return;
        }

        CharacterController cc = FindObjectOfType<CharacterController>();
        if (cc != null)
        {
            playerTransform = cc.transform;
            return;
        }
    }

    private void Update()
    {
        // Debug shortcut: B key to open Blacksmith UI directly anytime
        bool bPressed = Input.GetKeyDown(KeyCode.B);
        if (!bPressed && Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            bPressed = true;
        }
        if (bPressed)
        {
            OpenBlacksmithUI();
            return;
        }

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

        // 1. Check distance to player
        bool inRange = playerInTrigger;
        if (!inRange && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= interactionDistance)
            {
                inRange = true;
            }
        }

        // 2. Check mouse raycast hover and click
        CheckMouseInteraction(inRange);

        // 3. Update prompt visibility
        bool shouldShowPrompt = inRange || isHovered;
        if (interactionPrompt != null)
        {
            if (interactionPrompt.activeSelf != shouldShowPrompt)
            {
                interactionPrompt.SetActive(shouldShowPrompt);
            }
        }

        // 4. Keyboard interaction (E / Q)
        if (inRange || isHovered)
        {
            bool keyPressed = Input.GetKeyDown(interactKey) || Input.GetKeyDown(secondaryKey);
            if (!keyPressed && Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame)
                {
                    keyPressed = true;
                }
            }

            if (keyPressed)
            {
                OpenBlacksmithUI();
            }
        }
    }

    /// <summary>
    /// Handles mouse hovering and clicking on the anvil via Camera raycasting.
    /// Supports both free cursor and locked cursor (crosshair) modes.
    /// </summary>
    private void CheckMouseInteraction(bool inRange)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindObjectOfType<Camera>();
        if (cam == null) return;

        // Determine ray based on cursor state
        Ray ray;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Center of screen ray for locked cursor / first-person look
            ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            // Mouse pointer ray for free cursor
            Vector3 mousePos = Input.mousePosition;
            if (Mouse.current != null)
            {
                Vector2 mVal = Mouse.current.position.ReadValue();
                mousePos = new Vector3(mVal.x, mVal.y, 0f);
            }
            ray = cam.ScreenPointToRay(mousePos);
        }

        // Use RaycastAll to ensure we detect the anvil even if another collider/trigger is in between
        isHovered = false;
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance);
        foreach (var hit in hits)
        {
            if (IsAnvilObject(hit.collider.gameObject))
            {
                isHovered = true;
                break;
            }
        }

        // Check mouse click
        bool mouseClicked = Input.GetMouseButtonDown(0);
        if (!mouseClicked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseClicked = true;
        }

        if (mouseClicked && isHovered)
        {
            if (allowClickAnyDistance || inRange || playerTransform == null)
            {
                Debug.Log("[BlacksmithInteraction] Anvil clicked via Raycast!");
                OpenBlacksmithUI();
            }
        }
    }

    /// <summary>
    /// Unity built-in callback: triggered when this object's collider is clicked directly.
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log("[BlacksmithInteraction] Anvil clicked via OnMouseDown!");
        OpenBlacksmithUI();
    }

    private void OnMouseUpAsButton()
    {
        OpenBlacksmithUI();
    }

    public void OpenBlacksmithUI()
    {
        AutoFindUI();

        if (blacksmithUI != null)
        {
            if (!blacksmithUI.IsOpen)
            {
                Debug.Log("[BlacksmithInteraction] Opening Blacksmith UI!");
                blacksmithUI.OpenBlacksmith();
            }
        }
        else
        {
            Debug.LogWarning("[BlacksmithInteraction] BlacksmithUI reference is missing!");
        }
    }

    private bool IsAnvilObject(GameObject obj)
    {
        if (obj == null) return false;
        if (obj == gameObject) return true;
        if (obj.transform.IsChildOf(transform)) return true;
        if (transform.IsChildOf(obj.transform) && (obj.name.ToLower().Contains("anvil") || obj.name.ToLower().Contains("blacksmith"))) return true;
        string n = obj.name.ToLower();
        if (n.Contains("anvil") || n.Contains("blacksmith")) return true;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null || other.GetComponent<CharacterController>() != null)
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null || other.GetComponent<CharacterController>() != null)
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
