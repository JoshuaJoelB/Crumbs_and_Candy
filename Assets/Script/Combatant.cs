using UnityEngine;

public class Combatant
{
    public string name;
    public int maxHp, hp, maxStamina, stamina;
    public int armor, attackReduction;

    public bool parryBlock;
    public bool counterIncoming;
    public int  junkToAdd;

    public Combatant(string name, int maxHp, int maxStamina, int startStamina)
    {
        this.name = name;
        this.maxHp = hp = maxHp;
        this.maxStamina = maxStamina;
        stamina = startStamina;
    }

    public void TakeDamage(int amount)
    {
        int absorbed = Mathf.Min(amount, armor);
        armor -= absorbed;
        hp = Mathf.Max(0, hp - (amount - absorbed));
    }

    public void TakeDirectDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
    }

    public void RestoreStamina(int amount)
    {
        stamina = Mathf.Min(maxStamina, stamina + amount);
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(maxHp, hp + amount);
    }

    public int ResolveOutgoingAttack(int rawDamage)
    {
        int result = Mathf.Max(0, rawDamage - attackReduction);
        attackReduction = 0;
        return result;
    }
}