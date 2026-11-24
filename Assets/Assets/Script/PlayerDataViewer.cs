using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable item to display card ownership and upgrade levels clearly in the Unity Inspector.
/// </summary>
[Serializable]
public class CardInspectorItem
{
    [Header("Card Overview")]
    [Tooltip("Formatted display name with Level number and upgrade badge. Unity uses this field as the foldout title.")]
    public string cardDisplayName;

    public string cardId;
    public string cardName;
    public int level;
    public int maxLevel;
    public bool isUpgraded;
    public bool isMaxLevel;
    public CardClass cardClass;
    public int quantity;
    public int cost;
    public CardAcquisitionSource acquiredFrom;
    [TextArea(2, 4)]
    public string description;

    public CardInspectorItem() { }

    public CardInspectorItem(PlayerCardEntry entry)
    {
        cardId = entry.cardId;
        cardName = entry.CardName;
        level = entry.level;
        maxLevel = entry.MaxLevel;
        isUpgraded = entry.level > 1;
        isMaxLevel = entry.level >= entry.MaxLevel;
        cardClass = entry.CardClass;
        quantity = entry.quantity;
        cost = entry.Cost;
        acquiredFrom = entry.acquisitionSource;
        description = entry.Description;

        // The first string field determines the title shown in the Unity Inspector's foldout list
        string upgradeTag = isMaxLevel ? " [MAX LEVEL]" : (isUpgraded ? $" [UPGRADED +{level - 1}]" : "");
        cardDisplayName = $"[Level {level}] {cardName} ({cardId}){upgradeTag}";
    }
}

/// <summary>
/// Serializable snapshot of player profile stats.
/// </summary>
[Serializable]
public class PlayerProfileSnapshot
{
    public int level;
    public int xp;
    public int xpToNext;
    public int coins;
    public int maxHp;

    public void Refresh()
    {
        level = PlayerProfile.level;
        xp = PlayerProfile.xp;
        xpToNext = PlayerProfile.XpToNext;
        coins = PlayerProfile.coins;
        maxHp = PlayerProfile.MaxHp;
    }
}

/// <summary>
/// Put this component on any GameObject in your scene to inspect, monitor, and debug
/// the player's profile data, acquired cards, upgrade levels, and deck in real time via the Unity Inspector.
/// </summary>
[ExecuteAlways]
public class PlayerDataViewer : MonoBehaviour
{
    [Header("----- LIVE PLAYER PROFILE -----")]
    public PlayerProfileSnapshot playerProfile = new PlayerProfileSnapshot();

    [Header("----- CARDS OVERVIEW SUMMARY -----")]
    public int totalOwnedCards = 0;
    public int totalUpgradedCards = 0;
    public int maxLevelCards = 0;

    [Header("----- OWNED CARDS (SHOWING CARD LEVELS) -----")]
    [Tooltip("All cards owned by the player with their current upgrade level [Level X].")]
    public List<CardInspectorItem> ownedCards = new List<CardInspectorItem>();

    [Header("----- ACTIVE BATTLE DECK (WITH LEVELS) -----")]
    [Tooltip("Cards assigned to the active battle deck with their current upgrade levels.")]
    public List<string> activeDeck = new List<string>();

    [Header("----- ACQUISITION AUDIT HISTORY -----")]
    [Tooltip("Log of all card acquisitions and where they came from.")]
    public List<CardAcquisitionLog> acquisitionHistory = new List<CardAcquisitionLog>();

    [Header("----- DEBUG CHEAT TOOLS -----")]
    [Tooltip("Type any Card ID here to test acquiring or upgrading via the Context Menu / Inspector buttons.")]
    public string debugCardId = "BASIC-001";
    public CardAcquisitionSource debugSource = CardAcquisitionSource.Shop;
    public int debugCardLevel = 1;

    private float refreshTimer = 0f;

    void OnEnable()
    {
        RefreshViewer();
        PlayerCardData.OnCardDataChanged += RefreshViewer;
    }

    void OnDisable()
    {
        PlayerCardData.OnCardDataChanged -= RefreshViewer;
    }

    void Start()
    {
        RefreshViewer();
    }

    void Update()
    {
        // Periodic auto-refresh every 0.5s in play mode so Inspector stays live
        if (Application.isPlaying)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 0.5f)
            {
                refreshTimer = 0f;
                RefreshViewer();
            }
        }
    }

    /// <summary>
    /// Reads live data from PlayerProfile and PlayerCardData and populates the inspector lists with updated levels.
    /// </summary>
    [ContextMenu("Refresh Live Data")]
    public void RefreshViewer()
    {
        // 1. Refresh Profile Stats
        playerProfile.Refresh();

        // 2. Refresh Owned Cards with Level display
        ownedCards.Clear();
        List<PlayerCardEntry> cards = PlayerCardData.GetAllAcquiredCards();
        
        totalOwnedCards = cards.Count;
        totalUpgradedCards = 0;
        maxLevelCards = 0;

        foreach (var c in cards)
        {
            CardInspectorItem item = new CardInspectorItem(c);
            ownedCards.Add(item);

            if (item.isUpgraded) totalUpgradedCards++;
            if (item.isMaxLevel) maxLevelCards++;
        }

        // 3. Refresh Active Deck with Level indicators
        activeDeck.Clear();
        List<string> deckCardIds = PlayerCardData.GetActiveDeck();
        foreach (var id in deckCardIds)
        {
            int lvl = PlayerCardData.GetCardLevel(id);
            int maxLvl = CardDatabase.Exists(id) ? CardDatabase.Get(id).maxLevel : 1;
            string cName = CardDatabase.Exists(id) ? CardDatabase.Get(id).cardName : id;

            string statusTag = lvl >= maxLvl ? " [MAX]" : (lvl > 1 ? $" [UPGRADED +{lvl - 1}]" : "");
            activeDeck.Add($"[Level {lvl}] {cName} ({id}){statusTag}");
        }

        // 4. Refresh History
        acquisitionHistory = PlayerCardData.GetAcquisitionHistory();
    }

    /// <summary>
    /// Grants the card typed in debugCardId to the player.
    /// </summary>
    [ContextMenu("Give Debug Card")]
    public void GiveDebugCard()
    {
        if (string.IsNullOrEmpty(debugCardId))
        {
            Debug.LogWarning("[PlayerDataViewer] Enter a Card ID in debugCardId first!");
            return;
        }

        bool success = PlayerCardData.AcquireCard(debugCardId, debugSource, debugCardLevel);
        if (success)
        {
            Debug.Log($"[PlayerDataViewer] Successfully gave {debugCardId} to player!");
            RefreshViewer();
        }
    }

    /// <summary>
    /// Tests upgrading the card typed in debugCardId directly from the Inspector.
    /// </summary>
    [ContextMenu("Debug: Upgrade Specified Card")]
    public void UpgradeDebugCard()
    {
        if (string.IsNullOrEmpty(debugCardId))
        {
            Debug.LogWarning("[PlayerDataViewer] Enter a Card ID in debugCardId first!");
            return;
        }

        if (!PlayerCardData.OwnsCard(debugCardId))
        {
            Debug.LogWarning($"[PlayerDataViewer] Player does not own {debugCardId}!");
            return;
        }

        if (!PlayerCardData.CanUpgradeCard(debugCardId))
        {
            Debug.LogWarning($"[PlayerDataViewer] {debugCardId} is already at MAX level!");
            return;
        }

        bool success = PlayerCardData.UpgradeCard(debugCardId, 0); // 0 coin cost for debug
        if (success)
        {
            int newLvl = PlayerCardData.GetCardLevel(debugCardId);
            Debug.Log($"[PlayerDataViewer] Upgraded {debugCardId} to Level {newLvl}!");
            RefreshViewer();
        }
    }

    /// <summary>
    /// Adds 100 coins to player for testing shop purchases and upgrades.
    /// </summary>
    [ContextMenu("Debug: Add 100 Coins")]
    public void AddDebugCoins()
    {
        PlayerProfile.coins += 100;
        PlayerProfile.Save();
        RefreshViewer();
        Debug.Log($"[PlayerDataViewer] Added 100 coins. Total: {PlayerProfile.coins}");
    }

    /// <summary>
    /// Resets all player card data back to default starter cards (BASIC-001, BASIC-002, BASIC-003).
    /// </summary>
    [ContextMenu("Reset Player Cards to Starter")]
    public void ResetCards()
    {
        PlayerCardData.ResetData();
        RefreshViewer();
        Debug.Log("[PlayerDataViewer] Player cards reset to starter cards.");
    }
}
