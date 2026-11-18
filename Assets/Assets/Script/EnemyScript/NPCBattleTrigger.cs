using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    public string enemyId = "ScarecrowEnemy001";
    public string battleSceneName = "BattleScene";
    public float triggerDistance = 2f;
    public float returnDistance = 5f;
    public Transform center;

    Transform player;
    static bool battleLoading;

    public static void ResetBattleLoading()
    {
        battleLoading = false;
    }

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
            Debug.Log($"[Trigger] {name} was already defeated. Disabling.");
            gameObject.SetActive(false);
            return;
        }

        FindPlayer();
        Debug.Log($"[Trigger] {name} started. Player found: {(player != null ? player.name : "NULL")}");
    }

    void FindPlayer()
    {
        // Hanapin lang ang player kung may tag na "Player" at active
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null && p.activeInHierarchy)
        {
            player = p.transform;
            return;
        }

        // Fallback: hanapin sa pamamagitan ng pangalan kung sakaling magulo ang tag
        p = GameObject.Find("player_entityy");
        if (p != null && p.activeInHierarchy)
        {
            player = p.transform;
            Debug.LogWarning($"[Trigger] Player found by name fallback: {p.name}. Ayusin ang tag.");
        }
    }

    void Update()
    {
        if (battleLoading) return;
        if (player == null) { FindPlayer(); if (player == null) return; }

        Vector3 diff = player.position - Origin();
        diff.y = 0;
        if (diff.magnitude > triggerDistance) return;

        // I-double check kung defeated na talaga (para sa mga nag-reload)
        if (BattleTransfer.defeated.Contains(Key()))
        {
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"[Trigger] FIRING from {name}!");
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