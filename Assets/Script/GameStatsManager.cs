using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StatBlock
{
    public string id;
    public string displayName;
    public string difficulty;

    public int level = 1;
    public int maxHp = 100;
    public int hp = 100;
    public int maxStamina = 5;
    public int stamina = 5;
    public int armor = 0;
    public int attackReduction = 0;
    
    public int xp = 0;
    public int xpToNext = 50;
    public int coins = 0;

    public int xpReward = 0;
    public int coinReward = 0;

    public StatBlock() { }

    public StatBlock(string id, string displayName)
    {
        this.id = id;
        this.displayName = displayName;
    }

    public void ResetForBattle(bool fullHeal, int startStamina)
    {
        armor = 0;
        attackReduction = 0;
        stamina = Mathf.Clamp(startStamina, 0, maxStamina);
        if (fullHeal) hp = maxHp;
    }
}

public class GameStatsManager : MonoBehaviour
{
    public const string PlayerId = "Player";
    public static GameStatsManager Instance { get; private set; }

    [Header("----- PLAYER STATS (EDITABLE) -----")]
    public StatBlock playerStats = new StatBlock(PlayerId, "PLAYER");

    [Header("----- CARD COUNTS (EDITABLE) -----")]
    public int handCards = 0;
    public int drawCards = 0;
    public int discardCards = 0;

    [Header("----- UI REFERENCES -----")]
    public StatBar playerHpBar;
    public StatBar playerManaBar;
    public StatBar enemyHpBar;
    public StatBar enemyManaBar;
    public TMP_Text playerStatsText; 
    public TMP_Text enemyStatsText;

    [Header("----- EVENTS -----")]
    public UnityEvent onPlayerDeath;
    public UnityEvent onPlayerLevelUp;

    public StatBlock activeEnemy;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPlayerData();
    }

    void Update()
    {
        if (Application.isPlaying) RefreshUI();
    }

    void OnApplicationQuit()
    {
        SavePlayerData();
    }

    public void SetupUIReferences(StatBar pHpBar, StatBar pManaBar, StatBar eHpBar, StatBar eManaBar, TMP_Text pText, TMP_Text eText)
    {
        playerHpBar = pHpBar;
        playerManaBar = pManaBar;
        enemyHpBar = eHpBar;
        enemyManaBar = eManaBar;
        playerStatsText = pText;
        enemyStatsText = eText;
        RefreshUI();
    }

    [ContextMenu("Save Inspector Data to PlayerPrefs")]
    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("save_exists", 1);
        PlayerPrefs.SetInt("p_level", playerStats.level);
        PlayerPrefs.SetInt("p_maxHp", playerStats.maxHp);
        PlayerPrefs.SetInt("p_hp", playerStats.hp);
        PlayerPrefs.SetInt("p_maxStamina", playerStats.maxStamina);
        PlayerPrefs.SetInt("p_stamina", playerStats.stamina);
        PlayerPrefs.SetInt("p_armor", playerStats.armor);
        PlayerPrefs.SetInt("p_xp", playerStats.xp);
        PlayerPrefs.SetInt("p_xpToNext", playerStats.xpToNext);
        PlayerPrefs.SetInt("p_coins", playerStats.coins);
        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("save_exists"))
        {
            playerStats.level = PlayerPrefs.GetInt("p_level", 1);
            playerStats.maxHp = PlayerPrefs.GetInt("p_maxHp", 100);
            playerStats.hp = PlayerPrefs.GetInt("p_hp", 100);
            playerStats.maxStamina = PlayerPrefs.GetInt("p_maxStamina", 5);
            playerStats.stamina = PlayerPrefs.GetInt("p_stamina", 5);
            playerStats.armor = PlayerPrefs.GetInt("p_armor", 0);
            playerStats.xp = PlayerPrefs.GetInt("p_xp", 0);
            playerStats.xpToNext = PlayerPrefs.GetInt("p_xpToNext", 50);
            playerStats.coins = PlayerPrefs.GetInt("p_coins", 0);
        }
    }

    [ContextMenu("Reset Saved Data (Delete Save)")]
    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteAll();
        RefreshUI();
    }

    public void SetupEnemy(EnemyData data)
    {
        activeEnemy = new StatBlock(data.id, data.enemyName);
        activeEnemy.difficulty = data.difficulty;
        activeEnemy.maxHp = data.maxHp;
        activeEnemy.hp = data.maxHp;
        activeEnemy.maxStamina = data.maxMana;
        activeEnemy.stamina = 0;
        activeEnemy.xpReward = data.xpReward;
        activeEnemy.coinReward = data.coinReward;
        RefreshUI();
    }

    public void ClearEnemy()
    {
        activeEnemy = null;
        RefreshUI();
    }

    public void ApplyDamageToEnemy(int amount)
    {
        if (activeEnemy == null) return;
        int absorbed = Mathf.Min(amount, activeEnemy.armor);
        activeEnemy.armor -= absorbed;
        activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - (amount - absorbed));
        RefreshUI();
    }

    public void ApplyDamageToPlayer(int amount)
    {
        int absorbed = Mathf.Min(amount, playerStats.armor);
        playerStats.armor -= absorbed;
        playerStats.hp = Mathf.Max(0, playerStats.hp - (amount - absorbed));
        RefreshUI();
        if (playerStats.hp <= 0) onPlayerDeath.Invoke();
    }

    public void AddPlayerArmor(int amount)
    {
        playerStats.armor += amount;
        RefreshUI();
    }

    public void RestorePlayerStamina(int amount)
    {
        playerStats.stamina = Mathf.Min(playerStats.maxStamina, playerStats.stamina + amount);
        RefreshUI();
    }

    public void AddEnemyAttackReduction(int amount)
    {
        if (activeEnemy != null) activeEnemy.attackReduction += amount;
        RefreshUI();
    }

    public bool AddReward()
    {
        if (activeEnemy == null) return false;
        playerStats.coins += activeEnemy.coinReward;
        playerStats.xp += activeEnemy.xpReward;
        
        bool leveledUp = false;
        while (playerStats.xp >= playerStats.xpToNext)
        {
            playerStats.xp -= playerStats.xpToNext;
            playerStats.level++;
            playerStats.maxHp += 10;
            playerStats.hp = playerStats.maxHp;
            playerStats.xpToNext = 50 * playerStats.level;
            leveledUp = true;
        }
        SavePlayerData();
        RefreshUI();
        if (leveledUp) onPlayerLevelUp.Invoke();
        return leveledUp;
    }

    public void RefreshUI()
    {
        // --- PLAYER FORMAT ---
        if (playerStatsText != null)
        {
            playerStatsText.text = string.Format(
                "{0} Lv {1}\n" +
                "Armor {2}\n" +
                "XP {3}/{4}\n" +
                "Coins {5}",
                playerStats.displayName, 
                playerStats.level,
                playerStats.armor,
                playerStats.xp, 
                playerStats.xpToNext, 
                playerStats.coins
            );
        }

        // --- ENEMY FORMAT ---
        if (enemyStatsText != null)
        {
            if (activeEnemy != null)
            {
                // Changed this line: Removed the hardcoded "Enemy" word
                enemyStatsText.text = string.Format(
                    "{0}\n" +
                    "Next attack -{1}",
                    activeEnemy.displayName, 
                    activeEnemy.attackReduction
                );
            }
            else enemyStatsText.text = "";
        }

        // --- BARS ---
        if (playerHpBar != null) playerHpBar.Set(playerStats.hp, playerStats.maxHp);
        if (playerManaBar != null) playerManaBar.Set(playerStats.stamina, playerStats.maxStamina);
        
        if (enemyHpBar != null && activeEnemy != null) enemyHpBar.Set(activeEnemy.hp, activeEnemy.maxHp);
        if (enemyManaBar != null && activeEnemy != null) enemyManaBar.Set(activeEnemy.stamina, activeEnemy.maxStamina);
    }
}