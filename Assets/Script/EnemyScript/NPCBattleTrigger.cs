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

        if (BattleTransfer.defeated.Contains(Key()))
        {
            gameObject.SetActive(false);
            return;
        }

        FindPlayer();
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Origin(), triggerDistance);
    }
}