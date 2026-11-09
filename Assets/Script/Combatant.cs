using UnityEngine;

public class Combatant
{
    public string name;
    public int maxHp, hp, maxStamina, stamina;   // stamina = mana
    public int armor, attackReduction;

    public Combatant(string name, int maxHp, int maxStamina, int startStamina)
    {
        this.name = name;
        this.maxHp = hp = maxHp;
        this.maxStamina = maxStamina;
        stamina = startStamina;
    }

    // Damage ay dadaan muna sa armor, ang matitira ang tatama sa HP.
    public void TakeDamage(int amount)
    {
        int absorbed = Mathf.Min(amount, armor);
        armor -= absorbed;
        hp = Mathf.Max(0, hp - (amount - absorbed));
    }

    public void RestoreStamina(int amount)
    {
        stamina = Mathf.Min(maxStamina, stamina + amount);
    }

    // Frost Bind: binabawasan ang susunod na attack, hindi bababa sa 0.
    public int ResolveOutgoingAttack(int rawDamage)
    {
        int result = Mathf.Max(0, rawDamage - attackReduction);
        attackReduction = 0;
        return result;
    }
}