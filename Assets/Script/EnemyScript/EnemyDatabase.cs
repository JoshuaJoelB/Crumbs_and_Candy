using System.Collections.Generic;

public static class EnemyDatabase
{
    static readonly Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();

    static EnemyDatabase()
    {
        // Scarecrow — walang mana card, walang auto regen na Mana card
        Add(new EnemyData(
            "ScarecrowEnemy001", "Scarecrow", "Easy",
            maxHp: 35, maxMana: 5, startingMana: 0,
            xpReward: 30, coinReward: 10,
            uniqueCardId: "SC-001",
            basicAttackCardId: "EN-SC-BASIC",    basicAttackWeight: 60,
            regularAttackCardId: "EN-SC-REGULAR", regularAttackWeight: 40,
            manaCardId: "",                       manaCardWeight: 0));

        // Will-O'-Wisp
        Add(new EnemyData(
            "WillOWispEnemy002", "Will o' Wisp", "Medium",
            maxHp: 40, maxMana: 10, startingMana: 8,
            xpReward: 60, coinReward: 25,
            uniqueCardId: "WIS-001",
            basicAttackCardId: "EN-WIS-BASIC",    basicAttackWeight: 40,
            regularAttackCardId: "EN-WIS-REGULAR", regularAttackWeight: 35,
            manaCardId: "EN-WIS-MANA",             manaCardWeight: 25));

        // Porcelain Doll
        Add(new EnemyData(
            "PorcelainDollEnemy004", "Porcelain Doll", "Tough",
            maxHp: 95, maxMana: 12, startingMana: 10,
            xpReward: 90, coinReward: 40,
            uniqueCardId: "PD-001",
            basicAttackCardId: "EN-PD-BASIC",     basicAttackWeight: 45,
            regularAttackCardId: "EN-PD-REGULAR",  regularAttackWeight: 35,
            manaCardId: "EN-PD-MANA",              manaCardWeight: 20));

        // Werewolf
        Add(new EnemyData(
            "WerewolfEnemy003", "Werewolf", "Hard",
            maxHp: 190, maxMana: 12, startingMana: 10,
            xpReward: 120, coinReward: 50,
            uniqueCardId: "WW-001",
            basicAttackCardId: "EN-WW-BASIC",     basicAttackWeight: 50,
            regularAttackCardId: "EN-WW-REGULAR",  regularAttackWeight: 50,
            manaCardId: "",                        manaCardWeight: 0));

        // Witch
        Add(new EnemyData(
            "WitchEnemy005", "Witch", "Boss",
            maxHp: 140, maxMana: 16, startingMana: 14,
            xpReward: 180, coinReward: 80,
            uniqueCardId: "WIT-001",
            basicAttackCardId: "EN-WIT-BASIC",    basicAttackWeight: 35,
            regularAttackCardId: "EN-WIT-REGULAR", regularAttackWeight: 40,
            manaCardId: "EN-WIT-MANA",             manaCardWeight: 25));
    }

    static void Add(EnemyData e) { enemies.Add(e.id, e); }
    public static EnemyData Get(string id) { return enemies[id]; }
}