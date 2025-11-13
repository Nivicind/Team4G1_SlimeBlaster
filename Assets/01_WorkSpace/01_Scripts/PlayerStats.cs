using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnumStat
{
    hp,
    hpLossPerSecond,
    damage,
    attackSizePercent,
    attackSpeed,
    exp,
    baseReflection,
    armor,
    bossArmor,
    bossDamage,
    critRatePercent,
    critDamagePercent,
    additionalDamagePerEnemyInAreaPercent,
    additionalRedBitsDropPerEnemy,
    spawnRatePercent
}

public enum EnumCurrency
{
    redBits,
    blueBits,
    purpleBits
}

public class PlayerStats : MonoBehaviour
{
    #region Inspector Variables
    [Header("Player Stats (Inspector)")]
    public int hp = 100;
    public int hpLossPerSecond = 1;
    public int damage = 10;
    public int attackSizePercent = 100;
    public int attackSpeed = 1;
    public int exp = 0;
    public int baseReflection = 0;
    public int armor = 0;
    public int bossArmor = 0;
    public int bossDamage = 0;
    public int critRatePercent = 5;
    public int critDamagePercent = 150;
    public int additionalDamagePerEnemyInAreaPercent = 0;
    public int additionalRedBitsDropPerEnemy = 0;
    public int spawnRatePercent = 100;

    [Header("Currencies (Inspector)")]
    public int redBits = 1000;
    public int blueBits = 1000;
    public int purpleBits = 1000;
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
        statsDict[EnumStat.hp] = 100;
        statsDict[EnumStat.hpLossPerSecond] = 1;
        statsDict[EnumStat.damage] = 10;
        statsDict[EnumStat.attackSizePercent] = 100;
        statsDict[EnumStat.attackSpeed] = 1;
        statsDict[EnumStat.exp] = 0;
        statsDict[EnumStat.baseReflection] = 0;
        statsDict[EnumStat.armor] = 0;
        statsDict[EnumStat.bossArmor] = 0;
        statsDict[EnumStat.bossDamage] = 0;
        statsDict[EnumStat.critRatePercent] = 5;
        statsDict[EnumStat.critDamagePercent] = 150;
        statsDict[EnumStat.additionalDamagePerEnemyInAreaPercent] = 0;
        statsDict[EnumStat.additionalRedBitsDropPerEnemy] = 0;
        statsDict[EnumStat.spawnRatePercent] = 100;

        currencyDict[EnumCurrency.redBits] = 1000;
        currencyDict[EnumCurrency.blueBits] = 1000;
        currencyDict[EnumCurrency.purpleBits] = 1000;
    }
    private void UpdateValueToInspector()
    {
        // Sync inspector variables to dictionary values
        hp = statsDict[EnumStat.hp];
        hpLossPerSecond = statsDict[EnumStat.hpLossPerSecond];
        damage = statsDict[EnumStat.damage];
        attackSizePercent = statsDict[EnumStat.attackSizePercent];
        attackSpeed = statsDict[EnumStat.attackSpeed];
        exp = statsDict[EnumStat.exp];
        baseReflection = statsDict[EnumStat.baseReflection];
        armor = statsDict[EnumStat.armor];
        bossArmor = statsDict[EnumStat.bossArmor];
        bossDamage = statsDict[EnumStat.bossDamage];
        critRatePercent = statsDict[EnumStat.critRatePercent];
        critDamagePercent = statsDict[EnumStat.critDamagePercent];
        additionalDamagePerEnemyInAreaPercent = statsDict[EnumStat.additionalDamagePerEnemyInAreaPercent];
        additionalRedBitsDropPerEnemy = statsDict[EnumStat.additionalRedBitsDropPerEnemy];
        spawnRatePercent = statsDict[EnumStat.spawnRatePercent];

        redBits = currencyDict[EnumCurrency.redBits];
        blueBits = currencyDict[EnumCurrency.blueBits];
        purpleBits = currencyDict[EnumCurrency.purpleBits];
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
    }

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
