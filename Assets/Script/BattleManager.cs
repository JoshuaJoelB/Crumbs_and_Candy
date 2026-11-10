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

    // Pagbukas ng BattleScene, kusang magsisimula ang laban (walang trigger button).
    void Start()
    {
        StartBattle(BattleTransfer.enemyId);
    }

    // ---------- START ----------
    public bool StartBattle(string enemyId)
    {
        if (state != State.Inactive) return false;          // walang duplicate na laban

        enemyData = EnemyDatabase.Get(enemyId);

        // Test cards: 2 copies ng bawat card
        acquiredCards = new List<string>();
        foreach (string id in CardDatabase.AllIds()) { acquiredCards.Add(id); acquiredCards.Add(id); }

        player = new Combatant("Player", PlayerProfile.MaxHp, 5, 3);
        enemy = new Combatant(enemyData.enemyName, enemyData.maxHp, enemyData.maxMana, 0);

        flow.StartBattle(acquiredCards, 3);                 // shuffle at draw 3, isang beses lang

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
        if (player.stamina < card.data.cost) { Debug.Log("Not enough Mana"); return; }

        player.stamina -= card.data.cost;
        ApplyEffect(card.data);
        flow.Discard(card);

        if (enemy.hp <= 0) { EndBattle(true); return; }
        Refresh();
    }

    void ApplyEffect(CardData c)
    {
        switch (c.effect)
        {
            case CardEffectType.Damage:         enemy.TakeDamage(c.value); break;
            case CardEffectType.RestoreStamina: player.RestoreStamina(c.value); break;
            case CardEffectType.Armor:          player.armor += c.value; break;
            case CardEffectType.Frost:          enemy.attackReduction += c.value; break;
        }
    }

    // ---------- TURNS ----------
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

        enemy.RestoreStamina(1);                            // +1 mana bawat turn
        int attack = enemyData.attackDamage;
        if (enemy.stamina >= enemyData.specialCost)         // sapat ang mana: special attack
        {
            enemy.stamina -= enemyData.specialCost;
            attack += enemyData.specialBonus;
        }

        int dmg = enemy.ResolveOutgoingAttack(attack);      // Frost Bind
        player.TakeDamage(dmg);                             // dadaan sa armor ng player, tapos HP
        player.armor = 0;                                   // armor ay para sa susunod na attack lang
        Refresh();
        yield return new WaitForSeconds(0.5f);

        if (player.hp <= 0) { EndBattle(false); yield break; }

        player.RestoreStamina(1);                           // +1 mana bawat turn
        flow.Draw(2);                                       // draw 2 bawat susunod na turn
        state = State.PlayerTurn;
        Refresh();
    }

    // ---------- END ----------
    void EndBattle(bool victory)
    {
        if (state == State.Ended || state == State.Inactive) return;   // isang beses lang ang result
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
            BattleTransfer.defeated.Add(BattleTransfer.enemyKey);      // wala na ang enemy na ito

        state = State.Inactive;
        SceneManager.LoadScene(BattleTransfer.explorationScene);
    }

    // ---------- UI ----------
    void Refresh()
    {
        foreach (Transform child in handContainer) Destroy(child.gameObject);
        foreach (CardInstance c in flow.hand)
        {
            CardUI ui = Instantiate(cardPrefab, handContainer);
            ui.Setup(c, OnCardClicked);
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