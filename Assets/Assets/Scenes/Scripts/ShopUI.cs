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
    }

    public void OpenShop()
    {
        if (isOpen) return;

        isOpen = true;

        if (shopPanel != null) shopPanel.SetActive(true);

        // Stop player movement
        if (playerMovement != null) playerMovement.enabled = false;

        // Allow mouse interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);

        // Restore player movement
        if (playerMovement != null) playerMovement.enabled = true;

        // Return to gameplay cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Purchases a card from the shop, deducting coins and recording the acquisition in PlayerCardData.
    /// </summary>
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

    /// <summary>
    /// Single-string parameter method visible in Unity Button OnClick() dropdown!
    /// Format: "CARD_ID,PRICE" (e.g. "NOR-001,20" or "MAG-001,35")
    /// </summary>
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
