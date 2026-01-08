using UnityEngine;

/// <summary>
/// 🎮 Game Config - Store magic numbers for designers
/// Create: Assets > Create > Game > Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                    Debug.LogError("❌ GameConfig not found! Create one in Resources folder!");
            }
            return _instance;
        }
    }

    [Header("⏱️ HP Loss")]
    [Tooltip("Seconds between each HP loss tick")]
    public float hpLossInterval = 1f;

    [Header("📐 EXP Formula")]
    [Tooltip("EXP needed = level × this value (100 = level 1 needs 100, level 2 needs 200...)")]
    public int expPerLevelMultiplier = 100;

    [Header("👾 Enemy Scaling (Per Level)")]
    [Tooltip("Multiply enemy HP by this for each level (index 0 = level 1, index 1 = level 2, etc.)")]
    public float[] enemyHealthMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    [Tooltip("Multiply enemy reflection by this for each level (index 0 = level 1, index 1 = level 2, etc.)")]
    public float[] enemyReflectionMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    [Tooltip("Multiply currency drop by this for each level (index 0 = level 1, index 1 = level 2, etc.)")]
    public float[] enemyCurrencyMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    /// <summary>
    /// 🎯 Get enemy health multiplier for the given level (1-based)
    /// Falls back to last value if level exceeds array length
    /// </summary>
    public float GetEnemyHealthMultiplier(int level)
    {
        if (enemyHealthMultipliers == null || enemyHealthMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(level - 1, 0, enemyHealthMultipliers.Length - 1);
        return enemyHealthMultipliers[index];
    }
    
    /// <summary>
    /// 🎯 Get enemy reflection multiplier for the given level (1-based)
    /// Falls back to last value if level exceeds array length
    /// </summary>
    public float GetEnemyReflectionMultiplier(int level)
    {
        if (enemyReflectionMultipliers == null || enemyReflectionMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(level - 1, 0, enemyReflectionMultipliers.Length - 1);
        return enemyReflectionMultipliers[index];
    }
    
    /// <summary>
    /// 🎯 Get enemy currency multiplier for the given level (1-based)
    /// Falls back to last value if level exceeds array length
    /// </summary>
    public float GetEnemyCurrencyMultiplier(int level)
    {
        if (enemyCurrencyMultipliers == null || enemyCurrencyMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(level - 1, 0, enemyCurrencyMultipliers.Length - 1);
        return enemyCurrencyMultipliers[index];
    }

    [Header("⚔️ Combat")]
    [Tooltip("Minimum damage after armor (can't go below this)")]
    public int minDamageAfterArmor = 1;
    
    [Tooltip("Minimum attack speed to prevent division by zero")]
    public float minAttackSpeed = 0.01f;

    [Header("🎯 Attack Size")]
    [Tooltip("Max attack size count (0-14 = 15 levels)")]
    public int maxAttackSizeCount = 14;
}
