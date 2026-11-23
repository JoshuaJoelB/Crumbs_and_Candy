using UnityEngine;

/// <summary>
/// Attach this script to your shop card button (e.g. the "Buy" button).
/// Exposes a 0-parameter Buy() method that shows up directly in Unity's Button OnClick() dropdown!
/// </summary>
public class ShopCardItem : MonoBehaviour
{
    [Header("Card Setup")]
    public string cardId = "NOR-001";
    public int price = 20;

    /// <summary>
    /// Shows up in Button OnClick () dropdown as: ShopCardItem -> Buy ()
    /// </summary>
    public void Buy()
    {
        ShopUI shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.BuyCard(cardId, price);
        }
        else
        {
            // Direct fallback
            if (PlayerProfile.coins < price)
            {
                Debug.LogWarning($"[ShopCardItem] Not enough coins. Needed: {price}, Owned: {PlayerProfile.coins}");
                return;
            }

            PlayerProfile.coins -= price;
            PlayerProfile.Save();
            PlayerCardData.AcquireFromShop(cardId);
            Debug.Log($"[ShopCardItem] Purchased {cardId} for {price} coins.");
        }
    }
}
