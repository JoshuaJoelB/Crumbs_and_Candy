using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textMesh == null)
            Debug.LogError("[DamagePopup] Walang TextMeshProUGUI component ang prefab!");
    }

    public void Setup(int damageAmount, bool isPlayerTakingDamage)
    {
        if (textMesh == null) return;

        // LAGYAN NG MINUS SIGN AT KULAY PULA
        textMesh.text = "-" + damageAmount.ToString();
        textMesh.color = new Color(1f, 0.2f, 0.2f); // Pulang-pula
        textColor = textMesh.color;

        disappearTimer = 0.8f;
        moveVector = new Vector3(0, 2f, 0); // Pataas ang lipad
    }

    void Update()
    {
        if (textMesh == null) return;

        transform.position += moveVector * Time.deltaTime;
        moveVector -= new Vector3(0, 8f, 0) * Time.deltaTime; // Gravity effect

        if (disappearTimer > 0.5f)
        {
            transform.localScale += Vector3.one * 2f * Time.deltaTime; // Lumalaki muna
        }
        else
        {
            transform.localScale -= Vector3.one * 2f * Time.deltaTime; // Lumiliit
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            textColor.a -= 3f * Time.deltaTime; // Nag-fa-fade
            textMesh.color = textColor;
            if (textColor.a < 0) Destroy(gameObject);
        }
    }
}