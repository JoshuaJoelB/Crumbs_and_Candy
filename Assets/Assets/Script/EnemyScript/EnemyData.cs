using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyCardEntry
{
    public string cardId;
    public int level;
    public int copies;

    public EnemyCardEntry(string cardId, int level, int copies)
    {
        this.cardId = cardId; this.level = level; this.copies = copies;
    }
}

[System.Serializable]
public class EnemyData
{
    public string id, enemyName, difficulty;
    public string battleSceneName; // <-- BAGONG DAGDAG PARA SA SCENE
    public int maxHp, maxMana, startingMana;
    public int xpReward, coinReward;

    public string uniqueCardId;
    public List<EnemyCardEntry> regularDeck;
    public List<EnemyCardEntry> classDeck;

    public EnemyData(string id, string name, string difficulty, string battleSceneName,
                     int maxHp, int maxMana, int startingMana, int xpReward, int coinReward,
                     string uniqueCardId,
                     List<EnemyCardEntry> regularDeck,
                     List<EnemyCardEntry> classDeck)
    {
        this.id = id; enemyName = name; this.difficulty = difficulty;
        this.battleSceneName = battleSceneName; // <-- I-assign
        this.maxHp = maxHp; this.maxMana = maxMana; this.startingMana = startingMana;
        this.xpReward = xpReward; this.coinReward = coinReward;
        this.uniqueCardId = uniqueCardId;
        this.regularDeck = regularDeck;
        this.classDeck = classDeck;
    }
}