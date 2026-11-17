using System;

[Serializable]
public class CardData
{
    public string id;
    public string cardName;
    public string description;
    public CardClass cardClass;
    public CardEffectType effect;

    public int maxLevel;
    public int cost;

    public int[] values;
    public int[] secondary;
    public int[] tertiary;

    public bool usableByPlayer = true;

    public CardData(string id, string name, string desc, CardClass cls,
                    CardEffectType effect, int maxLevel, int cost,
                    int[] values = null, int[] secondary = null, int[] tertiary = null)
    {
        this.id = id;
        cardName = name;
        description = desc;
        cardClass = cls;
        this.effect = effect;
        this.maxLevel = maxLevel;
        this.cost = cost;
        this.values    = values    ?? new int[0];
        this.secondary = secondary ?? new int[0];
        this.tertiary  = tertiary  ?? new int[0];
    }

    public int GetValue(int level)     => Pick(values,    level);
    public int GetSecondary(int level) => Pick(secondary, level);
    public int GetTertiary(int level)  => Pick(tertiary,  level);

    static int Pick(int[] arr, int level)
    {
        if (arr == null || arr.Length == 0) return 0;
        int i = Math.Min(Math.Max(level, 1), arr.Length) - 1;
        return arr[i];
    }
}