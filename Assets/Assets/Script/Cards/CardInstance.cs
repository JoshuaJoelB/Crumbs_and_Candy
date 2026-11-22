using UnityEngine;

public class CardInstance
{
    static int counter = 0;
    public readonly int instanceId;
    public readonly CardData data;
    public int Level { get; private set; }

    public CardInstance(CardData data, int level = 1) 
    { 
        this.data = data; 
        this.Level = level;
        instanceId = ++counter; 
    }

    public int Value => data.GetValue(Level);
    public int Secondary => data.GetSecondary(Level);
    public int Tertiary => data.GetTertiary(Level);

    public string GetDescription()
    {
        string desc = data.description;
        desc = desc.Replace("{value}", Value.ToString());
        desc = desc.Replace("{secondary}", Secondary.ToString());
        desc = desc.Replace("{tertiary}", Tertiary.ToString());
        return desc;
    }
}