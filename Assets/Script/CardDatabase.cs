using System.Collections.Generic;

public static class CardDatabase
{
    static readonly Dictionary<string, CardData> cards = new Dictionary<string, CardData>();

    static CardDatabase()
    {
        Add(new CardData("BasicStrikeCard001", "Basic Strike", "Deal 5 damage.", 0, CardEffectType.Damage, 5));
        Add(new CardData("HeavyStrikeCard002", "Heavy Strike", "Deal 10 damage.", 0, CardEffectType.Damage, 10));
        Add(new CardData("StaminaCardCard003", "Stamina Card", "Restore 2 Stamina.", 0, CardEffectType.RestoreStamina, 2));
        Add(new CardData("GuardCard004", "Guard", "Gain 5 armor vs the next enemy attack.", 1, CardEffectType.Armor, 5));
        Add(new CardData("FireSpellCard005", "Fire Spell", "Deal 8 damage.", 2, CardEffectType.Damage, 8));
        Add(new CardData("ArcaneShieldCard006", "Arcane Shield", "Block up to 4 damage from the next attack.", 1, CardEffectType.Shield, 4));
        Add(new CardData("FrostBindCard007", "Frost Bind", "Enemy's next attack is reduced by 3 (min 0).", 1, CardEffectType.Frost, 3));
    }

    static void Add(CardData c) { cards.Add(c.id, c); } // errors if an ID is duplicated, which is what we want

    public static CardData Get(string id) { return cards[id]; }
    public static List<string> AllIds() { return new List<string>(cards.Keys); }
}