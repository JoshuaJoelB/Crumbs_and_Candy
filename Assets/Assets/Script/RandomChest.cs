using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles chest interactions and awards cards to the player via PlayerCardData with the RandomChest source.
/// </summary>
public class RandomChest : MonoBehaviour
{
    [Header("Chest Card Pool")]
    public List<string> possibleCardIds = new List<string> { "WAR-001", "WAR-002", "MAG-001", "MAG-002", "TRI-001", "TRI-002", "BER-001", "BER-002" };
    public int coinRewardMin = 5;
    public int coinRewardMax = 20;

    [Header("Chest State")]
    public bool isOpened = false;
    public bool autoOpenOnTrigger = false;

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    /// <summary>
    /// Opens the chest, awards a random card to PlayerCardData, and awards bonus coins.
    /// </summary>
    public string OpenChest()
    {
        if (isOpened) return null;
        isOpened = true;

        string rewardedCardId = "BASIC-001";
        if (possibleCardIds != null && possibleCardIds.Count > 0)
        {
            int index = Random.Range(0, possibleCardIds.Count);
            rewardedCardId = possibleCardIds[index];
        }

        // Record card acquisition with RandomChest source
        PlayerCardData.AcquireFromRandomChest(rewardedCardId);

        // Award bonus coins
        int coins = Random.Range(coinRewardMin, coinRewardMax + 1);
        if (coins > 0)
        {
            PlayerProfile.coins += coins;
            PlayerProfile.Save();
        }

        Debug.Log($"[RandomChest] Chest opened! Card reward: {rewardedCardId}, Coins: {coins}");
        return rewardedCardId;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (autoOpenOnTrigger && !isOpened)
            {
                OpenChest();
            }
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
