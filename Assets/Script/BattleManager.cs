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

    [Header("Main UI")]
    public GameObject battlePanel;
    public Transform handContainer;
    public CardUI cardPrefab;
    public Button endTurnButton;

    [Header("Text & Bars")]
    public TMP_Text playerStatsText;
    public TMP_Text enemyStatsText;
    public TMP_Text deckCountText;
    public TMP_Text discardCountText;
    public TMP_Text turnIndicatorText; 

    public StatBar playerHpBar;
    public StatBar playerManaBar;
    public StatBar enemyHpBar;
    public StatBar enemyManaBar; // Ibinalik natin ito base sa hiling mo

    [Header("Victory / Defeat UI")]
    public VictoryRewardUI victoryUI;
    public DefeatUI defeatUI;

    Combatant player, enemy;
    EnemyData enemyData;
    CardFlow flow = new CardFlow();
    
    // Enemy Deck System
    List<CardInstance> enemyDeck = new List<CardInstance>();
    List<CardInstance> enemyDiscard = new List<CardInstance>();
    CardInstance enemyUniqueCard;
    
    List<CardInstance> startingDeck;
    bool lastVictory;
    private CardInstance previewedEnemyCard;

    public bool IsBusy { get { return state != State.Inactive; } }

    void Awake()
    {
        Instance = this;
        battlePanel.SetActive(false);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    void Start()
    {
        StartBattle(BattleTransfer.enemyId);
    }

    // ---------- START ----------
    public bool StartBattle(string enemyId)
    {
        if (state != State.Inactive) return false;

        enemyData = EnemyDatabase.Get(enemyId);

        // Player Deck Setup
        startingDeck = new List<CardInstance>();
        List<string> ownedCardIds = CardCollection.GetOwnedCards();

        if (ownedCardIds.Count == 0)
        {
            CardCollection.AddCard("BASIC-001");
            CardCollection.AddCard("BASIC-002");
            CardCollection.AddCard("BASIC-003");
            ownedCardIds = CardCollection.GetOwnedCards();
        }

        foreach (string id in ownedCardIds)
        {
            int lvl = CardCollection.GetLevel(id);
            startingDeck.Add(new CardInstance(CardDatabase.Get(id), lvl));
            startingDeck.Add(new CardInstance(CardDatabase.Get(id), lvl));
        }

        // Enemy Deck Setup
        enemyDeck.Clear();
        enemyDiscard.Clear();
        
        if (!string.IsNullOrEmpty(enemyData.uniqueCardId))
            enemyUniqueCard = new CardInstance(CardDatabase.Get(enemyData.uniqueCardId), 1);

        foreach (var entry in enemyData.regularDeck)
            for (int i = 0; i < entry.copies; i++)
                enemyDeck.Add(new CardInstance(CardDatabase.Get(entry.cardId), entry.level));

        foreach (var entry in enemyData.classDeck)
            for (int i = 0; i < entry.copies; i++)
                enemyDeck.Add(new CardInstance(CardDatabase.Get(entry.cardId), entry.level));

        ShuffleEnemyDeck();

        player = new Combatant("Player", PlayerProfile.MaxHp, 5, 3);
        enemy = new Combatant(enemyData.enemyName, enemyData.maxHp, enemyData.maxMana, enemyData.startingMana);

        flow.StartBattleWithInstances(startingDeck, 3);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        battlePanel.SetActive(true);
        state = State.PlayerTurn;
        
        SetEnemyIntentLogic(); 
        Refresh();
        return true;
    }

    // ---------- PLAYER TURN ----------
    void OnCardClicked(CardInstance card)
    {
        if (state != State.PlayerTurn) return;
        if (!flow.hand.Contains(card)) return;
        if (card.data.effect == CardEffectType.Junk) return;
        if (player.stamina < card.data.cost) return;

        player.stamina -= card.data.cost;
        ApplyEffect(card);
        flow.Discard(card);

        if (enemy.hp <= 0) { EndBattle(true); return; }
        Refresh();
    }

    void ApplyEffect(CardInstance card)
    {
        CardData c = card.data;
        int v = card.Value, s = card.Secondary, t = card.Tertiary;

        switch (c.effect)
        {
            case CardEffectType.Damage:
            case CardEffectType.MagicDamage: enemy.TakeDamage(v); break;
            case CardEffectType.DirectHpDamage: enemy.TakeDirectDamage(v); if (s > 0) player.Heal(s); break;
            case CardEffectType.Armor: player.armor += v; break;
            case CardEffectType.RestoreMana: player.RestoreStamina(v); break;
            case CardEffectType.RestoreHp: player.Heal(v); break;
            case CardEffectType.ReduceNextAttack:
            case CardEffectType.DamageAndReduce: enemy.TakeDamage(v); enemy.attackReduction += s; break;
            case CardEffectType.MultiHit: for (int i = 0; i < t && enemy.hp > 0; i++) enemy.TakeDamage(v); break;
            case CardEffectType.NullifyNextCard: enemy.parryBlock = true; if (v > 0) flow.Draw(v); break;
            case CardEffectType.CounterNextAttack: enemy.counterIncoming = true; if (v > 0) flow.Draw(v); break;
            case CardEffectType.AddJunkCards: enemy.junkToAdd += v; break;
            case CardEffectType.ArmorAndDraw: player.armor += v; if (s > 0) flow.Draw(s); break;
        }
    }

    // ---------- TURNS ----------
    void OnEndTurnClicked()
    {
        if (state != State.PlayerTurn) return;
        flow.PurgeJunkFromHand();
        state = State.EnemyTurn;
        Refresh();
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);

        if (enemy.junkToAdd > 0) { flow.AddJunk(enemy.junkToAdd); enemy.junkToAdd = 0; }
        enemy.RestoreStamina(1);

        if (enemy.parryBlock) enemy.parryBlock = false;
        else
        {
            if (previewedEnemyCard != null)
            {
                enemy.stamina -= previewedEnemyCard.data.cost;
                yield return new WaitForSeconds(0.5f);
                ExecuteEnemyCard(previewedEnemyCard);
                
                if (previewedEnemyCard != enemyUniqueCard)
                {
                    enemyDeck.Remove(previewedEnemyCard);
                    enemyDiscard.Add(previewedEnemyCard);
                }
            }

            Refresh();
            yield return new WaitForSeconds(0.5f);

            if (enemy.hp <= 0) { EndBattle(true); yield break; }
            if (player.hp <= 0) { EndBattle(false); yield break; }
        }

        player.RestoreStamina(1);
        flow.Draw(2);
        state = State.PlayerTurn;
        SetEnemyIntentLogic(); 
        Refresh();
    }

    // ---------- ENEMY CARD SELECTION ----------
    void SetEnemyIntentLogic()
    {
        previewedEnemyCard = null;

        if (enemyUniqueCard != null && enemy.stamina >= enemyUniqueCard.data.cost)
        {
            previewedEnemyCard = enemyUniqueCard;
            return;
        }

        if (enemyDeck.Count > 0)
        {
            foreach (var card in enemyDeck)
            {
                if (enemy.stamina >= card.data.cost)
                {
                    previewedEnemyCard = card;
                    return;
                }
            }
            previewedEnemyCard = enemyDeck[0]; 
        }
        else
        {
            DrawEnemyCard();
            if (enemyDeck.Count > 0) previewedEnemyCard = enemyDeck[0];
        }
    }

    void ShuffleEnemyDeck()
    {
        for (int i = enemyDeck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardInstance temp = enemyDeck[i];
            enemyDeck[i] = enemyDeck[j];
            enemyDeck[j] = temp;
        }
    }

    void DrawEnemyCard()
    {
        if (enemyDeck.Count == 0)
        {
            enemyDeck.AddRange(enemyDiscard);
            enemyDiscard.Clear();
            ShuffleEnemyDeck();
        }
    }

    // ---------- ENEMY CARD EXECUTION ----------
    void ExecuteEnemyCard(CardInstance card)
    {
        int v = card.Value;
        int s = card.Secondary;

        switch (card.data.effect)
        {
            case CardEffectType.Damage:
            case CardEffectType.MagicDamage:
                {
                    int dmg = enemy.ResolveOutgoingAttack(v);
                    if (enemy.counterIncoming) { enemy.counterIncoming = false; enemy.TakeDirectDamage(dmg); }
                    else player.TakeDamage(dmg);
                    player.armor = 0;
                }
                break;

            case CardEffectType.RestoreMana: enemy.RestoreStamina(v); break;
            case CardEffectType.DirectHpDamage:
                {
                    int dmg = enemy.ResolveOutgoingAttack(v);
                    if (enemy.counterIncoming) { enemy.counterIncoming = false; enemy.TakeDirectDamage(dmg); }
                    else player.TakeDirectDamage(dmg);
                    if (s > 0) enemy.Heal(s);
                }
                break;

            case CardEffectType.DamageAndReduce:
                {
                    int dmg = enemy.ResolveOutgoingAttack(v);
                    if (enemy.counterIncoming) { enemy.counterIncoming = false; enemy.TakeDirectDamage(dmg); }
                    else player.TakeDamage(dmg);
                    player.attackReduction += s;
                    player.armor = 0;
                }
                break;

            case CardEffectType.ArmorAndDraw: enemy.armor += v; break;
            case CardEffectType.Armor: enemy.armor += v; break; 
            case CardEffectType.MultiHit: for(int i=0; i<card.Tertiary; i++) player.TakeDamage(card.Value); break;
            case CardEffectType.ReduceNextAttack:
                int dmgFrost = enemy.ResolveOutgoingAttack(v);
                player.TakeDamage(dmgFrost);
                player.attackReduction += s;
                player.armor = 0;
                break;
        }
    }

    // ---------- END ----------
    void EndBattle(bool victory)
    {
        if (state == State.Ended || state == State.Inactive) return;
        state = State.Ended;
        lastVictory = victory;

        if (victory)
        {
            PlayerProfile.AddReward(enemyData.xpReward, enemyData.coinReward);
            
            // BAGONG TAWAG PARA SA RANDOM CLASS CARD REWARD
            string rewardCardId = GetRandomClassCard();

            if (victoryUI != null)
            {
                battlePanel.SetActive(false);
                victoryUI.ShowVictory(enemyData.xpReward, enemyData.coinReward, rewardCardId);
            }
            else
            {
                battlePanel.SetActive(false);
                StartCoroutine(ReturnToExploration());
            }
        }
        else
        {
            if (defeatUI != null)
            {
                battlePanel.SetActive(false);
                defeatUI.ShowDefeat(enemyData.enemyName);
            }
            else
            {
                battlePanel.SetActive(false);
                StartCoroutine(ReturnToExploration());
            }
        }
        Refresh();
    }

    // BAGONG METHOD PARA SA RANDOM CLASS CARD
    string GetRandomClassCard()
    {
        if (enemyData.classDeck == null || enemyData.classDeck.Count == 0)
            return "BASIC-001"; // Fallback kung walang class card

        int randomIndex = Random.Range(0, enemyData.classDeck.Count);
        return enemyData.classDeck[randomIndex].cardId;
    }

    public IEnumerator ReturnToExploration()
    {
        yield return new WaitForSeconds(1f);
        if (lastVictory && !string.IsNullOrEmpty(BattleTransfer.enemyKey))
            BattleTransfer.defeated.Add(BattleTransfer.enemyKey);

        NPCBattleTrigger.ResetBattleLoading();
        state = State.Inactive;
        SceneManager.LoadScene(BattleTransfer.explorationScene);
    }

    // ---------- UI ----------
    void Refresh()
    {
        if (handContainer != null)
        {
            for (int i = handContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = handContainer.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        if (cardPrefab != null && flow.hand != null)
        {
            foreach (CardInstance c in flow.hand)
            {
                CardUI ui = Instantiate(cardPrefab, handContainer);
                ui.Setup(c, OnCardClicked);
            }
        }

        if (playerStatsText != null) playerStatsText.text = $"PLAYER Lv {PlayerProfile.level}\nArmor {player.armor}\nXP {PlayerProfile.xp}/{PlayerProfile.XpToNext}\nCoins {PlayerProfile.coins}";
        if (enemyStatsText != null) enemyStatsText.text = $"{enemyData.enemyName} ({enemyData.difficulty})";
        if (turnIndicatorText != null) turnIndicatorText.text = (state == State.PlayerTurn) ? "Player Turn" : "Enemy Turn";

        if (deckCountText != null) deckCountText.text = "Draw: " + flow.deck.Count;
        if (discardCountText != null) discardCountText.text = "Discard: " + flow.discard.Count;

        if (playerHpBar != null) playerHpBar.Set(player.hp, player.maxHp);
        if (playerManaBar != null) playerManaBar.Set(player.stamina, player.maxStamina);
        if (enemyHpBar != null) enemyHpBar.Set(enemy.hp, enemy.maxHp);
        if (enemyManaBar != null) enemyManaBar.Set(enemy.stamina, enemy.maxStamina);

        if (endTurnButton != null) endTurnButton.interactable = (state == State.PlayerTurn);
    }
}