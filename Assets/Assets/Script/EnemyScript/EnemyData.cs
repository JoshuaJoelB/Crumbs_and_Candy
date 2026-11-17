using System.Collections.Generic;

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
    public int maxHp, maxMana, startingMana;
    public int xpReward, coinReward;

    public string uniqueCardId;
    public string rewardCardId; // <-- BAGONG DAGDAG PARA SA REWARD
    public List<EnemyCardEntry> regularDeck;
    public List<EnemyCardEntry> classDeck;

    public EnemyData(string id, string name, string difficulty,
                     int maxHp, int maxMana, int startingMana, int xpReward, int coinReward,
                     string uniqueCardId, string rewardCardId,
                     List<EnemyCardEntry> regularDeck,
                     List<EnemyCardEntry> classDeck)
    {
        this.id = id; enemyName = name; this.difficulty = difficulty;
        this.maxHp = maxHp; this.maxMana = maxMana; this.startingMana = startingMana;
        this.xpReward = xpReward; this.coinReward = coinReward;
        this.uniqueCardId = uniqueCardId;
        this.rewardCardId = rewardCardId;
        this.regularDeck = regularDeck;
        this.classDeck = classDeck;
    }
}