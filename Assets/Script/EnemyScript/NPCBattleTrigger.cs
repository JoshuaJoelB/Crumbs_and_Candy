using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    public string enemyId = "ScarecrowEnemy001";
    public string battleSceneName = "BattleScene";
    public float triggerDistance = 2f;    // gaano kalapit bago magsimula ang laban (mga 3 feet)
    public float returnDistance = 5f;     // gaano kalayo ibabalik ang player pagkatapos ng laban
    public Transform center;              // i-drag dito ang Visual (capsule) para nasa katawan ang bilog

    Transform player;
    static bool battleLoading;            // iisa para sa lahat ng enemy

    Vector3 Origin() { return center != null ? center.position : transform.position; }

    string Key()
    {
        Vector3 p = transform.position;
        return SceneManager.GetActiveScene().name + "/" + enemyId + "/" +
               Mathf.RoundToInt(p.x) + "_" + Mathf.RoundToInt(p.y) + "_" + Mathf.RoundToInt(p.z);
    }

    void Start()
    {
        battleLoading = false;

        // DEBUG
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[Trigger] '{name}' started in scene '{sceneName}' " +
                  $"key='{Key()}' " +
                  $"alreadyDefeated={BattleTransfer.defeated.Contains(Key())} " +
                  $"enemyId='{enemyId}' " +
                  $"battleSceneName='{battleSceneName}' " +
                  $"triggerDistance={triggerDistance} " +
                  $"center={(center != null ? center.name : "NULL (using transform.position)")}");

        if (BattleTransfer.defeated.Contains(Key()))
        {
            Debug.Log($"[Trigger] '{name}' DISABLED — this enemy was already defeated (key exists in BattleTransfer.defeated).");
            gameObject.SetActive(false);
            return;
        }

        FindPlayer();

        if (player == null)
            Debug.LogWarning($"[Trigger] '{name}' — NO PLAYER FOUND in scene '{sceneName}' " +
                             $"(tried tag 'Player'). Check that your player object is tagged 'Player' and is active.");
        else
            Debug.Log($"[Trigger] '{name}' — player found: '{player.name}' at {player.position}, " +
                      $"distance to origin = {Vector3.Distance(Flatten(player.position), Flatten(Origin())):F2}");
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (battleLoading) return;
        if (player == null) { FindPlayer(); if (player == null) return; }

        Vector3 diff = player.position - Origin();
        diff.y = 0;
        if (diff.magnitude > triggerDistance) return;

        // DEBUG 
        Debug.Log($"[Trigger] '{name}' FIRING! distance={diff.magnitude:F2} " +
                  $"enemyId='{enemyId}' battleScene='{battleSceneName}' " +
                  $"returning to scene='{SceneManager.GetActiveScene().name}'");

        battleLoading = true;

        Vector3 dir = diff.sqrMagnitude > 0.001f ? diff.normalized : -transform.forward;
        Vector3 back = Origin() + dir * returnDistance;
        back.y = player.position.y;

        BattleTransfer.enemyId = enemyId;
        BattleTransfer.enemyKey = Key();
        BattleTransfer.explorationScene = SceneManager.GetActiveScene().name;
        BattleTransfer.playerReturnPosition = back;
        BattleTransfer.hasReturnPosition = true;

        SceneManager.LoadScene(battleSceneName);
    }

    // Helper lang para sa debug log
    Vector3 Flatten(Vector3 v) { v.y = 0; return v; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Origin(), triggerDistance);
    }
}