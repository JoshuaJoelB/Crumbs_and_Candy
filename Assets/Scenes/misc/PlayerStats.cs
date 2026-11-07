using UnityEngine;

[CreateAssetMenu(menuName = "Testing/PlayerStats", fileName = "NewPlayerStats")]
public class PlayerStats : ScriptableObject
{
	[SerializeField] private int hp;
	[SerializeField] private int atk;
	[SerializeField] private int def;
	[SerializeField] private int spd; 

	public int hpValue => hp;
	public int atkValue => atk;
	public int defValue => def;
	public int spdValue => spd; 

}