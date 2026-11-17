public enum CardEffectType
{
    Damage,
    MagicDamage,
    DirectHpDamage,
    Armor,
    RestoreMana,
    RestoreHp,
    ReduceNextAttack,
    NullifyNextCard,
    CounterNextAttack,
    MultiHit,
    AddJunkCards,
    DrawCards,
    Junk,

    // Para sa unique enemy cards
    DamageAndReduce,    // SC-001, WIT-001
    ArmorAndDraw        // PD-001
}  