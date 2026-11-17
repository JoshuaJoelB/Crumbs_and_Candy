using System.Collections.Generic;
using UnityEngine;

public static class BattleTransfer
{
    public static string enemyId = "ScarecrowEnemy001";       // aling enemy ang lalabanan
    public static string enemyKey = "";                       // unique na key ng enemy sa scene
    public static string explorationScene = "ExplorationScene";
    public static Vector3 playerReturnPosition;
    public static bool hasReturnPosition;
    public static HashSet<string> defeated = new HashSet<string>();   // mga natalong enemy
}