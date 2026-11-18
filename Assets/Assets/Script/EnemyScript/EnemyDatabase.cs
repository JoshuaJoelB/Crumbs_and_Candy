using System.Collections.Generic;

public static class EnemyDatabase
{
    static readonly Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();

    static EnemyDatabase()
    {
        // 1. SCARECROW
        Add(new EnemyData("ScarecrowEnemy001", "Scarecrow", "Easy", 35, 10, 0, 10, 5, "SC-001", "NOR-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 1, 1), new EnemyCardEntry("BASIC-003", 1, 1)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("NOR-001", 1, 1), new EnemyCardEntry("NOR-002", 1, 1)
            }
        ));

        // 2. WILL-O'-WISP
        Add(new EnemyData("WillOWispEnemy002", "Will o' Wisp", "Medium", 40, 20, 8, 18, 8, "WIS-001", "MAG-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 1, 2), new EnemyCardEntry("BASIC-003", 1, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("MAG-001", 1, 1), new EnemyCardEntry("MAG-002", 1, 1)
            }
        ));

        // 3. PORCELAIN DOLL
        Add(new EnemyData("PorcelainDollEnemy004", "Porcelain Doll", "Tough", 95, 20, 10, 35, 18, "PD-001", "TRI-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 1, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 2, 4), new EnemyCardEntry("BASIC-003", 1, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("TRI-001", 1, 1), new EnemyCardEntry("TRI-002", 1, 1)
            }
        ));

        // 4. WEREWOLF
        Add(new EnemyData("WerewolfEnemy003", "Werewolf", "Hard", 190, 20, 10, 80, 40, "WW-001", "BER-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 2, 4), new EnemyCardEntry("ENEMY-ATTACK-001", 3, 6), new EnemyCardEntry("BASIC-003", 2, 2)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BER-001", 2, 1), new EnemyCardEntry("BER-002", 1, 1)
            }
        ));

        // 5. WITCH
        Add(new EnemyData("WitchEnemy005", "Witch", "Boss", 120, 25, 14, 50, 25, "WIT-001", "MAG-001",
            new List<EnemyCardEntry> {
                new EnemyCardEntry("BASIC-001", 2, 2), new EnemyCardEntry("ENEMY-ATTACK-001", 3, 4), new EnemyCardEntry("BASIC-003", 2, 4)
            },
            new List<EnemyCardEntry> {
                new EnemyCardEntry("MAG-001", 2, 1), new EnemyCardEntry("MAG-002", 2, 1)
            }
        ));
    }

    static void Add(EnemyData e) => enemies.Add(e.id, e);
    public static EnemyData Get(string id) => enemies[id];
}