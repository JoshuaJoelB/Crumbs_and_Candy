using System.Collections.Generic;

public static class EnemyDatabase
{
    static readonly Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();

    static EnemyDatabase()
    {
        //                     id                      name             difficulty    HP   mana atk spCost spBonus XP   coins
        Add(new EnemyData("ScarecrowEnemy001",    "Scarecrow",      "Easy",        40,  3,   5,  3,     2,     30,  10));
        Add(new EnemyData("WillOWispEnemy002",    "Will o' Wisp",   "Medium",      55,  6,   6,  2,     5,     60,  25));
        Add(new EnemyData("PorcelainDollEnemy004","Porcelain Doll", "Tough",       80,  4,   8,  3,     9,     90,  40));
        Add(new EnemyData("WerewolfEnemy003",     "Werewolf",       "Hard",        100, 6,   11, 3,     7,     120, 50));
        Add(new EnemyData("IronHuntsmanEnemy005", "Iron Huntsman",  "Extra Hard",  140, 6,   10, 4,     10,    180, 80));
    }

    static void Add(EnemyData e) { enemies.Add(e.id, e); }
    public static EnemyData Get(string id) { return enemies[id]; }
}