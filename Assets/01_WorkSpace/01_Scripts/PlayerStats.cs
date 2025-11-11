using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int hp = 100;
    public int hpLossPerSecond = 0;
    public int damage = 100;
    public int attackSize = 100;      // Used for box size
    public float attackSpeed = 1f;    // Attacks per second
    public int exp = 0;
    public int baseReflection = 0;
    public int armor = 0;
    public int bossArmor = 0;
    public int bossDamage = 0;

    [Header("Currencies")]
    public int redBits = 0;
    public int blueBits = 0;
    public int purpleBits = 0;

    public bool HasEnoughBits(string type, int amount)
    {
        return type switch
        {
            "redBits" => redBits >= amount,
            "blueBits" => blueBits >= amount,
            "purpleBits" => purpleBits >= amount,
            _ => false
        };
    }

    public void SpendBits(string type, int amount)
    {
        switch (type)
        {
            case "redBits": redBits -= amount; break;
            case "blueBits": blueBits -= amount; break;
            case "purpleBits": purpleBits -= amount; break;
        }
    }
}
