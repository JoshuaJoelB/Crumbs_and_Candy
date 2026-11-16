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
    public TMP_Text playerStatsText, enemyStatsText, deckCountText, discardCountText, resultText;
    public Button endTurnButton;

    [Header("Bars")]
    public StatBar playerHpBar;
    public StatBar playerManaBar;
    public StatBar enemyHpBar;
    public StatBar enemyManaBar;

    Combatant player, enemy;
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

        acquiredCards = new List<string>();
        foreach (string id in CardDatabase.AllIds())
        {
            if (id == "JUNK-001") continue;
            if (id.StartsWith("EN-")) continue;
            if (id.StartsWith("SC-")) continue;
            if (id.StartsWith("WIS-")) continue;
            if (id.StartsWith("PD-")) continue;
            if (id.StartsWith("WW-")) continue;
            if (id.StartsWith("WIT-")) continue;

            acquiredCards.Add(id);
            acquiredCards.Add(id);
        }

        player = new Combatant("Player", PlayerProfile.MaxHp, 5, 3);
        enemy = new Combatant(enemyData.enemyName, enemyData.maxHp, enemyData.maxMana, enemyData.startingMana);

        flow.StartBattle(acquiredCards, 3);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        resultText.text = "";
        battlePanel.SetActive(true);
        state = State.PlayerTurn;
        Refresh();
        return true;
    }

    // ---------- PLAY CARD ----------
    void OnCardClicked(CardInstance card)
    {
        if (state != State.PlayerTurn) return;
        if (!flow.hand.Contains(card)) return;

        if (card.data.effect == CardEffectType.Junk)
        {
            Debug.Log("Junk card — unplayable.");
            return;
        }

        if (player.stamina < card.data.cost)
        {
            Debug.Log("Not enough Mana");
            return;
        }

        player.stamina -= card.data.cost;
        ApplyEffect(card);
        flow.Discard(card);

        if (enemy.hp <= 0) { EndBattle(true); return; }
        Refresh();
    }

    void ApplyEffect(CardInstance card)
    {
        CardData c = card.data;
        int v = card.Value;
        int s = card.Secondary;
        int t = card.Tertiary;

        switch (c.effect)
        {
            case CardEffectType.Damage:
                enemy.TakeDamage(v);
                break;

            case CardEffectType.MagicDamage:
                enemy.TakeDamage(v);
                break;

            case CardEffectType.DirectHpDamage:
                enemy.TakeDirectDamage(v);
                if (s > 0) player.Heal(s);
                break;

            case CardEffectType.Armor:
                player.armor += v;
                break;

            case CardEffectType.RestoreMana:
                player.RestoreStamina(v);
                break;

            case CardEffectType.RestoreHp:
                player.Heal(v);
                break;

            case CardEffectType.ReduceNextAttack:
                enemy.TakeDamage(v);
                enemy.attackReduction += s;
                break;

            case CardEffectType.MultiHit:
                for (int i = 0; i < t && enemy.hp > 0; i++)
                    enemy.TakeDamage(v);
                break;

            case CardEffectType.NullifyNextCard:
                enemy.parryBlock = true;
                if (v > 0) flow.Draw(v);
                break;

            case CardEffectType.CounterNextAttack:
                enemy.counterIncoming = true;
                if (v > 0) flow.Draw(v);
                break;

            case CardEffectType.AddJunkCards:
                enemy.junkToAdd += v;
                break;

            case CardEffectType.DamageAndReduce:
                enemy.TakeDamage(v);
                enemy.attackReduction += s;
                break;

            case CardEffectType.ArmorAndDraw:
                player.armor += v;
                if (s > 0) flow.Draw(s);
                break;
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

        if (enemy.junkToAdd > 0)
        {
            flow.AddJunk(enemy.junkToAdd);
            enemy.junkToAdd = 0;
        }

        enemy.RestoreStamina(1);

        if (enemy.parryBlock)
        {
            enemy.parryBlock = false;
            Debug.Log("[Enemy] Parried — turn skipped.");
            Refresh();
        }
        else
        {
            CardInstance uniqueCard = TryGetUniqueCard();
            CardInstance chosenCard;

            if (uniqueCard != null && enemy.stamina >= uniqueCard.data.cost)
            {
                chosenCard = uniqueCard;
                Debug.Log($"[Enemy] Casting UNIQUE: {chosenCard.data.cardName}");
            }
            else
            {
                chosenCard = PickWeightedEnemyCard();
                if (chosenCard != null)
                    Debug.Log($"[Enemy] Playing: {chosenCard.data.cardName}");
            }

            if (chosenCard != null)
            {
                enemy.stamina -= chosenCard.data.cost;
                ExecuteEnemyCard(chosenCard);
                Refresh();
                yield return new WaitForSeconds(0.5f);
            }

            if (enemy.hp <= 0) { EndBattle(true); yield break; }
            if (player.hp <= 0) { EndBattle(false); yield break; }
        }

        player.RestoreStamina(1);
        flow.Draw(2);
        state = State.PlayerTurn;
        Refresh();
    }

    // ---------- ENEMY CARD SELECTION ----------
    CardInstance TryGetUniqueCard()
    {
        if (string.IsNullOrEmpty(enemyData.uniqueCardId)) return null;
        return new CardInstance(CardDatabase.Get(enemyData.uniqueCardId));
    }

    CardInstance PickWeightedEnemyCard()
    {
        var candidates = new List<(string id, int weight)>();
        if (!string.IsNullOrEmpty(enemyData.basicAttackCardId) && enemyData.basicAttackWeight > 0)
            candidates.Add((enemyData.basicAttackCardId, enemyData.basicAttackWeight));
        if (!string.IsNullOrEmpty(enemyData.regularAttackCardId) && enemyData.regularAttackWeight > 0)
            candidates.Add((enemyData.regularAttackCardId, enemyData.regularAttackWeight));
        if (!string.IsNullOrEmpty(enemyData.manaCardId) && enemyData.manaCardWeight > 0)
            candidates.Add((enemyData.manaCardId, enemyData.manaCardWeight));

        if (candidates.Count == 0) return null;

        var affordable = new List<(string id, int weight)>();
        foreach (var c in candidates)
        {
            CardData cd = CardDatabase.Get(c.id);
            if (enemy.stamina >= cd.cost) affordable.Add(c);
        }

        if (affordable.Count == 0) return null;

        int totalWeight = 0;
        foreach (var a in affordable) totalWeight += a.weight;

        int roll = Random.Range(0, totalWeight);
        foreach (var a in affordable)
        {
            roll -= a.weight;
            if (roll < 0) return new CardInstance(CardDatabase.Get(a.id));
        }

        return new CardInstance(CardDatabase.Get(affordable[0].id));
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
                    if (enemy.counterIncoming)
                    {
                        enemy.counterIncoming = false;
                        enemy.TakeDirectDamage(dmg);
                    }
                    else
                    {
                        player.TakeDamage(dmg);
                    }
                    player.armor = 0;
                }
                break;

            case CardEffectType.RestoreMana:
                enemy.RestoreStamina(v);
                break;

            case CardEffectType.DirectHpDamage:
                {
                    int dmg = enemy.ResolveOutgoingAttack(v);
                    if (enemy.counterIncoming)
                    {
                        enemy.counterIncoming = false;
                        enemy.TakeDirectDamage(dmg);
                    }
                    else
                    {
                        player.TakeDirectDamage(dmg);
                    }
                    if (s > 0) enemy.Heal(s);
                }
                break;

            case CardEffectType.DamageAndReduce:
                {
                    int dmg = enemy.ResolveOutgoingAttack(v);
                    if (enemy.counterIncoming)
                    {
                        enemy.counterIncoming = false;
                        enemy.TakeDirectDamage(dmg);
                    }
                    else
                    {
                        player.TakeDamage(dmg);
                    }
                    player.attackReduction += s;
                    player.armor = 0;
                }
                break;

            case CardEffectType.ArmorAndDraw:
                enemy.armor += v;
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
            bool leveledUp = PlayerProfile.AddReward(enemyData.xpReward, enemyData.coinReward);
            resultText.text = "VICTORY!\n+" + enemyData.xpReward + " XP   +" + enemyData.coinReward + " Coins";
            if (leveledUp) resultText.text += "\nLEVEL UP! Level " + PlayerProfile.level;
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

        // I-reset ang battleLoading para makapag-trigger ulit sa ibang enemy
        NPCBattleTrigger.ResetBattleLoading();

        state = State.Inactive;
        SceneManager.LoadScene(BattleTransfer.explorationScene);
    }

    // ---------- UI ----------
    void Refresh()
    {
        // I-detach muna at i-destroy ang mga lumang card UI
        if (handContainer != null)
        {
            for (int i = handContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = handContainer.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[BattleManager] cardPrefab is NULL! I-drag ang CardUI prefab sa Inspector.");
            return;
        }

        if (flow.hand != null)
        {
            foreach (CardInstance c in flow.hand)
            {
                CardUI ui = Instantiate(cardPrefab, handContainer);
                ui.Setup(c, OnCardClicked);
            }
        }

        playerStatsText.text = string.Format("PLAYER  Lv {0}\nArmor {1}\nXP {2}/{3}\nCoins {4}",
            PlayerProfile.level, player.armor, PlayerProfile.xp, PlayerProfile.XpToNext, PlayerProfile.coins);

        enemyStatsText.text = string.Format("{0} ({1})\nNext attack -{2}",
            enemyData.enemyName, enemyData.difficulty, enemy.attackReduction);

        if (playerHpBar != null)   playerHpBar.Set(player.hp, player.maxHp);
        if (playerManaBar != null) playerManaBar.Set(player.stamina, player.maxStamina);
        if (enemyHpBar != null)    enemyHpBar.Set(enemy.hp, enemy.maxHp);
        if (enemyManaBar != null)  enemyManaBar.Set(enemy.stamina, enemy.maxStamina);

        deckCountText.text = "Deck: " + flow.deck.Count;
        discardCountText.text = "Discard: " + flow.discard.Count;
        endTurnButton.interactable = (state == State.PlayerTurn);
    }
}