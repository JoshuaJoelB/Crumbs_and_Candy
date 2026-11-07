using UnityEngine;

public class BattleTestKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            BattleManager.Instance.StartBattle();
    }
}