using UnityEngine;

[CreateAssetMenu(fileName = "SOEnemyData", menuName = "Game/Enemy Data")]
public class SOEnemyData : ScriptableObject
{
    [Header("Stats")]
    public int hp = 100;
    public float baseReflectionMultiplier = 1f;
    public int exp = 10;
    public EnumCurrency currencyType = EnumCurrency.redBits;
    public int baseCurrencyAmount = 1;

    [Header("Spawn Settings")]
    public int spawnAmount = 1;       // Number of enemies per interval
    public float spawnInterval = 1f;  // Interval in seconds
    public int maxCapacity = 10;      // Max enemies alive simultaneously
    public float startTime = 0f;      // When this enemy type starts spawning (in seconds)
}
