using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;

    [Header("Player")]
    public Behaviour playerMovement;

    private bool isOpen;

    private void Start()
    {
        shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (isOpen) return;

        isOpen = true;

        shopPanel.SetActive(true);

        // Stop player movement
        playerMovement.enabled = false;

        // Allow mouse interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;

        shopPanel.SetActive(false);

        // Restore player movement
        playerMovement.enabled = true;

        // Return to gameplay cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}