using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Blacksmith UI for inspecting and upgrading cards owned by the player.
/// Hides all Canvas UI elements when the player clicks Leave or on scene start,
/// and reveals them when interacting with the Anvil.
/// </summary>
public class BlacksmithUI : MonoBehaviour
{
    [Header("UI Panels & Player")]
    public GameObject blacksmithPanel;
    public Behaviour playerMovement;

    [Header("Cards List (Your cards - select one)")]
    public Transform cardsContainer;               // Assign 'Cardsown'
    public CardUI cardPrefab;                      // Fallback prefab if CardPrefabManager is not in scene

    [Header("Preview Section")]
    public Transform upgradedCardPreviewContainer; // Assign 'upgradedcard'
    public TMP_Text selectedCardNameText;          // Optional: 'Selected: [card]'
    public TMP_Text upgradePreviewStatsText;       // Assign 'Upgrade Preview:' Text
    public TMP_Text upgradeCostText;               // Optional separate cost text

    [Header("Coin Balance & Status Feedback")]
    public TMP_Text coinBalanceText;               // Assign 'CoinsPrev' Text
    public TMP_Text statusMessageText;             // Optional: for error/success messages

    [Header("Buttons")]
    public Button upgradeButton;                   // Assign 'Upgradebutton'
    public Button leaveButton;                     // Assign 'Leave'

    [Header("Upgrade Cost Configuration")]
    public int level1To2Cost = 25;
    public int level2To3Cost = 50;

    private readonly List<CardUI> spawnedCards = new List<CardUI>();
    private CardUI spawnedPreviewCard;
    private PlayerCardEntry selectedCardEntry;
    private bool isOpen = false;
    private bool isUpgrading = false;

    private Canvas canvasComponent;
    private GraphicRaycaster raycasterComponent;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        FindCanvas();
        AutoFindReferences();

        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(CloseBlacksmith);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }

    private void FindCanvas()
    {
        if (canvasComponent == null)
        {
            canvasComponent = GetComponent<Canvas>();
        }
        if (canvasComponent == null)
        {
            canvasComponent = GetComponentInParent<Canvas>();
        }
        if (canvasComponent == null)
        {
            canvasComponent = GetComponentInChildren<Canvas>(true);
        }
        if (canvasComponent == null)
        {
            GameObject cObj = GameObject.Find("Canvas");
            if (cObj != null) canvasComponent = cObj.GetComponent<Canvas>();
        }
        if (canvasComponent == null)
        {
            canvasComponent = FindObjectOfType<Canvas>(true);
        }

        if (canvasComponent != null)
        {
            raycasterComponent = canvasComponent.GetComponent<GraphicRaycaster>();
        }
    }

    private void AutoFindReferences()
    {
        Transform root = canvasComponent != null ? canvasComponent.transform : transform;

        if (cardsContainer == null)
        {
            Transform t = root.Find("Cardsown");
            if (t != null) cardsContainer = t;
        }

        if (upgradedCardPreviewContainer == null)
        {
            Transform t = root.Find("upgradedcard");
            if (t != null) upgradedCardPreviewContainer = t;
        }

        if (upgradeButton == null)
        {
            Transform b = root.Find("Upgradebutton");
            if (b != null) upgradeButton = b.GetComponent<Button>();
        }

        if (leaveButton == null)
        {
            Transform b = root.Find("Leave");
            if (b != null) leaveButton = b.GetComponent<Button>();
        }

        // Comprehensive search for leave button if name differs or nested
        if (leaveButton == null)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.name.ToLower().Contains("leave") || 
                    (btn.GetComponentInChildren<TMP_Text>() != null && btn.GetComponentInChildren<TMP_Text>().text.ToLower().Contains("leave")))
                {
                    leaveButton = btn;
                    break;
                }
            }
        }

        if (coinBalanceText == null)
        {
            Transform c = root.Find("CoinsPrev");
            if (c != null) coinBalanceText = c.GetComponentInChildren<TMP_Text>();
        }

        if (upgradePreviewStatsText == null)
        {
            Transform p = root.Find("Text (TMP)");
            if (p != null) upgradePreviewStatsText = p.GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        FindCanvas();
        AutoFindReferences();

        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveListener(CloseBlacksmith);
            leaveButton.onClick.AddListener(CloseBlacksmith);
        }

        // Hide all Canvas elements immediately when the scene starts playing
        HideCanvasUI();
    }

    private void OnEnable()
    {
        PlayerCardData.OnCardDataChanged += OnDataChanged;
    }

    private void OnDisable()
    {
        PlayerCardData.OnCardDataChanged -= OnDataChanged;
    }

    private void OnDataChanged()
    {
        if (isOpen)
        {
            UpdateCoinBalanceText();
        }
    }

    // =========================================================================
    // 1. OPEN / CLOSE BLACKSMITH (HIDE / SHOW CANVAS)
    // =========================================================================

    public void OpenBlacksmith()
    {
        if (isOpen) return;
        isOpen = true;

        FindCanvas();
        ShowCanvasUI();

        // Stop player movement
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Free mouse cursor for clicking UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ClearSelection();
        RefreshUI();
    }

    /// <summary>
    /// Triggered by the Leave button: Hides all UI in the canvas and restores gameplay.
    /// </summary>
    public void CloseBlacksmith()
    {
        isOpen = false;

        // Completely hide all elements in the canvas
        HideCanvasUI();

        // Restore player movement
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Re-lock mouse cursor to gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ClearSelection();
    }

    /// <summary>
    /// Public alias for button OnClick() events in the Unity Inspector.
    /// </summary>
    public void Leave()
    {
        CloseBlacksmith();
    }

    /// <summary>
    /// Public alias for button OnClick() events in the Unity Inspector.
    /// </summary>
    public void OnLeaveButtonClicked()
    {
        CloseBlacksmith();
    }

    private void ShowCanvasUI()
    {
        FindCanvas();

        if (canvasComponent != null)
        {
            if (!canvasComponent.gameObject.activeSelf)
            {
                canvasComponent.gameObject.SetActive(true);
            }
            canvasComponent.enabled = true;
        }

        if (raycasterComponent != null)
        {
            raycasterComponent.enabled = true;
        }

        if (blacksmithPanel != null && blacksmithPanel != gameObject)
        {
            blacksmithPanel.SetActive(true);
        }

        SetCanvasChildrenActive(true);
    }

    private void HideCanvasUI()
    {
        FindCanvas();

        // 1. Hide canvas component rendering
        if (canvasComponent != null)
        {
            canvasComponent.enabled = false;
        }

        if (raycasterComponent != null)
        {
            raycasterComponent.enabled = false;
        }

        // 2. Hide panel if separate
        if (blacksmithPanel != null && blacksmithPanel != gameObject)
        {
            blacksmithPanel.SetActive(false);
        }

        // 3. Hide all direct children under Canvas
        SetCanvasChildrenActive(false);
    }

    private void SetCanvasChildrenActive(bool active)
    {
        Transform root = canvasComponent != null ? canvasComponent.transform : transform;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            // Ignore interaction prompts if parented under canvas
            if (child.name.ToLower().Contains("prompt")) continue;
            child.gameObject.SetActive(active);
        }
    }

    // =========================================================================
    // 2. UI REFRESH & CARD POPULATION
    // =========================================================================

    public void RefreshUI()
    {
        UpdateCoinBalanceText();
        PopulateUpgradeableCards();
        UpdatePreviewDisplay();
    }

    private void UpdateCoinBalanceText()
    {
        if (coinBalanceText != null)
        {
            coinBalanceText.text = $"Balance: {PlayerProfile.coins} Coins";
        }
    }

    private void PopulateUpgradeableCards()
    {
        ClearSpawnedCards();

        if (cardsContainer == null) return;

        List<PlayerCardEntry> upgradeable = PlayerCardData.GetUpgradeableCards();

        if (upgradeable.Count == 0)
        {
            ShowStatusMessage("No upgradeable cards available. (All cards at MAX level or unowned)", Color.yellow);
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        foreach (var entry in upgradeable)
        {
            CardData data = entry.CardInfo;
            if (data == null) continue;

            CardUI prefabToUse = GetCardUIPrefab(data);
            if (prefabToUse != null)
            {
                CardUI spawned = Instantiate(prefabToUse, cardsContainer);
                CardInstance instance = entry.CreateInstance();
                
                // Clicking this card selects it for upgrade preview
                spawned.Setup(instance, (cardInst) => SelectCard(entry));
                spawnedCards.Add(spawned);
            }
        }

        // Auto-select the first card if nothing is selected or if previous selection was maxed out
        if (selectedCardEntry == null || !PlayerCardData.CanUpgradeCard(selectedCardEntry.cardId))
        {
            SelectCard(upgradeable[0]);
        }
        else
        {
            // Re-fetch the updated entry from PlayerCardData
            selectedCardEntry = PlayerCardData.GetCardEntry(selectedCardEntry.cardId);
            UpdatePreviewDisplay();
        }
    }

    private CardUI GetCardUIPrefab(CardData data)
    {
        if (CardPrefabManager.Instance != null)
        {
            return CardPrefabManager.Instance.GetPrefab(data.cardClass, data.id);
        }
        return cardPrefab;
    }

    // =========================================================================
    // 3. CARD SELECTION & UPGRADE PREVIEW
    // =========================================================================

    public void SelectCard(PlayerCardEntry entry)
    {
        selectedCardEntry = entry;
        ClearStatusMessage();
        UpdatePreviewDisplay();
    }

    private void UpdatePreviewDisplay()
    {
        if (spawnedPreviewCard != null)
        {
            Destroy(spawnedPreviewCard.gameObject);
            spawnedPreviewCard = null;
        }

        if (selectedCardEntry == null || selectedCardEntry.CardInfo == null)
        {
            if (selectedCardNameText != null) selectedCardNameText.text = "Selected: None";
            if (upgradePreviewStatsText != null) upgradePreviewStatsText.text = "Select a card to preview upgrade.";
            if (upgradeCostText != null) upgradeCostText.text = "";
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        int currentLvl = selectedCardEntry.level;
        int nextLvl = currentLvl + 1;
        bool canUpgrade = PlayerCardData.CanUpgradeCard(selectedCardEntry.cardId);
        int cost = GetUpgradeCost(selectedCardEntry);

        if (selectedCardNameText != null)
        {
            selectedCardNameText.text = $"Selected: {selectedCardEntry.CardName} (Lv. {currentLvl})";
        }

        if (upgradeCostText != null)
        {
            upgradeCostText.text = canUpgrade ? $"Cost: {cost} Coins" : "MAX LEVEL";
        }

        if (upgradePreviewStatsText != null)
        {
            if (canUpgrade)
            {
                CardInstance currentInstance = selectedCardEntry.CreateInstance();
                CardInstance nextInstance = new CardInstance(selectedCardEntry.CardInfo, nextLvl);

                string statChange = "";
                int curVal = currentInstance.Value;
                int nxtVal = nextInstance.Value;
                if (curVal != nxtVal)
                {
                    statChange += $"{selectedCardEntry.Effect}: {curVal} -> <color=#4CAF50><b>{nxtVal}</b></color>\n";
                }

                int curSec = currentInstance.Secondary;
                int nxtSec = nextInstance.Secondary;
                if (curSec != nxtSec)
                {
                    statChange += $"Secondary: {curSec} -> <color=#4CAF50><b>{nxtSec}</b></color>\n";
                }

                upgradePreviewStatsText.text = 
                    $"<b>Upgrade preview:</b>\n" +
                    $"Level: {currentLvl} -> <color=#4CAF50><b>{nextLvl}</b></color>\n" +
                    $"Upgrade Cost: <b>{cost} Coins</b>\n" +
                    statChange +
                    $"<i>\"{nextInstance.GetDescription()}\"</i>";
            }
            else
            {
                upgradePreviewStatsText.text = "<b>Upgrade Preview:</b>\nThis card is already at <color=#FFD700><b>MAX LEVEL</b></color>!";
            }
        }

        if (upgradedCardPreviewContainer != null && canUpgrade)
        {
            CardUI prefabToUse = GetCardUIPrefab(selectedCardEntry.CardInfo);
            if (prefabToUse != null)
            {
                spawnedPreviewCard = Instantiate(prefabToUse, upgradedCardPreviewContainer);
                CardInstance nextInstance = new CardInstance(selectedCardEntry.CardInfo, nextLvl);
                spawnedPreviewCard.Setup(nextInstance, (c) => {});
            }
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = canUpgrade;
        }
    }

    public int GetUpgradeCost(PlayerCardEntry entry)
    {
        if (entry == null) return 0;
        if (entry.level == 1) return level1To2Cost;
        if (entry.level == 2) return level2To3Cost;
        return level2To3Cost;
    }

    // =========================================================================
    // 4. CONFIRM UPGRADE ACTION
    // =========================================================================

    public void OnUpgradeClicked()
    {
        if (isUpgrading) return; // Prevent double clicks

        if (selectedCardEntry == null)
        {
            ShowStatusMessage("Please select a card to upgrade.", Color.yellow);
            return;
        }

        if (!PlayerCardData.CanUpgradeCard(selectedCardEntry.cardId))
        {
            ShowStatusMessage("Card is already at MAX level!", Color.yellow);
            return;
        }

        int cost = GetUpgradeCost(selectedCardEntry);

        // Check coin balance
        if (PlayerProfile.coins < cost)
        {
            ShowStatusMessage($"Insufficient Coins! (Need {cost} Coins, you have {PlayerProfile.coins})", Color.red);
            return;
        }

        isUpgrading = true;

        string cardId = selectedCardEntry.cardId;
        string cardName = selectedCardEntry.CardName;

        // Upgrade via PlayerCardData
        bool success = PlayerCardData.UpgradeCard(cardId, cost);

        if (success)
        {
            int newLevel = PlayerCardData.GetCardLevel(cardId);
            ShowStatusMessage($"Successfully upgraded {cardName} to Level {newLevel}!", Color.green);

            // Re-render Blacksmith UI
            RefreshUI();

            // Refresh PlayerDataViewer immediately so the Unity Inspector updates live
            PlayerDataViewer viewer = FindObjectOfType<PlayerDataViewer>();
            if (viewer != null)
            {
                viewer.RefreshViewer();
            }
        }
        else
        {
            ShowStatusMessage("Upgrade failed. Please check card status or coin balance.", Color.red);
        }

        isUpgrading = false;
    }

    // =========================================================================
    // 5. HELPERS
    // =========================================================================

    private void ShowStatusMessage(string msg, Color color)
    {
        if (statusMessageText != null)
        {
            statusMessageText.text = msg;
            statusMessageText.color = color;
        }
        else if (upgradePreviewStatsText != null && !msg.StartsWith("Successfully"))
        {
            upgradePreviewStatsText.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(color)}><b>{msg}</b></color>";
        }
        Debug.Log($"[BlacksmithUI] {msg}");
    }

    private void ClearStatusMessage()
    {
        if (statusMessageText != null) statusMessageText.text = "";
    }

    private void ClearSelection()
    {
        selectedCardEntry = null;
        ClearSpawnedCards();
        if (spawnedPreviewCard != null)
        {
            Destroy(spawnedPreviewCard.gameObject);
            spawnedPreviewCard = null;
        }
        ClearStatusMessage();
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
