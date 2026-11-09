using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    public string enemyId = "ScarecrowEnemy001";
    public string battleSceneName = "BattleScene";
    public float triggerDistance = 2f;    // gaano kalapit bago magsimula ang laban (mga 3 feet)
    public float returnDistance = 5f;     // gaano kalayo ibabalik ang player pagkatapos ng laban

    Transform player;

    // Static, kaya iisa ito para sa lahat ng enemy.
    // Pinipigilan nito ang dalawang enemy na magtrigger nang sabay.
    static bool battleLoading;

    string Key()
    {
        Vector3 p = transform.position;
        return SceneManager.GetActiveScene().name + "/" + enemyId + "/" +
               Mathf.RoundToInt(p.x) + "_" + Mathf.RoundToInt(p.y) + "_" + Mathf.RoundToInt(p.z);
    }

    void Start()
    {
        battleLoading = false;     // i-reset sa bawat pagbukas ng scene

        if (BattleTransfer.defeated.Contains(Key()))     // natalo na, kaya nawawala
        {
            gameObject.SetActive(false);
            return;
        }

        FindPlayer();
        Debug.Log(name + " ready | id=" + enemyId + " | player found=" + (player != null));
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

        Vector3 diff = player.position - transform.position;
        diff.y = 0;                                       // tingnan lang ang pahalang na distansya
        if (diff.magnitude > triggerDistance) return;     // malayo pa

        battleLoading = true;                             // isang laban lang sa isang pagkakataon

        // Saan ibabalik ang player pagkatapos ng laban
        Vector3 dir = diff.sqrMagnitude > 0.001f ? diff.normalized : -transform.forward;
        Vector3 back = transform.position + dir * returnDistance;
        back.y = player.position.y;

        BattleTransfer.enemyId = enemyId;
        BattleTransfer.enemyKey = Key();
        BattleTransfer.explorationScene = SceneManager.GetActiveScene().name;
        BattleTransfer.playerReturnPosition = back;
        BattleTransfer.hasReturnPosition = true;

        SceneManager.LoadScene(battleSceneName);
    }

    // Pulang bilog sa Scene view para makita ang trigger distance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}