using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable item to display card ownership clearly in the Unity Inspector.
/// </summary>
[Serializable]
public class CardInspectorItem
{
    public string cardId;
    public string cardName;
    public CardClass cardClass;
    public int level;
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
        cardClass = entry.CardClass;
        level = entry.level;
        quantity = entry.quantity;
        cost = entry.Cost;
        acquiredFrom = entry.acquisitionSource;
        description = entry.Description;
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
/// the player's profile data and all cards they own in real time via the Unity Inspector.
/// </summary>
[ExecuteAlways]
public class PlayerDataViewer : MonoBehaviour
{
    [Header("----- LIVE PLAYER PROFILE -----")]
    public PlayerProfileSnapshot playerProfile = new PlayerProfileSnapshot();

    [Header("----- OWNED CARDS (PLAYER CARD DATA) -----")]
    [Tooltip("All cards currently owned by the player, retrieved from PlayerCardData.")]
    public List<CardInspectorItem> ownedCards = new List<CardInspectorItem>();

    [Header("----- ACTIVE BATTLE DECK -----")]
    [Tooltip("Card IDs currently assigned to the active battle deck.")]
    public List<string> activeDeck = new List<string>();

    [Header("----- ACQUISITION AUDIT HISTORY -----")]
    [Tooltip("Log of all card acquisitions and where they came from.")]
    public List<CardAcquisitionLog> acquisitionHistory = new List<CardAcquisitionLog>();

    [Header("----- DEBUG CHEAT TOOLS -----")]
    [Tooltip("Type any Card ID here and right-click -> 'Give Debug Card' or click in context menu.")]
    public string debugCardIdToAdd = "NOR-001";
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
    /// Reads live data from PlayerProfile and PlayerCardData and populates the inspector lists.
    /// </summary>
    [ContextMenu("Refresh Live Data")]
    public void RefreshViewer()
    {
        // 1. Refresh Profile Stats
        playerProfile.Refresh();

        // 2. Refresh Owned Cards
        ownedCards.Clear();
        List<PlayerCardEntry> cards = PlayerCardData.GetAllAcquiredCards();
        foreach (var c in cards)
        {
            ownedCards.Add(new CardInspectorItem(c));
        }

        // 3. Refresh Active Deck
        activeDeck = PlayerCardData.GetActiveDeck();

        // 4. Refresh History
        acquisitionHistory = PlayerCardData.GetAcquisitionHistory();
    }

    /// <summary>
    /// Grants the card typed in debugCardIdToAdd to the player.
    /// </summary>
    [ContextMenu("Give Debug Card")]
    public void GiveDebugCard()
    {
        if (string.IsNullOrEmpty(debugCardIdToAdd))
        {
            Debug.LogWarning("[PlayerDataViewer] Enter a Card ID in debugCardIdToAdd first!");
            return;
        }

        bool success = PlayerCardData.AcquireCard(debugCardIdToAdd, debugSource, debugCardLevel);
        if (success)
        {
            Debug.Log($"[PlayerDataViewer] Successfully gave {debugCardIdToAdd} to player!");
            RefreshViewer();
        }
    }

    /// <summary>
    /// Adds 100 coins to player for testing shop purchases.
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
