using System.Collections.Generic;

/// <summary>
/// SHOP DATABASE
/// Stores shop item IDs, prices, stock info, and availability.
/// Only class cards (NOR, WAR, MAG, TRI, BER) are sold in the shop.
/// Provides shop information to the Shop Manager.
/// </summary>
public static class ShopDatabase
{
    /// <summary>
    /// Represents a single item listing in the shop.
    /// </summary>
    public class ShopItem
    {
        public string cardId;
        public int price;
        public int stock;         // -1 = unlimited
        public bool isAvailable;

        public ShopItem(string cardId, int price, int stock = -1)
        {
            this.cardId = cardId;
            this.price = price;
            this.stock = stock;
            this.isAvailable = true;
        }

        /// <summary>
        /// Gets the CardData from the general CardDatabase.
        /// </summary>
        public CardData CardInfo => CardDatabase.Exists(cardId) ? CardDatabase.Get(cardId) : null;
        public string CardName => CardInfo != null ? CardInfo.cardName : cardId;
        public CardClass CardClass => CardInfo != null ? CardInfo.cardClass : CardClass.Basic;

        /// <summary>
        /// Whether this item can currently be purchased (available + in stock).
        /// </summary>
        public bool CanPurchase => isAvailable && (stock == -1 || stock > 0);

        /// <summary>
        /// Consumes one unit of stock. Returns false if out of stock.
        /// </summary>
        public bool ConsumeStock()
        {
            if (stock == -1) return true; // unlimited
            if (stock <= 0) return false;
            stock--;
            if (stock == 0) isAvailable = false;
            return true;
        }
    }

    // =========================================================================
    // SHOP ITEM CATALOG — Only Class Cards (Shop, Chest & Enemy Rewards)
    // =========================================================================

    private static readonly Dictionary<string, ShopItem> shopCatalog = new Dictionary<string, ShopItem>();

    /// <summary>
    /// All card IDs that can appear in the shop. Matches the "Class Cards" table.
    /// </summary>
    public static readonly string[] ShopCardIds = new string[]
    {
        "NOR-001",  // Heavy Attack    — Normal,     Cost 2
        "NOR-002",  // Basic Guard     — Normal,     Cost 1
        "WAR-001",  // Parry           — Warrior,    Cost 1
        "WAR-002",  // Pierce          — Warrior,    Cost 2
        "MAG-001",  // Fireball        — Mage,       Cost 2
        "MAG-002",  // Frost Bind      — Mage,       Cost 2
        "TRI-001",  // Counter         — Trickster,  Cost 2
        "TRI-002",  // Prankster       — Trickster,  Cost 1
        "BER-001",  // Bloodthirst     — Berserker,  Cost 2
        "BER-002",  // Rampage         — Berserker,  Cost 3
    };

    /// <summary>
    /// Coin price multiplier applied to the card's mana cost to determine the shop price.
    /// Price = card.cost * PriceMultiplier
    /// </summary>
    public const int PriceMultiplier = 10;

    static ShopDatabase()
    {
        InitializeCatalog();
    }

    private static void InitializeCatalog()
    {
        shopCatalog.Clear();
        foreach (string id in ShopCardIds)
        {
            if (!CardDatabase.Exists(id)) continue;

            CardData card = CardDatabase.Get(id);
            int price = card.cost * PriceMultiplier; // e.g. cost 2 → 20 coins
            shopCatalog[id] = new ShopItem(id, price, stock: -1); // unlimited stock
        }
    }

    // =========================================================================
    // PUBLIC API
    // =========================================================================

    /// <summary>
    /// Returns the ShopItem for a given card ID, or null if not in the shop catalog.
    /// </summary>
    public static ShopItem GetItem(string cardId)
    {
        shopCatalog.TryGetValue(cardId, out var item);
        return item;
    }

    /// <summary>
    /// Whether a card ID is part of the shop catalog.
    /// </summary>
    public static bool IsShopCard(string cardId)
    {
        return shopCatalog.ContainsKey(cardId);
    }

    /// <summary>
    /// Returns the shop price for a card. Returns -1 if not a shop card.
    /// </summary>
    public static int GetPrice(string cardId)
    {
        if (shopCatalog.TryGetValue(cardId, out var item))
            return item.price;
        return -1;
    }

    /// <summary>
    /// Returns all items currently available for purchase.
    /// </summary>
    public static List<ShopItem> GetAvailableItems()
    {
        List<ShopItem> available = new List<ShopItem>();
        foreach (var item in shopCatalog.Values)
        {
            if (item.CanPurchase)
                available.Add(item);
        }
        return available;
    }

    /// <summary>
    /// Returns ALL shop items regardless of availability.
    /// </summary>
    public static List<ShopItem> GetAllItems()
    {
        return new List<ShopItem>(shopCatalog.Values);
    }

    /// <summary>
    /// Resets all stock and availability back to defaults.
    /// </summary>
    public static void ResetStock()
    {
        InitializeCatalog();
    }
}
