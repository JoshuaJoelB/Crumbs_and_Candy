using UnityEngine;

public class CardInstance
{
    static int counter = 0;

    public readonly int instanceId;
    public readonly CardData data;
    public int level;

    public CardInstance(CardData data, int level = 1)
    {
        this.data = data;
        this.level = Mathf.Clamp(level, 1, data.maxLevel);
        instanceId = ++counter;
    }

    public bool CanUpgrade => level < data.maxLevel;

    public bool Upgrade()
    {
        if (!CanUpgrade) return false;
        level++;
        return true;
    }

    public int Value     => data.GetValue(level);
    public int Secondary => data.GetSecondary(level);
    public int Tertiary  => data.GetTertiary(level);

    public string GetDescription()
    {
        return data.description
            .Replace("{value}",     Value.ToString())
            .Replace("{secondary}", Secondary.ToString())
            .Replace("{tertiary}",  Tertiary.ToString());
    }
}