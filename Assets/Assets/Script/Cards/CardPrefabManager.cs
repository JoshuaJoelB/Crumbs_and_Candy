using UnityEngine;

public class CardPrefabManager : MonoBehaviour
{
    public static CardPrefabManager Instance;

    [Header("Assign your Card Prefabs here")]
    public CardUI basicPrefab;
    public CardUI normalPrefab;
    public CardUI warriorPrefab;
    public CardUI magePrefab;
    public CardUI tricksterPrefab;
    public CardUI berserkerPrefab;
    
    [Header("Enemy & Special Cards")]
    public CardUI enemyRegularPrefab; // Para sa ENEMY-ATTACK-001
    public CardUI uniqueEnemyPrefab;  // Para sa SC-001, WIS-001, etc.
    public CardUI junkPrefab;         // Para sa JUNK-001

    void Awake() { Instance = this; }

    public CardUI GetPrefab(CardClass cardClass, string cardId = "")
    {
        // Special cases para sa Enemy class
        if (cardClass == CardClass.Enemy)
        {
            if (cardId == "JUNK-001") return junkPrefab;
            if (cardId.StartsWith("ENEMY-ATTACK-")) return enemyRegularPrefab;
            
            // Kung unique enemy card (SC-001, WIS-001, PD-001, etc.)
            return uniqueEnemyPrefab; 
        }

        switch (cardClass)
        {
            case CardClass.Basic: return basicPrefab;
            case CardClass.Normal: return normalPrefab;
            case CardClass.Warrior: return warriorPrefab;
            case CardClass.Mage: return magePrefab;
            case CardClass.Trickster: return tricksterPrefab;
            case CardClass.Berserker: return berserkerPrefab;
            default: return basicPrefab;
        }
    }
}