public enum CardEffectType { Damage, RestoreStamina, Armor, Frost }

[System.Serializable]
public class CardData
{
    public string id;           // e.g. BasicStrikeCard001
    public string cardName;
    public string description;
    public int cost;            // Stamina cost
    public CardEffectType effect;
    public int value;           // 5 damage, 10 damage, etc.

    public CardData(string id, string name, string desc, int cost, CardEffectType effect, int value)
    {
        this.id = id; cardName = name; description = desc;
        this.cost = cost; this.effect = effect; this.value = value;
    }
}