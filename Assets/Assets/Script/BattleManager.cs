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
    
    [Header("Card UI (Drag these from Hierarchy)")]
    public Transform handContainer;    
    public CardUI cardPrefab;
    public TMP_Text deckCountText;     
    public TMP_Text discardCountText;  
    public Button endTurnButton;

    [Header("Text & Bars")]
    public TMP_Text playerStatsText;
    public TMP_Text enemyStatsText;
    public TMP_Text turnIndicatorText; 

    public StatBar playerHpBar;
    public StatBar playerManaBar;
    public StatBar enemyHpBar;
    public StatBar enemyManaBar;

    [Header("Victory / Defeat UI")]
    public VictoryRewardUI victoryUI;
    public DefeatUI defeatUI;

    Combatant player, enemy;
    EnemyData enemyData;
    CardFlow flow = new CardFlow();
    
    List<CardInstance> enemyDeck = new List<CardInstance>();
    List<CardInstance> enemyDiscard = new List<CardInstance>();
    CardInstance enemyUniqueCard;
    
    List<CardInstance> startingDeck;
    bool lastVictory;
    private CardInstance previewedEnemyCard;

    // --- VARIABLES TO TRACK INSPECTOR CHANGES ---
    private int lastSyncedHand = -1;
    private int lastSyncedDraw = -1;
    private int lastSyncedDiscard = -1;

    private int lastPlayerHp = -1;
    private int lastPlayerMaxHp = -1;
    private int lastPlayerMana = -1;
    private int lastPlayerMaxMana = -1;
    private int lastPlayerArmor = -1;

    private int lastEnemyHp = -1;
    private int lastEnemyMaxHp = -1;
    private int lastEnemyMana = -1;
    private int lastEnemyMaxMana = -1;
    private int lastEnemyArmor = -1;

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

    // --- NAG-SYNC NG INSPECTOR PERO TUMITIGIL KAPAG TAPOS NA ANG LABAN ---
    void Update()
    {
        if (state == State.Inactive || state == State.Ended || GameStatsManager.Instance == null || player == null || enemy == null) return;
        var gm = GameStatsManager.Instance;

        bool needsRefresh = false;

        if (gm.playerStats.hp != lastPlayerHp) { player.hp = gm.playerStats.hp; needsRefresh = true; }
        if (gm.playerStats.maxHp != lastPlayerMaxHp) { player.maxHp = gm.playerStats.maxHp; needsRefresh = true; }
        if (gm.playerStats.stamina != lastPlayerMana) { player.stamina = gm.playerStats.stamina; needsRefresh = true; }
        if (gm.playerStats.maxStamina != lastPlayerMaxMana) { player.maxStamina = gm.playerStats.maxStamina; needsRefresh = true; }
        if (gm.playerStats.armor != lastPlayerArmor) { player.armor = gm.playerStats.armor; needsRefresh = true; }

        if (gm.activeEnemy != null)
        {
            if (gm.activeEnemy.hp != lastEnemyHp) { enemy.hp = gm.activeEnemy.hp; needsRefresh = true; }
            if (gm.activeEnemy.maxHp != lastEnemyMaxHp) { enemy.maxHp = gm.activeEnemy.maxHp; needsRefresh = true; }
            if (gm.activeEnemy.stamina != lastEnemyMana) { enemy.stamina = gm.activeEnemy.stamina; needsRefresh = true; }
            if (gm.activeEnemy.maxStamina != lastEnemyMaxMana) { enemy.maxStamina = gm.activeEnemy.maxStamina; needsRefresh = true; }
            if (gm.activeEnemy.armor != lastEnemyArmor) { enemy.armor = gm.activeEnemy.armor; needsRefresh = true; }
        }

        if (gm.handCards != lastSyncedHand) { ForceHandCount(gm.handCards); needsRefresh = true; }
        if (gm.drawCards != lastSyncedDraw) { ForceDrawCount(gm.drawCards); needsRefresh = true; }
        if (gm.discardCards != lastSyncedDiscard) { ForceDiscardCount(gm.discardCards); needsRefresh = true; }

        if (needsRefresh) Refresh();
    }

    void ForceHandCount(int target)
    {
        target = Mathf.Max(0, target);
        while (flow.hand.Count > target && flow.hand.Count > 0) flow.Discard(flow.hand[flow.hand.Count - 1]);
        while (flow.hand.Count < target)
        {
            if (flow.deck.Count > 0) flow.Draw(1);
            else flow.hand.Add(new CardInstance(CardDatabase.Get("BASIC-001")));
        }
    }

    void ForceDrawCount(int target)
    {
        target = Mathf.Max(0, target);
        while (flow.deck.Count > target && flow.deck.Count > 0) flow.deck.RemoveAt(flow.deck.Count - 1);
        while (flow.deck.Count < target)
        {
            if (flow.discard.Count > 0) { flow.deck.Add(flow.discard[0]); flow.discard.RemoveAt(0); }
            else flow.deck.Add(new CardInstance(CardDatabase.Get("BASIC-001")));
        }
    }

    void ForceDiscardCount(int target)
    {
        target = Mathf.Max(0, target);
        while (flow.discard.Count > target && flow.discard.Count > 0) flow.discard.RemoveAt(flow.discard.Count - 1);
        while (flow.discard.Count < target)
        {
            if (flow.deck.Count > 0) { flow.discard.Add(flow.deck[0]); flow.deck.RemoveAt(0); }
            else flow.discard.Add(new CardInstance(CardDatabase.Get("BASIC-001")));
        }
    }

    public bool StartBattle(string enemyId)
    {
        if (state != State.Inactive) return false;
        enemyData = EnemyDatabase.Get(enemyId);

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

    void OnCardClicked(CardInstance card)
    {
        if (state != State.PlayerTurn || !flow.hand.Contains(card) || card.data.effect == CardEffectType.Junk || player.stamina < card.data.cost) return;

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
            case CardEffectType.Damage: case CardEffectType.MagicDamage: enemy.TakeDamage(v); break;
            case CardEffectType.DirectHpDamage: enemy.TakeDirectDamage(v); if (s > 0) player.Heal(s); break;
            case CardEffectType.Armor: player.armor += v; break;
            case CardEffectType.RestoreMana: player.RestoreStamina(v); break;
            case CardEffectType.RestoreHp: player.Heal(v); break;
            case CardEffectType.ReduceNextAttack: case CardEffectType.DamageAndReduce: enemy.TakeDamage(v); enemy.attackReduction += s; break;
            case CardEffectType.MultiHit: for (int i = 0; i < t && enemy.hp > 0; i++) enemy.TakeDamage(v); break;
            case CardEffectType.NullifyNextCard: enemy.parryBlock = true; if (v > 0) flow.Draw(v); break;
            case CardEffectType.CounterNextAttack: enemy.counterIncoming = true; if (v > 0) flow.Draw(v); break;
            case CardEffectType.AddJunkCards: enemy.junkToAdd += v; break;
            case CardEffectType.ArmorAndDraw: player.armor += v; if (s > 0) flow.Draw(s); break;
        }
    }

    void OnEndTurnClicked()
    {
        if (state != State.PlayerTurn) return;
        flow.PurgeJunkFromHand();
        flow.EnforceHandLimit(3); // <-- BAGONG LOGIC PARA SA 3 CARD LIMIT
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

    void EndBattle(bool victory)
    {
        if (state == State.Ended || state == State.Inactive) return;
        state = State.Ended; // <-- ITO ANG PUMIPIGIL SA CONFLICT
        lastVictory = victory;

        if (victory)
        {
            PlayerProfile.AddReward(enemyData.xpReward, enemyData.coinReward);
            string rewardCardId = GetRandomClassCard();

            if (victoryUI != null)
            {
                battlePanel.SetActive(false);
                victoryUI.ShowVictory(enemyData.xpReward, enemyData.coinReward, rewardCardId);
            }
            else
            {
                Debug.LogError("Victory UI is NULL! Check Inspector.");
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
    }

    string GetRandomClassCard()
    {
        if (enemyData.classDeck == null || enemyData.classDeck.Count == 0) return "BASIC-001";
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
            CardClass cClass = c.data.cardClass;
            CardUI prefabToUse = CardPrefabManager.Instance.GetPrefab(c.data.cardClass, c.data.id);
            CardUI ui = Instantiate(prefabToUse, handContainer);
ui.Setup(c, OnCardClicked);
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

        // --- SYNC TO INSPECTOR (Ito ang nagpapakita ng stats sa GameStatsManager) ---
        if (GameStatsManager.Instance != null && player != null && enemy != null)
        {
            var gm = GameStatsManager.Instance;
            
            gm.handCards = flow.hand.Count;
            gm.drawCards = flow.deck.Count;
            gm.discardCards = flow.discard.Count;

            gm.playerStats.hp = player.hp;
            gm.playerStats.maxHp = player.maxHp;
            gm.playerStats.stamina = player.stamina;
            gm.playerStats.maxStamina = player.maxStamina;
            gm.playerStats.armor = player.armor;
            gm.playerStats.level = PlayerProfile.level;
            gm.playerStats.xp = PlayerProfile.xp;
            gm.playerStats.xpToNext = PlayerProfile.XpToNext;
            gm.playerStats.coins = PlayerProfile.coins;

            if (gm.activeEnemy == null || gm.activeEnemy.displayName != enemyData.enemyName)
            {
                gm.activeEnemy = new StatBlock(enemyData.id, enemyData.enemyName);
                gm.activeEnemy.difficulty = enemyData.difficulty;
            }
            gm.activeEnemy.hp = enemy.hp;
            gm.activeEnemy.maxHp = enemy.maxHp;
            gm.activeEnemy.stamina = enemy.stamina;
            gm.activeEnemy.maxStamina = enemy.maxStamina;
            gm.activeEnemy.armor = enemy.armor;

            lastSyncedHand = gm.handCards;
            lastSyncedDraw = gm.drawCards;
            lastSyncedDiscard = gm.discardCards;

            lastPlayerHp = gm.playerStats.hp;
            lastPlayerMaxHp = gm.playerStats.maxHp;
            lastPlayerMana = gm.playerStats.stamina;
            lastPlayerMaxMana = gm.playerStats.maxStamina;
            lastPlayerArmor = gm.playerStats.armor;

            if (gm.activeEnemy != null)
            {
                lastEnemyHp = gm.activeEnemy.hp;
                lastEnemyMaxHp = gm.activeEnemy.maxHp;
                lastEnemyMana = gm.activeEnemy.stamina;
                lastEnemyMaxMana = gm.activeEnemy.maxStamina;
                lastEnemyArmor = gm.activeEnemy.armor;
            }
        }
    }
}