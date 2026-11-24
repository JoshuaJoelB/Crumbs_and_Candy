using System.Collections.Generic;
using UnityEngine;

public static class EnemyDatabase
{
    static readonly Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();

    static EnemyDatabase()
    {
        // 1. SCARECROW (CLASS: NORMAL, ROLE: TUTORIAL)
        Add(new EnemyData(
            "ScarecrowEnemy001", "Scarecrow", "Tutorial",
            "ScarecrowBattleScene", // <-- BATTLE SCENE
            35, 10, 0, 10, 5,
            "SC-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 1, 1), new EnemyCardEntry("BASIC-003", 1, 1)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("NOR-001", 1, 1), new EnemyCardEntry("NOR-002", 1, 1)
            }
        ));

        // 2. WILL-O'-WISP (CLASS: MAGE, ROLE: REGULAR ENEMY)
        Add(new EnemyData(
            "WillOWispEnemy002", "Will o' Wisp", "Medium",
            "WillOWispBattleScene", // <-- BATTLE SCENE
            40, 20, 8, 18, 8,
            "WIS-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 1, 2), new EnemyCardEntry("BASIC-003", 1, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("MAG-001", 1, 1), new EnemyCardEntry("MAG-002", 1, 1)
            }
        ));

        // 3. PORCELAIN DOLL (CLASS: TRICKSTER, ROLE: ELITE)
        Add(new EnemyData(
            "PorcelainDollEnemy004", "Porcelain Doll", "Elite",
            "PorcelainDollBattleScene", // <-- BATTLE SCENE
            95, 20, 10, 35, 18,
            "PD-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 2, 4), new EnemyCardEntry("BASIC-003", 1, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("TRI-001", 1, 1), new EnemyCardEntry("TRI-002", 1, 1)
            }
        ));

        // 4. WEREWOLF (CLASS: BERSERKER, ROLE: MAJOR ENCOUNTER)
        Add(new EnemyData(
            "WerewolfEnemy003", "Werewolf", "Hard",
            "WerewolfBattleScene", // <-- BATTLE SCENE
            190, 20, 10, 80, 40,
            "WW-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 2, 4), new EnemyCardEntry("ENEMY-ATTACK-001", 3, 6), new EnemyCardEntry("BASIC-003", 2, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BER-001", 2, 1), new EnemyCardEntry("BER-002", 1, 1)
            }
        ));

        // 5. WITCH (CLASS: MAGE, ROLE: FINAL BOSS)
        Add(new EnemyData(
            "WitchEnemy005", "Witch", "Boss",
            "WitchBattleScene", // <-- BATTLE SCENE
            120, 25, 14, 50, 25,
            "WIT-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 2, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 3, 4), new EnemyCardEntry("BASIC-003", 2, 4)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("MAG-001", 2, 1), new EnemyCardEntry("MAG-002", 2, 1)
            }
        ));
    }

    static void Add(EnemyData e) { enemies.Add(e.id, e); }
    
    public static EnemyData Get(string id) 
    { 
        if (enemies.ContainsKey(id)) return enemies[id];
        Debug.LogWarning("Enemy not found: " + id + ". Defaulting to Scarecrow.");
        return enemies["ScarecrowEnemy001"]; 
    }

    public static bool Exists(string id) => enemies.ContainsKey(id);
    public static List<string> AllIds() => new List<string>(enemies.Keys);
}