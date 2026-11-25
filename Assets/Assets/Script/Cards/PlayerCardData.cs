using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardAcquisitionSource
{
    InitialDeck,
    Shop,
    EnemyReward,
    RandomChest
}

[Serializable]
public class PlayerCardEntry
{
    public string cardId;
    public int level;
    public int quantity;
    public CardAcquisitionSource acquisitionSource;
    public string acquiredDate;

    public PlayerCardEntry() { }

    public PlayerCardEntry(string cardId, int level = 1, int quantity = 1, CardAcquisitionSource source = CardAcquisitionSource.EnemyReward)
    {
        this.cardId = cardId;
        this.level = Mathf.Max(1, level);
        this.quantity = Mathf.Max(1, quantity);
        this.acquisitionSource = source;
        this.acquiredDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Retrieves actual card details from the existing General Card Data (CardDatabase).
    /// </summary>
    public CardData CardInfo => CardDatabase.Exists(cardId) ? CardDatabase.Get(cardId) : null;

    public string CardName => CardInfo != null ? CardInfo.cardName : cardId;
    public string Description => CardInfo != null ? CardInfo.description : "";
    public CardClass CardClass => CardInfo != null ? CardInfo.cardClass : CardClass.Basic;
    public CardEffectType Effect => CardInfo != null ? CardInfo.effect : CardEffectType.Damage;
    public int Cost => CardInfo != null ? CardInfo.cost : 0;
    public int MaxLevel => CardInfo != null ? CardInfo.maxLevel : 1;

    /// <summary>
    /// Creates a usable CardInstance for UI or battle using the General Card Data and this card's level.
    /// </summary>
    public CardInstance CreateInstance()
    {
        if (!CardDatabase.Exists(cardId)) return null;
        return new CardInstance(CardDatabase.Get(cardId), level);
    }
}

/// <summary>
/// Audit record of an individual card acquisition event.
/// </summary>
[Serializable]
public class CardAcquisitionLog
{
    public string cardId;
    public CardAcquisitionSource source;
    public string timestamp;

    public CardAcquisitionLog() { }

    public CardAcquisitionLog(string cardId, CardAcquisitionSource source)
    {
        this.cardId = cardId;
        this.source = source;
        this.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// Serializable wrapper for saving player card data via JsonUtility.
/// </summary>
[Serializable]
public class PlayerCardCollectionSaveData
{
    public List<PlayerCardEntry> cards = new List<PlayerCardEntry>();
    public List<CardAcquisitionLog> history = new List<CardAcquisitionLog>();
    public List<string> activeDeck = new List<string>();
}

/// <summary>
/// SYSTEM: PLAYER CARD DATA
/// Manages player ownership of acquired cards, inventory access, deck management, and persistence.
/// </summary>
public static class PlayerCardData
{
    private const string SaveKey = "PlayerCardData_Collection";
    private static readonly Dictionary<string, PlayerCardEntry> ownedCards = new Dictionary<string, PlayerCardEntry>();
    private static readonly List<CardAcquisitionLog> acquisitionHistory = new List<CardAcquisitionLog>();
    private static readonly List<string> activeDeck = new List<string>();
    private static bool isInitialized = false;

    // Events for UI or gameplay reactions
    public static event Action<PlayerCardEntry, CardAcquisitionSource> OnCardAcquired;
    public static event Action OnCardDataChanged;

    static PlayerCardData()
    {
        EnsureInitialized();
    }

    public static void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;
        Load();
    }

    // =========================================================================
    // 1. ACQUISITION METHODS
    // =========================================================================

    /// <summary>
    /// Record a card obtained from the Shop.
    /// </summary>
    public static bool AcquireFromShop(string cardId, int level = 1, int quantity = 1)
    {
        return AcquireCard(cardId, CardAcquisitionSource.Shop, level, quantity);
    }

    /// <summary>
    /// Record a card obtained from Enemy Rewards.
    /// </summary>
    public static bool AcquireFromEnemyReward(string cardId, int level = 1, int quantity = 1)
    {
        return AcquireCard(cardId, CardAcquisitionSource.EnemyReward, level, quantity);
    }

    /// <summary>
    /// Record a card obtained from a Random Chest.
    /// </summary>
    public static bool AcquireFromRandomChest(string cardId, int level = 1, int quantity = 1)
    {
        return AcquireCard(cardId, CardAcquisitionSource.RandomChest, level, quantity);
    }

    /// <summary>
    /// Core acquisition method: handles validation, duplicate rules, fixed copies/levels, history logging, and saving.
    /// </summary>
    public static bool AcquireCard(string cardId, CardAcquisitionSource source, int level = 1, int quantity = 1)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(cardId))
        {
            Debug.LogWarning("[PlayerCardData] Cannot acquire card with null or empty ID.");
            return false;
        }

        if (!CardDatabase.Exists(cardId))
        {
            Debug.LogError($"[PlayerCardData] Card ID '{cardId}' does not exist in General Card Data (CardDatabase).");
            return false;
        }

        CardData generalCard = CardDatabase.Get(cardId);
        int clampedLevel = Mathf.Clamp(level, 1, generalCard.maxLevel);

        // Record the acquisition attempt in history
        acquisitionHistory.Add(new CardAcquisitionLog(cardId, source));

        PlayerCardEntry entry;
        if (ownedCards.TryGetValue(cardId, out entry))
        {
            // DUPLICATE RULE:
            // Card copies and levels are fixed; no random card upgrades.
            // If the player already owns this card, we keep the existing level (capped at maxLevel)
            // and increment copies/quantity if appropriate.
            entry.quantity += Mathf.Max(1, quantity);
            Debug.Log($"[PlayerCardData] Duplicate acquired: {cardId} from {source}. Total copies: {entry.quantity}");
        }
        else
        {
            // New card acquired
            entry = new PlayerCardEntry(cardId, clampedLevel, quantity, source);
            ownedCards.Add(cardId, entry);

            if (!activeDeck.Contains(cardId))
            {
                activeDeck.Add(cardId);
            }

            Debug.Log($"[PlayerCardData] New card acquired: {cardId} ({generalCard.cardName}) from {source} at Level {clampedLevel}");
        }

        Save();

        OnCardAcquired?.Invoke(entry, source);
        OnCardDataChanged?.Invoke();
        return true;
    }

    // =========================================================================
    // CARD UPGRADE SYSTEM (BLACKSMITH)
    // =========================================================================

    /// <summary>
    /// Checks whether an owned card can be upgraded to the next level.
    /// Returns true only if player owns the card and current level is less than max level.
    /// </summary>
    public static bool CanUpgradeCard(string cardId)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(cardId) || !ownedCards.ContainsKey(cardId)) return false;
        PlayerCardEntry entry = ownedCards[cardId];
        if (entry == null || entry.CardInfo == null) return false;
        return entry.level < entry.MaxLevel;
    }

    /// <summary>
    /// Returns all upgradeable cards currently owned by the player.
    /// </summary>
    public static List<PlayerCardEntry> GetUpgradeableCards()
    {
        EnsureInitialized();
        List<PlayerCardEntry> list = new List<PlayerCardEntry>();
        foreach (var entry in ownedCards.Values)
        {
            if (entry != null && entry.CardInfo != null && entry.level < entry.MaxLevel)
            {
                list.Add(entry);
            }
        }
        return list;
    }

    /// <summary>
    /// Upgrades an owned card by 1 level by spending coins.
    /// Deducts coins only on success and prevents duplicating the card.
    /// </summary>
    public static bool UpgradeCard(string cardId, int coinCost)
    {
        EnsureInitialized();

        if (!CanUpgradeCard(cardId))
        {
            Debug.LogWarning($"[PlayerCardData] Cannot upgrade card {cardId}. Not owned or already at max level.");
            return false;
        }

        if (PlayerProfile.coins < coinCost)
        {
            Debug.LogWarning($"[PlayerCardData] Insufficient coins to upgrade {cardId}. Needed: {coinCost}, Current: {PlayerProfile.coins}");
            return false;
        }

        // Deduct coins and persist player stats
        PlayerProfile.coins -= coinCost;
        PlayerProfile.Save();

        // Increment card level safely without duplicating
        PlayerCardEntry entry = ownedCards[cardId];
        entry.level = Mathf.Min(entry.level + 1, entry.MaxLevel);

        Save();

        OnCardDataChanged?.Invoke();
        Debug.Log($"[PlayerCardData] Successfully upgraded {cardId} ({entry.CardName}) to Level {entry.level}. Remaining Coins: {PlayerProfile.coins}");
        return true;
    }

    // =========================================================================
    // 2. CARD INVENTORY & IDENTIFICATION
    // =========================================================================

    public static bool HasCard(string cardId)
    {
        EnsureInitialized();
        return !string.IsNullOrEmpty(cardId) && ownedCards.ContainsKey(cardId);
    }

    public static bool OwnsCard(string cardId)
    {
        return HasCard(cardId);
    }

    public static int GetCardLevel(string cardId)
    {
        EnsureInitialized();
        if (ownedCards.TryGetValue(cardId, out var entry))
        {
            return entry.level;
        }
        return 1;
    }

    public static int GetCardQuantity(string cardId)
    {
        EnsureInitialized();
        if (ownedCards.TryGetValue(cardId, out var entry))
        {
            return entry.quantity;
        }
        return 0;
    }

    public static PlayerCardEntry GetCardEntry(string cardId)
    {
        EnsureInitialized();
        ownedCards.TryGetValue(cardId, out var entry);
        return entry;
    }

    /// <summary>
    /// Returns all card entries owned by the player.
    /// </summary>
    public static List<PlayerCardEntry> GetAllAcquiredCards()
    {
        EnsureInitialized();
        return new List<PlayerCardEntry>(ownedCards.Values);
    }

    /// <summary>
    /// Returns the list of all owned card IDs.
    /// </summary>
    public static List<string> GetOwnedCardIds()
    {
        EnsureInitialized();
        return new List<string>(ownedCards.Keys);
    }

    /// <summary>
    /// Returns CardInstances of all owned cards for display in UI.
    /// </summary>
    public static List<CardInstance> GetInventoryCardInstances()
    {
        EnsureInitialized();
        List<CardInstance> instances = new List<CardInstance>();
        foreach (var entry in ownedCards.Values)
        {
            CardInstance inst = entry.CreateInstance();
            if (inst != null) instances.Add(inst);
        }
        return instances;
    }

    /// <summary>
    /// Returns CardInstances representing the player's deck for battles.
    /// </summary>
    public static List<CardInstance> GetDeckCardInstances(int copiesPerCard = 2)
    {
        EnsureInitialized();
        List<CardInstance> deck = new List<CardInstance>();
        List<string> deckIds = activeDeck.Count > 0 ? activeDeck : GetOwnedCardIds();

        foreach (string id in deckIds)
        {
            if (ownedCards.TryGetValue(id, out var entry))
            {
                for (int i = 0; i < copiesPerCard; i++)
                {
                    CardInstance inst = entry.CreateInstance();
                    if (inst != null) deck.Add(inst);
                }
            }
        }
        return deck;
    }

    // =========================================================================
    // 3. DECK MANAGEMENT
    // =========================================================================

    public static List<string> GetActiveDeck()
    {
        EnsureInitialized();
        return new List<string>(activeDeck);
    }

    public static void SetActiveDeck(List<string> cardIds)
    {
        EnsureInitialized();
        activeDeck.Clear();
        if (cardIds != null)
        {
            foreach (string id in cardIds)
            {
                if (HasCard(id) && !activeDeck.Contains(id))
                {
                    activeDeck.Add(id);
                }
            }
        }
        Save();
        OnCardDataChanged?.Invoke();
    }

    public static bool AddCardToDeck(string cardId)
    {
        EnsureInitialized();
        if (!HasCard(cardId) || activeDeck.Contains(cardId)) return false;
        activeDeck.Add(cardId);
        Save();
        OnCardDataChanged?.Invoke();
        return true;
    }

    public static bool RemoveCardFromDeck(string cardId)
    {
        EnsureInitialized();
        if (activeDeck.Remove(cardId))
        {
            Save();
            OnCardDataChanged?.Invoke();
            return true;
        }
        return false;
    }

    // =========================================================================
    // 4. ACQUISITION HISTORY
    // =========================================================================

    public static List<CardAcquisitionLog> GetAcquisitionHistory()
    {
        EnsureInitialized();
        return new List<CardAcquisitionLog>(acquisitionHistory);
    }

    public static List<CardAcquisitionLog> GetAcquisitionsBySource(CardAcquisitionSource source)
    {
        EnsureInitialized();
        return acquisitionHistory.FindAll(log => log.source == source);
    }

    // =========================================================================
    // 5. DATA MANAGEMENT & PERSISTENCE
    // =========================================================================

    public static void Save()
    {
        PlayerCardCollectionSaveData data = new PlayerCardCollectionSaveData();
        data.cards = new List<PlayerCardEntry>(ownedCards.Values);
        data.history = new List<CardAcquisitionLog>(acquisitionHistory);
        data.activeDeck = new List<string>(activeDeck);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);

        // Also sync legacy PlayerPrefs keys for full backward compatibility
        foreach (var entry in ownedCards.Values)
        {
            PlayerPrefs.SetInt("card_owned_" + entry.cardId, 1);
            PlayerPrefs.SetInt("card_level_" + entry.cardId, entry.level);
        }

        PlayerPrefs.Save();
    }

    public static void Load()
    {
        ownedCards.Clear();
        acquisitionHistory.Clear();
        activeDeck.Clear();

        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    PlayerCardCollectionSaveData data = JsonUtility.FromJson<PlayerCardCollectionSaveData>(json);
                    if (data != null && data.cards != null)
                    {
                        foreach (var card in data.cards)
                        {
                            if (CardDatabase.Exists(card.cardId))
                            {
                                ownedCards[card.cardId] = card;
                            }
                        }

                        if (data.history != null)
                        {
                            acquisitionHistory.AddRange(data.history);
                        }

                        if (data.activeDeck != null)
                        {
                            activeDeck.AddRange(data.activeDeck);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[PlayerCardData] Failed to parse save data JSON: " + e.Message);
                }
            }
        }
        else
        {
            // Backward-compatibility: check legacy PlayerPrefs keys
            foreach (string id in CardDatabase.AllIds())
            {
                if (PlayerPrefs.GetInt("card_owned_" + id, 0) == 1)
                {
                    int level = PlayerPrefs.GetInt("card_level_" + id, 1);
                    ownedCards[id] = new PlayerCardEntry(id, level, 1, CardAcquisitionSource.InitialDeck);
                    activeDeck.Add(id);
                }
            }
        }

        // If player has no cards, initialize starter basic cards
        if (ownedCards.Count == 0)
        {
            InitializeStarterCards();
        }
    }

    private static void InitializeStarterCards()
    {
        string[] starterCards = new[] { "BASIC-001", "BASIC-002", "BASIC-003" };
        foreach (string id in starterCards)
        {
            if (CardDatabase.Exists(id))
            {
                PlayerCardEntry entry = new PlayerCardEntry(id, 1, 1, CardAcquisitionSource.InitialDeck);
                ownedCards[id] = entry;
                activeDeck.Add(id);
                acquisitionHistory.Add(new CardAcquisitionLog(id, CardAcquisitionSource.InitialDeck));
            }
        }
        Save();
    }

    /// <summary>
    /// Resets all player card data and re-initializes starter cards.
    /// </summary>
    public static void ResetData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        foreach (string id in CardDatabase.AllIds())
        {
            PlayerPrefs.DeleteKey("card_owned_" + id);
            PlayerPrefs.DeleteKey("card_level_" + id);
        }
        PlayerPrefs.Save();

        ownedCards.Clear();
        acquisitionHistory.Clear();
        activeDeck.Clear();

        InitializeStarterCards();
        OnCardDataChanged?.Invoke();
    }
}
