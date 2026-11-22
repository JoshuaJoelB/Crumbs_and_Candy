using System.Collections.Generic;
using UnityEngine;

public static class EnemyDatabase
{
    static readonly Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();

    static EnemyDatabase()
    {
        // Scarecrow
        Add(new EnemyData(
            "ScarecrowEnemy001", "Scarecrow", "Easy", 
            35, 0, 0, 30, 10, 
            "SC-001", "NOR-001", 
            new List<EnemyCardEntry> { new EnemyCardEntry("ENEMY-ATTACK-001", 1, 5) },
            new List<EnemyCardEntry> { new EnemyCardEntry("NOR-001", 1, 1) }
        ));

        // Will-o'-Wisp
        Add(new EnemyData(
            "WillOWispEnemy002", "Will o' Wisp", "Medium", 
            40, 6, 2, 60, 25, 
            "WIS-001", "MAG-001", 
            new List<EnemyCardEntry> { new EnemyCardEntry("ENEMY-ATTACK-001", 1, 5) },
            new List<EnemyCardEntry> { new EnemyCardEntry("MAG-001", 1, 1) }
        ));

        // Werewolf
        Add(new EnemyData(
            "WerewolfEnemy003", "Werewolf", "Hard", 
            190, 6, 3, 120, 50, 
            "WW-001", "WAR-002", 
            new List<EnemyCardEntry> { new EnemyCardEntry("ENEMY-ATTACK-001", 2, 4) },
            new List<EnemyCardEntry> { new EnemyCardEntry("WAR-002", 1, 1) }
        ));

        // Porcelain Doll
        Add(new EnemyData(
            "PorcelainDollEnemy004", "Porcelain Doll", "Tough", 
            95, 4, 2, 90, 40, 
            "PD-001", "MAG-002", 
            new List<EnemyCardEntry> { new EnemyCardEntry("ENEMY-ATTACK-001", 1, 5) },
            new List<EnemyCardEntry> { new EnemyCardEntry("MAG-002", 1, 1) }
        ));

        // Witch
        Add(new EnemyData(
            "WitchEnemy005", "Witch", "Boss", 
            140, 6, 4, 180, 80, 
            "WIT-001", "WAR-001", 
            new List<EnemyCardEntry> { new EnemyCardEntry("ENEMY-ATTACK-001", 2, 5) },
            new List<EnemyCardEntry> { new EnemyCardEntry("WAR-001", 1, 1) }
        ));
    }

    static void Add(EnemyData e) { enemies.Add(e.id, e); }
    
    public static EnemyData Get(string id) 
    { 
        if (enemies.ContainsKey(id)) return enemies[id];
        
        // If you accidentally typed the wrong ID in the NPCBattleTrigger, 
        // this will fallback to the Scarecrow so the game doesn't crash.
        Debug.LogWarning("Enemy not found: " + id + ". Defaulting to Scarecrow.");
        return enemies["ScarecrowEnemy001"]; 
    }
}