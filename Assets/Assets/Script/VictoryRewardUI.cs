using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictoryRewardUI : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject victoryPanel;
    public RectTransform panelToBounce; 
    public TMPro.TMP_Text rewardText;

    [Header("Card Reward")]
    public Transform cardContainer; 
    public CardUI cardPrefab;       

    [Header("Buttons")]
    public Button continueButton;

    private BattleManager battleManager;
    private string currentRewardCardId;
    private CardUI spawnedCard;

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        
        battleManager = FindObjectOfType<BattleManager>();
    }

    public void ShowVictory(int xp, int coins, string rewardCardId)
    {
        currentRewardCardId = rewardCardId;
        CardData data = CardDatabase.Get(rewardCardId);

        // I-update ang reward text (XP at Coins)
        if (rewardText != null)
            rewardText.text = $"+{xp} XP   +{coins} Coins";
        
        // Linisin ang lumang card (kung meron man)
        if (spawnedCard != null) Destroy(spawnedCard.gameObject);

        // I-instantiate ang bagong card prefab
        if (cardPrefab != null && cardContainer != null)
        {
        CardClass cClass = data.cardClass;
        CardUI prefabToUse = CardPrefabManager.Instance.GetPrefab(data.cardClass, data.id);
        spawnedCard = Instantiate(prefabToUse, cardContainer);
        spawnedCard.Setup(new CardInstance(data), (c) => {});
            
            // I-setup ang card (walang onClick function dahil reward lang ito)
            spawnedCard.Setup(new CardInstance(data), (c) => {});
        }

        if (victoryPanel != null) 
        {
            victoryPanel.SetActive(true);
            
            // Bounce effect
            if (panelToBounce != null)
                StartCoroutine(BounceEffect());
        }
    }

    IEnumerator BounceEffect()
    {
        Vector3 originalScale = Vector3.one;
        panelToBounce.localScale = Vector3.zero;

        float timer = 0f;
        float duration = 0.4f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float scale = Mathf.Sin(progress * Mathf.PI * 1.5f) * 1.1f;
            panelToBounce.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        timer = 0f;
        duration = 0.1f;
        Vector3 overshootScale = panelToBounce.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            panelToBounce.localScale = Vector3.Lerp(overshootScale, originalScale, progress);
            yield return null;
        }
        panelToBounce.localScale = originalScale;
    }

    void OnContinueClicked()
    {
        if (!string.IsNullOrEmpty(currentRewardCardId))
        {
            CardCollection.AddCard(currentRewardCardId);
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        if (battleManager != null)
        {
            battleManager.StartCoroutine(battleManager.ReturnToExploration());
        }
    }
}