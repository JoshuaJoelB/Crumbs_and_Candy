using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string enemyId = "ScarecrowEnemy001";
    public float triggerDistance = 2f;
    public float returnDistance = 5f;

    [Header("Player Connection")]
    [Tooltip("Drag your Player (Hansel_Final-1) here!")]
    public Transform player;

    static bool battleLoading;

    public static void ResetBattleLoading()
    {
        battleLoading = false;
    }

    Vector3 Origin() { return transform.position; }

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

        // If you forgot to drag the player in the Inspector, try to find it automatically
        if (player == null)
        {
            CharacterController cc = FindAnyObjectByType<CharacterController>();
            if (cc != null)
            {
                player = cc.transform;
            }
        }
    }

    void Update()
    {
        if (battleLoading) return;
        
        // If we still don't have a player, we can't do anything
        if (player == null) return;

        Vector3 diff = player.position - Origin();
        diff.y = 0; // Ignore height difference

        if (diff.magnitude > triggerDistance) return;

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

        EnemyData enemyData = EnemyDatabase.Get(enemyId);

        BattleTransfer.enemyId = enemyId;
        BattleTransfer.enemyKey = Key();
        BattleTransfer.explorationScene = SceneManager.GetActiveScene().name;
        BattleTransfer.playerReturnPosition = back;
        BattleTransfer.hasReturnPosition = true;

        // Loads the correct battle scene from your EnemyDatabase
        SceneManager.LoadScene(enemyData.battleSceneName);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Origin(), triggerDistance);
    }
}