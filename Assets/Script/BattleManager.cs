using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    enum State { Inactive, PlayerTurn, EnemyTurn, Ended }
    State state = State.Inactive;

    [Header("UI")]
    public GameObject battlePanel;
    public Transform handContainer;
    public CardUI cardPrefab;
    public TMP_Text deckCountText, discardCountText, resultText;
    public Button endTurnButton;

    [Header("Stats UI (Drag Correct UI Here!)")]
    public TMP_Text playerStatsTextUI; 
    public TMP_Text enemyStatsTextUI; 

    [Header("Bars")]
    public StatBar playerHpBar;
    public StatBar playerManaBar;
    public StatBar enemyHpBar;
    public StatBar enemyManaBar;

    EnemyData enemyData;
    CardFlow flow = new CardFlow();
    List<string> acquiredCards;
    bool lastVictory;

    public bool IsBusy { get { return state != State.Inactive; } }

    void Awake()
    {
        Instance = this;
        battlePanel.SetActive(false);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.SetupUIReferences(
                playerHpBar, playerManaBar, 
                enemyHpBar, enemyManaBar, 
                playerStatsTextUI, enemyStatsTextUI
            );
        }
    }

    void Start()
    {
        StartBattle(BattleTransfer.enemyId);
    }

    void Update()
    {
        if (state == State.Inactive || GameStatsManager.Instance == null) return;

        var gm = GameStatsManager.Instance;

        deckCountText.text = "Deck: " + gm.drawCards;
        discardCountText.text = "Discard: " + gm.discardCards;

        if (handContainer.childCount != gm.handCards)
        {
            RefreshHandVisuals();
        }
    }

    void RefreshHandVisuals()
    {
        var gm = GameStatsManager.Instance;
        foreach (Transform child in handContainer) Destroy(child.gameObject);
        
        for (int i = 0; i < gm.handCards; i++)
        {
            // FIXED: Changed "BasicStrikeCard001" to the new "BASIC-001" ID
            CardInstance c = (i < flow.hand.Count) ? flow.hand[i] : new CardInstance(CardDatabase.Get("BASIC-001"));
            CardUI ui = Instantiate(cardPrefab, handContainer);
            ui.Setup(c, OnCardClicked);
        }
    }

    public bool StartBattle(string enemyId)
    {
        if (state != State.Inactive) return false;

        enemyData = EnemyDatabase.Get(enemyId);
        var gm = GameStatsManager.Instance;

        gm.playerStats.ResetForBattle(fullHeal: true, startStamina: 1);
        gm.SetupEnemy(enemyData);

        acquiredCards = new List<string>();
        foreach (string id in CardDatabase.AllIds()) { acquiredCards.Add(id); acquiredCards.Add(id); }
        flow.StartBattle(acquiredCards, 3);

        gm.handCards = flow.hand.Count;
        gm.drawCards = flow.deck.Count;
        gm.discardCards = flow.discard.Count;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        resultText.text = "";
        battlePanel.SetActive(true);
        state = State.PlayerTurn;
        Refresh();
        return true;
    }

    void OnCardClicked(CardInstance card)
    {
        if (state != State.PlayerTurn) return;
        if (!flow.hand.Contains(card)) return;
        
        var gm = GameStatsManager.Instance;
        if (gm.playerStats.stamina < card.data.cost) { Debug.Log("Not enough Mana"); return; }

        gm.playerStats.stamina -= card.data.cost;
        ApplyEffect(card.data);
        flow.Discard(card);

        gm.handCards = flow.hand.Count;
        gm.drawCards = flow.deck.Count;
        gm.discardCards = flow.discard.Count;

        if (gm.activeEnemy != null && gm.activeEnemy.hp <= 0) { EndBattle(true); return; }
        Refresh();
    }

    void ApplyEffect(CardData c)
    {
        var gm = GameStatsManager.Instance;
        
        // FIXED: Extracts the base value from your new 'values' array
        int val = (c.values != null && c.values.Length > 0) ? c.values[0] : 0;

        switch (c.effect)
        {
            case CardEffectType.Damage:         
                gm.ApplyDamageToEnemy(val); 
                break;
            case CardEffectType.MagicDamage:
            case CardEffectType.DirectHpDamage:
                // For now, magic and direct damage just deal normal damage
                gm.ApplyDamageToEnemy(val); 
                break;
            case CardEffectType.RestoreMana: 
                gm.RestorePlayerStamina(val); 
                break;
            case CardEffectType.Armor:          
                gm.AddPlayerArmor(val); 
                break;
            case CardEffectType.ReduceNextAttack:
            case CardEffectType.DamageAndReduce:          
                gm.AddEnemyAttackReduction(val); 
                break;
            // Add other effects here as you implement them (Junk, MultiHit, etc.)
        }
    }

    void OnEndTurnClicked()
    {
        if (state != State.PlayerTurn) return;
        state = State.EnemyTurn;
        Refresh();
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        var gm = GameStatsManager.Instance;

        if (gm.activeEnemy == null) yield break;

        gm.activeEnemy.stamina = Mathf.Min(gm.activeEnemy.maxStamina, gm.activeEnemy.stamina + 1); 
        
        // FIXED: Since your new EnemyData doesn't have attackDamage/specialCost anymore,
        // I put a placeholder of 5 damage. You will need to implement an Enemy Card Deck here later!
        int attack = 5; 
        
        attack = Mathf.Max(0, attack - gm.activeEnemy.attackReduction);
        gm.activeEnemy.attackReduction = 0;

        gm.ApplyDamageToPlayer(attack); 
        gm.playerStats.armor = 0; 
        
        Refresh();
        yield return new WaitForSeconds(0.5f);

        if (gm.playerStats.hp <= 0) 
        { 
            EndBattle(false); 
            yield break; 
        }

        gm.RestorePlayerStamina(1);
        flow.Draw(2); 

        gm.handCards = flow.hand.Count;
        gm.drawCards = flow.deck.Count;
        gm.discardCards = flow.discard.Count;
        
        state = State.PlayerTurn;
        Refresh();
    }

    public void EndBattle(bool victory)
    {
        if (state == State.Ended || state == State.Inactive) return;
        state = State.Ended;
        lastVictory = victory;
        var gm = GameStatsManager.Instance;

        if (victory)
        {
            bool leveledUp = gm.AddReward();
            resultText.text = "VICTORY!\n+" + enemyData.xpReward + " XP   +" + enemyData.coinReward + " Coins";
            if (leveledUp) resultText.text += "\nLEVEL UP! Level " + gm.playerStats.level;
        }
        else
        {
            resultText.text = "DEFEAT";
        }

        Refresh();
        StartCoroutine(ReturnToExploration());
    }

    IEnumerator ReturnToExploration()
    {
        yield return new WaitForSeconds(3f);

        if (lastVictory && !string.IsNullOrEmpty(BattleTransfer.enemyKey))
            BattleTransfer.defeated.Add(BattleTransfer.enemyKey);

        GameStatsManager.Instance.ClearEnemy();
        state = State.Inactive;
        SceneManager.LoadScene(BattleTransfer.explorationScene);
    }

    public void Refresh()
    {
        var gm = GameStatsManager.Instance;

        if (playerHpBar != null)   playerHpBar.Set(gm.playerStats.hp, gm.playerStats.maxHp);
        if (playerManaBar != null) playerManaBar.Set(gm.playerStats.stamina, gm.playerStats.maxStamina);
        
        if (gm.activeEnemy != null)
        {
            if (enemyHpBar != null)    enemyHpBar.Set(gm.activeEnemy.hp, gm.activeEnemy.maxHp);
            if (enemyManaBar != null)  enemyManaBar.Set(gm.activeEnemy.stamina, gm.activeEnemy.maxStamina);
        }

        gm.RefreshUI();
        endTurnButton.interactable = (state == State.PlayerTurn);
    }
}