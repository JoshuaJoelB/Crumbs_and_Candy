using UnityEngine;

/// <summary>
/// Trigger-based interaction for the shop NPC / area.
/// When the player enters range and presses Q, opens the ShopManager.
/// </summary>
public class ShopInteraction : MonoBehaviour
{
    [Header("Shop Reference")]
    [Tooltip("Assign the GameObject that has the ShopManager component.")]
    public ShopManager shopManager;

    [Header("Legacy Fallback (optional)")]
    [Tooltip("If ShopManager is not assigned, falls back to the old ShopUI.")]
    public ShopUI shopUI;

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            if (shopManager != null)
            {
                shopManager.OpenShop();
            }
            else if (shopUI != null)
            {
                shopUI.OpenShop();
            }
            else
            {
                Debug.LogWarning("[ShopInteraction] No ShopManager or ShopUI assigned!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
