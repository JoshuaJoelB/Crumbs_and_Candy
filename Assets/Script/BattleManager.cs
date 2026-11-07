using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    [Header("Exploration")]
    public MonoBehaviour[] disableDuringBattle;   // drag your player movement script(s) here

    [Header("Balance")]
    public int enemyAttackDamage = 8;

    Combatant player, enemy;
    CardFlow flow = new CardFlow();
    List<string> acquiredCards;

    public bool IsBusy { get { return state != State.Inactive; } }

    void Awake()
    {
        Instance = this;
        battlePanel.SetActive(false);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    // ---------- START ----------
    public bool StartBattle()
    {
        if (state != State.Inactive) return false;     // no duplicate battles

        acquiredCards = new List<string>();            // test list: 2 copies of each card
        foreach (string id in CardDatabase.AllIds()) { acquiredCards.Add(id); acquiredCards.Add(id); }

        player = new Combatant("Player", 100, 5, 3);
        enemy  = new Combatant("Enemy", 60, 5, 3);

        flow.StartBattle(acquiredCards, 3);            // shuffle + draw 3, ONCE
        foreach (var m in disableDuringBattle) m.enabled = false;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;

        resultText.text = "";
        battlePanel.SetActive(true);
        state = State.PlayerTurn;                      // player goes first (no extra draw)
        Refresh();
        return true;
    }

    // ---------- PLAY CARD ----------
    void OnCardClicked(CardInstance card)
    {
        if (state != State.PlayerTurn) return;
        if (!flow.hand.Contains(card)) return;
        if (player.stamina < card.data.cost) { Debug.Log("Not enough Stamina"); return; }

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
            case CardEffectType.Shield:         player.shield += c.value; break;
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
        int dmg = enemy.ResolveOutgoingAttack(enemyAttackDamage);   // Frost Bind
        player.ReceiveAttack(dmg);                                  // Guard / Arcane Shield
        Refresh();
        yield return new WaitForSeconds(0.5f);

        if (player.hp <= 0) { EndBattle(false); yield break; }

        player.RestoreStamina(1);        // +1 Stamina each turn (change if you like)
        flow.Draw(2);                    // draw 2 at start of each later turn
        state = State.PlayerTurn;
        Refresh();
    }

    // ---------- END ----------
    void EndBattle(bool victory)
    {
        if (state == State.Ended || state == State.Inactive) return;   // result fires once
        state = State.Ended;
        resultText.text = victory ? "VICTORY!" : "DEFEAT";
        Refresh();
        StartCoroutine(ReturnToExploration());
    }

    IEnumerator ReturnToExploration()
    {
        yield return new WaitForSeconds(2.5f);
        battlePanel.SetActive(false);
        foreach (var m in disableDuringBattle) m.enabled = true;
        state = State.Inactive;
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
        playerStatsText.text = string.Format("PLAYER\nHP {0}/{1}\nStamina {2}/{3}\nArmor {4}  Shield {5}",
            player.hp, player.maxHp, player.stamina, player.maxStamina, player.armor, player.shield);
        enemyStatsText.text = string.Format("ENEMY\nHP {0}/{1}\nNext attack -{2}",
            enemy.hp, enemy.maxHp, enemy.attackReduction);
        deckCountText.text = "Deck: " + flow.deck.Count;
        discardCountText.text = "Discard: " + flow.discard.Count;
        endTurnButton.interactable = (state == State.PlayerTurn);
    }
}