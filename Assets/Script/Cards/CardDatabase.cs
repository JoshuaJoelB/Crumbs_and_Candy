using System.Collections.Generic;

public static class CardDatabase
{
    static readonly Dictionary<string, CardData> cards = new Dictionary<string, CardData>();

    static CardDatabase()
    {
        // ================= BASIC CARDS =================
        Add(new CardData(
            "BASIC-001", "Basic Attack",
            "Deal {value} physical damage.",
            CardClass.Basic, CardEffectType.Damage,
            maxLevel: 3, cost: 0,
            values: new[] { 3, 4, 5 }));

        Add(new CardData(
            "BASIC-002", "Basic Guard",
            "Gain {value} armor.",
            CardClass.Basic, CardEffectType.Armor,
            maxLevel: 3, cost: 0,
            values: new[] { 3, 4, 5 }));

        Add(new CardData(
            "BASIC-003", "Basic Mana",
            "Restore {value} mana.",
            CardClass.Basic, CardEffectType.RestoreMana,
            maxLevel: 3, cost: 0,
            values: new[] { 1, 2, 3 }));

        // ================= NORMAL CLASS =================
        Add(new CardData(
            "NOR-001", "Heavy Attack",
            "Deal {value} physical damage.",
            CardClass.Normal, CardEffectType.Damage,
            maxLevel: 2, cost: 2,
            values: new[] { 13, 17 }));

        Add(new CardData(
            "NOR-002", "Basic Guard",
            "Gain {value} armor.",
            CardClass.Normal, CardEffectType.Armor,
            maxLevel: 2, cost: 1,
            values: new[] { 8, 11 }));

        // ================= WARRIOR =================
        Add(new CardData(
            "WAR-001", "Parry",
            "Nullify the opponent's next card. Draw {value} card(s).",
            CardClass.Warrior, CardEffectType.NullifyNextCard,
            maxLevel: 2, cost: 1,
            values: new[] { 0, 1 }));

        Add(new CardData(
            "WAR-002", "Pierce",
            "Deal {value} direct HP damage (bypasses armor).",
            CardClass.Warrior, CardEffectType.DirectHpDamage,
            maxLevel: 2, cost: 2,
            values: new[] { 10, 14 }));

        // ================= MAGE =================
        Add(new CardData(
            "MAG-001", "Fireball",
            "Deal {value} magic damage.",
            CardClass.Mage, CardEffectType.MagicDamage,
            maxLevel: 2, cost: 2,
            values: new[] { 12, 16 }));

        Add(new CardData(
            "MAG-002", "Frost Bind",
            "Deal {value} magic damage. Reduce the opponent's next attack by {secondary}.",
            CardClass.Mage, CardEffectType.ReduceNextAttack,
            maxLevel: 2, cost: 2,
            values: new[] { 6, 9 },
            secondary: new[] { 4, 6 }));

        // ================= TRICKSTER =================
        Add(new CardData(
            "TRI-001", "Counter",
            "Counter the opponent's next attack, return its damage. Draw {value} card(s).",
            CardClass.Trickster, CardEffectType.CounterNextAttack,
            maxLevel: 2, cost: 2,
            values: new[] { 1, 2 }));

        Add(new CardData(
            "TRI-002", "Prankster",
            "Add {value} Junk Card(s) to the opponent's deck.",
            CardClass.Trickster, CardEffectType.AddJunkCards,
            maxLevel: 2, cost: 1,
            values: new[] { 1, 2 }));

        // ================= BERSERKER =================
        Add(new CardData(
            "BER-001", "Bloodthirst",
            "Deal {value} direct HP damage. Restore {secondary} HP.",
            CardClass.Berserker, CardEffectType.DirectHpDamage,
            maxLevel: 2, cost: 2,
            values: new[] { 10, 14 },
            secondary: new[] { 4, 6 }));

        Add(new CardData(
            "BER-002", "Rampage",
            "Attack {tertiary} times, {value} damage each.",
            CardClass.Berserker, CardEffectType.MultiHit,
            maxLevel: 2, cost: 3,
            values: new[] { 5, 7 },
            tertiary: new[] { 3, 3 }));

        // ================= UNIQUE ENEMY CARDS (MAX LEVEL) =================
        Add(new CardData(
            "SC-001", "Straw Trap",
            "Deal {value} damage. Reduce opponent's next attack by {secondary}.",
            CardClass.Enemy, CardEffectType.DamageAndReduce,
            maxLevel: 1, cost: 1,
            values: new[] { 5 },
            secondary: new[] { 3 }));

        Add(new CardData(
            "WIS-001", "Soul Flame",
            "Deal {value} magic damage.",
            CardClass.Enemy, CardEffectType.MagicDamage,
            maxLevel: 1, cost: 2,
            values: new[] { 11 }));

        Add(new CardData(
            "PD-001", "False Reflection",
            "Gain {value} armor. Draw {secondary} card(s).",
            CardClass.Enemy, CardEffectType.ArmorAndDraw,
            maxLevel: 1, cost: 2,
            values: new[] { 8 },
            secondary: new[] { 1 }));

        Add(new CardData(
            "WW-001", "Savage Bite",
            "Deal {value} physical damage. Restore {secondary} HP.",
            CardClass.Enemy, CardEffectType.DirectHpDamage,
            maxLevel: 1, cost: 3,
            values: new[] { 13 },
            secondary: new[] { 4 }));

        Add(new CardData(
            "WIT-001", "Wicked Curse",
            "Deal {value} magic damage. Reduce opponent's next attack by {secondary}.",
            CardClass.Enemy, CardEffectType.DamageAndReduce,
            maxLevel: 1, cost: 2,
            values: new[] { 7 },
            secondary: new[] { 4 }));

        // ================= ENEMY REGULAR DECK CARDS =================
        // Scarecrow
        Add(new CardData("EN-SC-BASIC", "Scarecrow Basic Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 3 }));
        Add(new CardData("EN-SC-REGULAR", "Scarecrow Regular Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 6 }));

        // Will-O'-Wisp
        Add(new CardData("EN-WIS-BASIC", "Wisp Basic Attack",
            "Deal {value} magic damage.", CardClass.Enemy, CardEffectType.MagicDamage,
            maxLevel: 1, cost: 0, values: new[] { 3 }));
        Add(new CardData("EN-WIS-REGULAR", "Wisp Regular Attack",
            "Deal {value} magic damage.", CardClass.Enemy, CardEffectType.MagicDamage,
            maxLevel: 1, cost: 0, values: new[] { 6 }));
        Add(new CardData("EN-WIS-MANA", "Wisp Mana",
            "Restore {value} mana.", CardClass.Enemy, CardEffectType.RestoreMana,
            maxLevel: 1, cost: 0, values: new[] { 2 }));

        // Porcelain Doll
        Add(new CardData("EN-PD-BASIC", "Doll Basic Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 3 }));
        Add(new CardData("EN-PD-REGULAR", "Doll Regular Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 9 }));
        Add(new CardData("EN-PD-MANA", "Doll Mana",
            "Restore {value} mana.", CardClass.Enemy, CardEffectType.RestoreMana,
            maxLevel: 1, cost: 0, values: new[] { 2 }));

        // Werewolf
        Add(new CardData("EN-WW-BASIC", "Werewolf Basic Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 3 }));
        Add(new CardData("EN-WW-REGULAR", "Werewolf Regular Attack",
            "Deal {value} damage.", CardClass.Enemy, CardEffectType.Damage,
            maxLevel: 1, cost: 0, values: new[] { 12 }));

        // Witch
        Add(new CardData("EN-WIT-BASIC", "Witch Basic Attack",
            "Deal {value} magic damage.", CardClass.Enemy, CardEffectType.MagicDamage,
            maxLevel: 1, cost: 0, values: new[] { 3 }));
        Add(new CardData("EN-WIT-REGULAR", "Witch Regular Attack",
            "Deal {value} magic damage.", CardClass.Enemy, CardEffectType.MagicDamage,
            maxLevel: 1, cost: 0, values: new[] { 10 }));
        Add(new CardData("EN-WIT-MANA", "Witch Mana",
            "Restore {value} mana.", CardClass.Enemy, CardEffectType.RestoreMana,
            maxLevel: 1, cost: 0, values: new[] { 2 }));

        // ================= JUNK =================
        Add(new CardData(
            "JUNK-001", "Junk",
            "Unplayable. Discard at end of turn.",
            CardClass.Enemy, CardEffectType.Junk,
            maxLevel: 1, cost: 0));
    }

    static void Add(CardData c)
    {
        if (cards.ContainsKey(c.id))
            throw new System.Exception($"Duplicate Card ID: {c.id}");
        cards.Add(c.id, c);
    }

    public static CardData Get(string id)
    {
        if (!cards.TryGetValue(id, out var c))
            throw new System.Exception($"Unknown card ID: {id}");
        return c;
    }

    public static List<string> AllIds() => new List<string>(cards.Keys);
    public static bool Exists(string id) => cards.ContainsKey(id);
}