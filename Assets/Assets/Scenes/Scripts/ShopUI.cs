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
        if (shopPanel != null) shopPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenShop()
    {
        ShopManager sm = FindObjectOfType<ShopManager>();
        if (sm != null)
        {
            sm.OpenShop();
            return;
        }

        if (isOpen) return;

        isOpen = true;

        if (shopPanel != null) shopPanel.SetActive(true);

        // Stop player movement
        if (playerMovement != null) playerMovement.enabled = false;

        // Allow mouse interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool BuyCard(string cardId, int price)
    {
        if (PlayerProfile.coins < price)
        {
            Debug.LogWarning($"[ShopUI] Not enough coins for {cardId}. Needed: {price}, Current: {PlayerProfile.coins}");
            return false;
        }

        PlayerProfile.coins -= price;
        PlayerProfile.Save();

        bool success = PlayerCardData.AcquireFromShop(cardId);
        Debug.Log($"[ShopUI] Successfully purchased {cardId}. Remaining coins: {PlayerProfile.coins}");
        return success;
    }

    public void BuyCardString(string cardIdAndPrice)
    {
        if (string.IsNullOrEmpty(cardIdAndPrice)) return;

        string[] parts = cardIdAndPrice.Split(',');
        string cardId = parts[0].Trim();
        int price = 10;

        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int parsedPrice))
        {
            price = parsedPrice;
        }

        BuyCard(cardId, price);
    }
}
