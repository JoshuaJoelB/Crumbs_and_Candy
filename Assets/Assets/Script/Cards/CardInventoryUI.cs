using System.Collections.Generic;
using UnityEngine;


public class CardInventoryUI : MonoBehaviour
{
    [Header("UI Containers")]
    public Transform inventoryContainer;
    public GameObject inventoryPanel;

    [Header("Optional Details View")]
    public TMPro.TMP_Text selectedCardNameText;
    public TMPro.TMP_Text selectedCardDescText;
    public TMPro.TMP_Text totalCardsCountText;

    private readonly List<CardUI> spawnedCards = new List<CardUI>();

    private void OnEnable()
    {
        PlayerCardData.OnCardDataChanged += RefreshInventory;
        RefreshInventory();
    }

    private void OnDisable()
    {
        PlayerCardData.OnCardDataChanged -= RefreshInventory;
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

 
    public void RefreshInventory()
    {
        ClearSpawnedCards();

        if (inventoryContainer == null) return;

        List<PlayerCardEntry> ownedCards = PlayerCardData.GetAllAcquiredCards();

        if (totalCardsCountText != null)
        {
            totalCardsCountText.text = $"Cards Owned: {ownedCards.Count}";
        }

        foreach (var entry in ownedCards)
        {
            CardData data = entry.CardInfo;
            if (data == null) continue;

            CardUI prefab = null;
            if (CardPrefabManager.Instance != null)
            {
                prefab = CardPrefabManager.Instance.GetPrefab(data.cardClass, data.id);
            }

            if (prefab != null)
            {
                CardUI cardUI = Instantiate(prefab, inventoryContainer);
                CardInstance instance = entry.CreateInstance();
                cardUI.Setup(instance, OnCardClicked);
                spawnedCards.Add(cardUI);
            }
        }
    }

    private void OnCardClicked(CardInstance card)
    {
        if (card == null) return;

        if (selectedCardNameText != null)
            selectedCardNameText.text = $"{card.data.cardName} (Lv. {card.Level})";

        if (selectedCardDescText != null)
            selectedCardDescText.text = card.GetDescription();

        Debug.Log($"[CardInventoryUI] Selected Card: {card.data.cardName} ({card.data.id})");
    }

    private void ClearSpawnedCards()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        spawnedCards.Clear();
    }
}
