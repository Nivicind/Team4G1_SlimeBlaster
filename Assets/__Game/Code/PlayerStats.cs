using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region Inspector Variables
    [Header("Base Stats")]
    public int baseHp = 100;
    public int baseDamage = 10;
    public int baseArmor = 0;

    [Header("Player Stats (Inspector)")]
    public int hp = 100; //ok
    public int hpLossPerSecond = 1; //ok
    public int damage = 10; //ok
    public int attackSizePercent = 100;//ok
    public int attackSpeed = 1;
    public int exp = 0;
    public int level = 1;
    public int baseReflection = 0; //ok
    public int armor = 0; //ok
    public int bossArmor = 0;
    public int bossDamage = 0; //ok
    public int critRatePercent = 5;
    public int critDamagePercent = 150;
    public int additionalDamagePerEnemyInAreaPercent = 0;
    public int additionalBlueBitsDropPerEnemy = 0;  //ok
    public int additionalPinkBitsDropPerEnemy = 0; //ok
    public int additionalYellowBitsDropPerEnemy = 0;  //ok
    public int additionalGreenBitsDropPerEnemy = 0;  //ok
    public int spawnRatePercent = 100; //ok

    [Header("Currencies (Inspector)")]
    public int blueBits = 1000;
    public int pinkBits = 1000;
    public int yellowBits = 1000;
    public int greenBits = 1000;
    #endregion

    // Runtime dictionaries
    private Dictionary<EnumStat, int> statsDict = new Dictionary<EnumStat, int>();
    private Dictionary<EnumCurrency, int> currencyDict = new Dictionary<EnumCurrency, int>();

    private void Awake()
    {
        SetDefaultValues();
    }
    private void Update()
    {
        UpdateValueToInspector();
    }

    private void SetDefaultValues()
    {
        // Initialize dictionary with default values
        statsDict[EnumStat.baseHp] = 100;
        statsDict[EnumStat.baseDamage] = 10;
        statsDict[EnumStat.baseArmor] = 0;

        statsDict[EnumStat.hp] = 100;
        statsDict[EnumStat.hpLossPerSecond] = 1;
        statsDict[EnumStat.damage] = 10;
        statsDict[EnumStat.attackSizePercent] = 100;
        statsDict[EnumStat.attackSpeed] = 1;
        statsDict[EnumStat.exp] = 0;
        statsDict[EnumStat.level] = 1;
        statsDict[EnumStat.baseReflection] = 5;
        statsDict[EnumStat.armor] = 0;
        statsDict[EnumStat.bossArmor] = 0;
        statsDict[EnumStat.bossDamage] = 0;
        statsDict[EnumStat.critRatePercent] = 5;
        statsDict[EnumStat.critDamagePercent] = 150;
        statsDict[EnumStat.additionalDamagePerEnemyInAreaPercent] = 0;
        statsDict[EnumStat.additionalBlueBitsDropPerEnemy] = 0;
        statsDict[EnumStat.additionalPinkBitsDropPerEnemy] = 0;
        statsDict[EnumStat.additionalYellowBitsDropPerEnemy] = 0;
        statsDict[EnumStat.additionalGreenBitsDropPerEnemy] = 0;
        statsDict[EnumStat.spawnRatePercent] = 100;

        currencyDict[EnumCurrency.blueBits] = 1000;
        currencyDict[EnumCurrency.pinkBits] = 1000;
        currencyDict[EnumCurrency.yellowBits] = 1000;
        currencyDict[EnumCurrency.greenBits] = 1000;
    }
    private void UpdateValueToInspector()
    {
        // Sync inspector variables to dictionary values
        baseHp = statsDict[EnumStat.baseHp];
        baseDamage = statsDict[EnumStat.baseDamage];
        baseArmor = statsDict[EnumStat.baseArmor];

        hp = statsDict[EnumStat.hp];
        hpLossPerSecond = statsDict[EnumStat.hpLossPerSecond];
        damage = statsDict[EnumStat.damage];
        attackSizePercent = statsDict[EnumStat.attackSizePercent];
        attackSpeed = statsDict[EnumStat.attackSpeed];
        exp = statsDict[EnumStat.exp];
        level = statsDict[EnumStat.level];
        baseReflection = statsDict[EnumStat.baseReflection];
        armor = statsDict[EnumStat.armor];
        bossArmor = statsDict[EnumStat.bossArmor];
        bossDamage = statsDict[EnumStat.bossDamage];
        critRatePercent = statsDict[EnumStat.critRatePercent];
        critDamagePercent = statsDict[EnumStat.critDamagePercent];
        additionalDamagePerEnemyInAreaPercent = statsDict[EnumStat.additionalDamagePerEnemyInAreaPercent];
        additionalBlueBitsDropPerEnemy = statsDict[EnumStat.additionalBlueBitsDropPerEnemy];
        additionalPinkBitsDropPerEnemy = statsDict[EnumStat.additionalPinkBitsDropPerEnemy];
        additionalYellowBitsDropPerEnemy = statsDict[EnumStat.additionalYellowBitsDropPerEnemy];
        additionalGreenBitsDropPerEnemy = statsDict[EnumStat.additionalGreenBitsDropPerEnemy];
        spawnRatePercent = statsDict[EnumStat.spawnRatePercent];

        blueBits = currencyDict[EnumCurrency.blueBits];
        pinkBits = currencyDict[EnumCurrency.pinkBits];
        yellowBits = currencyDict[EnumCurrency.yellowBits];
        greenBits = currencyDict[EnumCurrency.greenBits];
    }

    // Runtime methods
    public int GetStatValue(EnumStat type) => statsDict.TryGetValue(type, out int val) ? val : 0;
    public void SetStatValue(EnumStat type, int value)
    {
        statsDict[type] = value; // only dictionary changes
    }
    public void AddStat(EnumStat type, int amount)
    {
        int current = GetStatValue(type);
        SetStatValue(type, current + amount);
        
        // Check for level up when exp is added
        if (type == EnumStat.exp)
        {
            CheckLevelUp();
        }
    }

    private void CheckLevelUp()
    {
        int currentExp = GetStatValue(EnumStat.exp);
        int currentLevel = GetStatValue(EnumStat.level);
        int expRequired = GetExpRequiredForLevel(currentLevel);

        while (currentExp >= expRequired)
        {
            // Level up!
            currentExp -= expRequired;
            currentLevel++;
            
            SetStatValue(EnumStat.exp, currentExp);
            SetStatValue(EnumStat.level, currentLevel);
            
            Debug.Log($"Level Up! Now level {currentLevel}");
            
            // Recalculate for next level
            expRequired = GetExpRequiredForLevel(currentLevel);
        }
    }

    private int GetExpRequiredForLevel(int level)
    {
        // Formula: level * 100
        // Level 1 → 100 exp, Level 2 → 200 exp, Level 3 → 300 exp, etc.
        return level * 100;
    }

    public int GetCurrency(EnumCurrency type)
        => currencyDict.TryGetValue(type, out int val) ? val : 0;
    public bool HasEnoughCurrency(EnumCurrency type, int amount)
        => currencyDict.TryGetValue(type, out int val) && val >= amount;
    public void SpendCurrency(EnumCurrency type, int amount)
    {
        if (!currencyDict.ContainsKey(type)) return;
        currencyDict[type] = Mathf.Max(0, currencyDict[type] - amount);
    }
    public void AddCurrency(EnumCurrency type, int amount)
    {
        if (currencyDict.ContainsKey(type))
            currencyDict[type] += amount;
        else
            currencyDict[type] = amount;
    }
}
