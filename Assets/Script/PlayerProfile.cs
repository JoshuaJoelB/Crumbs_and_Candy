using UnityEngine;

public static class PlayerProfile
{
    public static int level = 1;
    public static int xp = 0;
    public static int coins = 0;

    public static int XpToNext { get { return 50 * level; } }
    public static int MaxHp { get { return 100 + (level - 1) * 10; } }

    static PlayerProfile() { Load(); }

    // Returns true if the player leveled up
    public static bool AddReward(int xpGain, int coinGain)
    {
        coins += coinGain;
        xp += xpGain;
        bool leveledUp = false;
        while (xp >= XpToNext) { xp -= XpToNext; level++; leveledUp = true; }
        Save();
        return leveledUp;
    }

    public static void Save()
    {
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("xp", xp);
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        level = PlayerPrefs.GetInt("level", 1);
        xp = PlayerPrefs.GetInt("xp", 0);
        coins = PlayerPrefs.GetInt("coins", 0);
    }

    public static void ResetAll() { level = 1; xp = 0; coins = 0; Save(); }
}