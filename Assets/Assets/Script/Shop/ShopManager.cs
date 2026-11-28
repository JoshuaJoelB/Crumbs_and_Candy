using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// SHOP MANAGER
/// Loads available shop items from ShopDatabase.
/// Displays 6 random cards at a time (refresh to re-roll).
/// Handles item selection, coin checks, purchases, and sends to PlayerCardData.
/// Updates the shop interface and connects to PlayerDataViewer.
/// </summary>
public class ShopManager : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR REFERENCES
    // =========================================================================

    [Header("UI Panels & Player")]
    public GameObject shopPanel;
    public Behaviour playerMovement;

    [Header("Card Display (6 Slots)")]
    [Tooltip("Parent container where the 6 shop card slots are spawned.")]
    public Transform cardDisplayContainer;

    [Header("Coin Display")]
    public TMP_Text coinBalanceText;

    [Header("Status Feedback")]
    public TMP_Text statusMessageText;

    [Header("Buttons")]
    public Button refreshButton;
    public Button leaveButton;

    [Header("Shop Settings")]
    [Tooltip("Number of cards shown in the shop at once.")]
    public int displayCount = 6;

    [Tooltip("Coin cost to refresh the shop selection. 0 = free.")]
    public int refreshCost = 0;

    // =========================================================================
    // INTERNAL STATE
    // =========================================================================

    private bool isOpen = false;
    private readonly List<ShopDatabase.ShopItem> currentOfferings = new List<ShopDatabase.ShopItem>();
    private readonly List<CardUI> spawnedCards = new List<CardUI>();

    public bool IsOpen => isOpen;

    /// <summary>
    /// Fired after a successful purchase so external systems can react.
    /// </summary>
    public static event Action<string> OnItemPurchased;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(CloseShop);
        }
    }

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void OnEnable()
    {
        PlayerCardData.OnCardDataChanged += OnPlayerDataChanged;
    }

    private void OnDisable()
    {
        PlayerCardData.OnCardDataChanged -= OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged()
    {
        if (isOpen)
        {
            UpdateCoinDisplay();
            RefreshCardDisplay(); // update sold-out states
        }
    }

    // =========================================================================
    // 1. OPEN / CLOSE SHOP
    // =========================================================================

    public void OpenShop()
    {
        if (isOpen) return;
        isOpen = true;

        PlayerProfile.Load();
        PlayerCardData.EnsureInitialized();

        if (shopPanel != null) shopPanel.SetActive(true);

        // Freeze player
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Roll new shop offerings
        RollShopOfferings();
        UpdateCoinDisplay();
        ClearStatusMessage();

        Debug.Log("[ShopManager] Shop opened.");
    }

    public void CloseShop()
    {
        if (!isOpen) return;
        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);

        // Restore player
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ClearSpawnedCards();
        Debug.Log("[ShopManager] Shop closed.");
    }

    /// <summary>
    /// Public alias for Unity button OnClick() binding.
    /// </summary>
    public void Leave()
    {
        CloseShop();
    }

    // =========================================================================
    // 2. ROLL / REFRESH SHOP OFFERINGS
    // =========================================================================

    /// <summary>
    /// Randomly picks 'displayCount' cards from the ShopDatabase to show.
    /// </summary>
    private void RollShopOfferings()
    {
        currentOfferings.Clear();

        List<ShopDatabase.ShopItem> available = ShopDatabase.GetAvailableItems();

        // Shuffle available items using Fisher-Yates
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = available[i];
            available[i] = available[j];
            available[j] = temp;
        }

        // Pick up to displayCount
        int count = Mathf.Min(displayCount, available.Count);
        for (int i = 0; i < count; i++)
        {
            currentOfferings.Add(available[i]);
        }

        RefreshCardDisplay();
    }

    /// <summary>
    /// Refresh button handler — re-rolls the shop offerings.
    /// </summary>
    public void OnRefreshClicked()
    {
        // Check refresh cost
        if (refreshCost > 0)
        {
            if (PlayerProfile.coins < refreshCost)
            {
                ShowStatusMessage($"Not enough coins to refresh! Need {refreshCost} coins.", Color.red);
                return;
            }

            PlayerProfile.coins -= refreshCost;
            PlayerProfile.Save();
        }

        RollShopOfferings();
        UpdateCoinDisplay();
        ShowStatusMessage("Shop refreshed!", Color.cyan);
        Debug.Log("[ShopManager] Shop refreshed.");
    }

    // =========================================================================
    // 3. DISPLAY CARDS IN UI
    // =========================================================================

    private void RefreshCardDisplay()
    {
        ClearSpawnedCards();

        if (cardDisplayContainer == null)
        {
            Debug.LogWarning("[ShopManager] cardDisplayContainer is not assigned!");
            return;
        }

        // Ensure container has a LayoutGroup so cards don't overlap
        if (cardDisplayContainer.GetComponent<LayoutGroup>() == null)
        {
            var grid = cardDisplayContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 220);
            grid.spacing = new Vector2(15, 15);
            grid.childAlignment = TextAnchor.MiddleCenter;
        }

        if (CardPrefabManager.Instance == null)
        {
            CardPrefabManager.Instance = FindObjectOfType<CardPrefabManager>();
        }

        foreach (var shopItem in currentOfferings)
        {
            CardData cardData = shopItem.CardInfo;
            if (cardData == null) continue;

            // Get the correct prefab based on class
            CardUI prefab = null;
            if (CardPrefabManager.Instance != null)
            {
                prefab = CardPrefabManager.Instance.GetPrefab(cardData.cardClass, cardData.id);
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[ShopManager] No prefab found for {shopItem.cardId} ({cardData.cardClass})");
                continue;
            }

            CardUI cardUI = Instantiate(prefab, cardDisplayContainer);
            CardInstance instance = new CardInstance(cardData, 1); // shop cards are always level 1

            // Setup the card display
            cardUI.Setup(instance, (clickedCard) => OnCardClicked(shopItem));

            // Show price on the cost text (override the mana cost with coin price)
            if (cardUI.costText != null)
            {
                cardUI.costText.text = shopItem.price.ToString();
            }

            // Grey out if already owned
            if (PlayerCardData.OwnsCard(shopItem.cardId))
            {
                SetCardOwned(cardUI);
            }

            spawnedCards.Add(cardUI);
        }
    }

    private void SetCardOwned(CardUI cardUI)
    {
        // Visual indicator that player already owns this card
        if (cardUI.nameText != null)
        {
            cardUI.nameText.text += " [OWNED]";
        }
    }

    // =========================================================================
    // 4. PURCHASE LOGIC
    // =========================================================================

    /// <summary>
    /// Called when a shop card is clicked. Processes the purchase.
    /// </summary>
    private void OnCardClicked(ShopDatabase.ShopItem shopItem)
    {
        if (shopItem == null) return;

        string cardId = shopItem.cardId;
        string cardName = shopItem.CardName;
        int price = shopItem.price;

        // --- Check if already owned ---
        if (PlayerCardData.OwnsCard(cardId))
        {
            ShowStatusMessage($"You already own {cardName}!", Color.yellow);
            Debug.Log($"[ShopManager] Player already owns {cardId}.");
            return;
        }

        // --- Check player coins (insufficient funds handling) ---
        if (PlayerProfile.coins < price)
        {
            ShowStatusMessage(
                $"Not enough coins! Need {price}, you have {PlayerProfile.coins}.",
                Color.red
            );
            Debug.LogWarning($"[ShopManager] Insufficient funds for {cardId}. Need: {price}, Have: {PlayerProfile.coins}");
            return;
        }

        // --- Check stock ---
        if (!shopItem.CanPurchase)
        {
            ShowStatusMessage($"{cardName} is sold out!", Color.yellow);
            return;
        }

        // --- Process purchase ---
        // 1. Deduct coins
        PlayerProfile.coins -= price;
        PlayerProfile.Save();

        // 2. Consume stock
        shopItem.ConsumeStock();

        // 3. Send purchased item to Inventory Manager (PlayerCardData)
        bool acquired = PlayerCardData.AcquireFromShop(cardId);

        if (acquired)
        {
            ShowStatusMessage(
                $"Purchased {cardName} for {price} coins!",
                Color.green
            );
            Debug.Log($"[ShopManager] Successfully purchased {cardId} ({cardName}) for {price} coins. Remaining: {PlayerProfile.coins}");

            // 4. Fire event for external listeners
            OnItemPurchased?.Invoke(cardId);

            // 5. Refresh PlayerDataViewer so Unity Inspector updates live
            PlayerDataViewer viewer = FindObjectOfType<PlayerDataViewer>();
            if (viewer != null)
            {
                viewer.RefreshViewer();
            }

            // 6. Update shop UI
            UpdateCoinDisplay();
            RefreshCardDisplay();
        }
        else
        {
            // Refund on failure
            PlayerProfile.coins += price;
            PlayerProfile.Save();
            ShowStatusMessage("Purchase failed! Coins refunded.", Color.red);
            Debug.LogError($"[ShopManager] AcquireFromShop failed for {cardId}. Coins refunded.");
        }
    }

    // =========================================================================
    // 5. UI HELPERS
    // =========================================================================

    /// <summary>
    /// Updates the coin balance display text.
    /// </summary>
    private void UpdateCoinDisplay()
    {
        if (coinBalanceText != null)
        {
            coinBalanceText.text = $"Balance: {PlayerProfile.coins} Coins";
        }
    }

    private void ShowStatusMessage(string message, Color color)
    {
        if (statusMessageText != null)
        {
            statusMessageText.text = message;
            statusMessageText.color = color;
        }
        Debug.Log($"[ShopManager] {message}");
    }

    private void ClearStatusMessage()
    {
        if (statusMessageText != null)
        {
            statusMessageText.text = "";
        }
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
