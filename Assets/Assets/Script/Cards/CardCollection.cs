using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridge interface to PlayerCardData for backwards-compatibility with existing battle and reward systems.
/// Delegates all storage, retrieval, and acquisition to the Player Card Data system.
/// </summary>
public static class CardCollection
{
    public static List<string> GetOwnedCards()
    {
        return PlayerCardData.GetOwnedCardIds();
    }

    public static int GetLevel(string cardId)
    {
        return PlayerCardData.GetCardLevel(cardId);
    }

    public static void AddCard(string cardId)
    {
        PlayerCardData.AcquireFromEnemyReward(cardId);
    }

    public static void AddCard(string cardId, CardAcquisitionSource source)
    {
        PlayerCardData.AcquireCard(cardId, source);
    }

    public static bool HasCard(string cardId)
    {
        return PlayerCardData.HasCard(cardId);
    }

    public static List<PlayerCardEntry> GetAllCards()
    {
        return PlayerCardData.GetAllAcquiredCards();
    }

    public static bool CanUpgradeCard(string cardId)
    {
        return PlayerCardData.CanUpgradeCard(cardId);
    }

    public static bool UpgradeCard(string cardId, int coinCost)
    {
        return PlayerCardData.UpgradeCard(cardId, coinCost);
    }

    public static List<PlayerCardEntry> GetUpgradeableCards()
    {
        return PlayerCardData.GetUpgradeableCards();
    }

    public static List<CardInstance> GetCardInstances()
    {
        return PlayerCardData.GetInventoryCardInstances();
    }

    public static void Save()
    {
        PlayerCardData.Save();
    }

    public static void Load()
    {
        PlayerCardData.Load();
    }

    public static void Reset()
    {
        PlayerCardData.ResetData();
    }
}
