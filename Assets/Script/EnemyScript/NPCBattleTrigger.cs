using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    public string enemyId = "ScarecrowEnemy001";
    public string battleSceneName = "BattleScene";
<<<<<<< HEAD
    public float triggerDistance = 2f;    
    public float returnDistance = 5f;     
    public Transform center;              

    Transform player;
    static bool battleLoading;            
=======
    public float triggerDistance = 2f;
    public float returnDistance = 5f;
    public Transform center;

    Transform player;
    static bool battleLoading;

    public static void ResetBattleLoading()
    {
        battleLoading = false;
    }
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857

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

<<<<<<< HEAD
        // Disable this trigger if we are already in the battle scene
        if (SceneManager.GetActiveScene().name == battleSceneName)
        {
            enabled = false;
            return;
        }

        if (BattleTransfer.defeated.Contains(Key()))
        {
=======
        if (BattleTransfer.defeated.Contains(Key()))
        {
            Debug.Log($"[Trigger] {name} was already defeated. Disabling.");
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857
            gameObject.SetActive(false);
            return;
        }

        FindPlayer();
<<<<<<< HEAD
=======
        Debug.Log($"[Trigger] {name} started. Player found: {(player != null ? player.name : "NULL")}");
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857
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

<<<<<<< HEAD
=======
        // I-double check kung defeated na talaga (para sa mga nag-reload)
        if (BattleTransfer.defeated.Contains(Key()))
        {
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"[Trigger] FIRING from {name}!");
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857
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