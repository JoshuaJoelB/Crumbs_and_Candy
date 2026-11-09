[System.Serializable]
public class EnemyData
{
    public string id, enemyName, difficulty;
    public int maxHp, maxMana, attackDamage, specialCost, specialBonus, xpReward, coinReward;

    public EnemyData(string id, string name, string difficulty, int maxHp, int maxMana,
                     int attackDamage, int specialCost, int specialBonus, int xpReward, int coinReward)
    {
        this.id = id; enemyName = name; this.difficulty = difficulty;
        this.maxHp = maxHp; this.maxMana = maxMana; this.attackDamage = attackDamage;
        this.specialCost = specialCost; this.specialBonus = specialBonus;
        this.xpReward = xpReward; this.coinReward = coinReward;
    }
}