[System.Serializable]
public class EnemyData
{
    public string id, enemyName, difficulty;
    public int maxHp, maxMana;
    public int startingMana;
    public int xpReward, coinReward;

    // Unique card (auto-cast kapag sapat ang mana)
    public string uniqueCardId;

    // Regular deck cards (weighted random)
    public string basicAttackCardId;
    public string regularAttackCardId;
    public string manaCardId;          // "" kung walang mana card
    public int    manaCardWeight;      // gaano kadalas i-draw ang mana card
    public int    basicAttackWeight;
    public int    regularAttackWeight;

    public EnemyData(string id, string name, string difficulty,
                     int maxHp, int maxMana, int startingMana,
                     int xpReward, int coinReward,
                     string uniqueCardId,
                     string basicAttackCardId, int basicAttackWeight,
                     string regularAttackCardId, int regularAttackWeight,
                     string manaCardId, int manaCardWeight)
    {
        this.id = id;
        enemyName = name;
        this.difficulty = difficulty;
        this.maxHp = maxHp;
        this.maxMana = maxMana;
        this.startingMana = startingMana;
        this.xpReward = xpReward;
        this.coinReward = coinReward;
        this.uniqueCardId = uniqueCardId;
        this.basicAttackCardId = basicAttackCardId;
        this.basicAttackWeight = basicAttackWeight;
        this.regularAttackCardId = regularAttackCardId;
        this.regularAttackWeight = regularAttackWeight;
        this.manaCardId = manaCardId;
        this.manaCardWeight = manaCardWeight;
    }
}