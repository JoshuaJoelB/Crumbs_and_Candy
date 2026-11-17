using UnityEngine;

public class GameReset : MonoBehaviour
{
    void Awake()
    {
        BattleTransfer.defeated.Clear();
        BattleTransfer.enemyId = "ScarecrowEnemy001";
        BattleTransfer.enemyKey = "";
        BattleTransfer.explorationScene = "ExplorationScene";
        BattleTransfer.hasReturnPosition = false;

        NPCBattleTrigger.ResetBattleLoading();

        Debug.Log("[GameReset] Static data cleared.");
    }
}