using UnityEngine;

public class Combatant
{
    public string name;
    public int maxHp, hp, maxStamina, stamina;
    public int armor, shield, attackReduction;

    public Combatant(string name, int maxHp, int maxStamina, int startStamina)
    {
        this.name = name; this.maxHp = hp = maxHp;
        this.maxStamina = maxStamina; stamina = startStamina;
    }

    public void TakeDamage(int amount) { hp = Mathf.Max(0, hp - amount); }

    public void RestoreStamina(int amount) { stamina = Mathf.Min(maxStamina, stamina + amount); }

    // Called when THIS combatant attacks. Frost Bind lowers it, never below 0.
    public int ResolveOutgoingAttack(int rawDamage)
    {
        int result = Mathf.Max(0, rawDamage - attackReduction);
        attackReduction = 0;                 // "next attack only"
        return result;
    }

    // Called when THIS combatant is attacked. Guard and Arcane Shield absorb damage.
    public void ReceiveAttack(int damage)
    {
        int absorbed = Mathf.Min(damage, armor); damage -= absorbed;
        absorbed = Mathf.Min(damage, shield);    damage -= absorbed;
        armor = 0; shield = 0;               // only protect against the NEXT attack
        TakeDamage(damage);
    }
}