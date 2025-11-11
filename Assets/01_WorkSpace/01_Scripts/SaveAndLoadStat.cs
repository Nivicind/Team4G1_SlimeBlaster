using UnityEngine;
using System.Collections.Generic;

public class SaveAndLoadStat : MonoBehaviour
{
    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("All Upgrade Buttons")]
    public List<UpgradeButton> allUpgradeButtons;

    private void Start()
    {
        // Apply all upgrades at the start
        ApplyAllUpgrades();
    }

    /// <summary>
    /// Apply all upgrades from buttons to PlayerStats
    /// </summary>
    private void ApplyAllUpgrades()
    {
        foreach (var button in allUpgradeButtons)
        {
            var node = button.nodeInstance;
            if (node.currentLevel > 0)
            {
                int totalValue = node.data.perUpgradeValue * node.currentLevel;
                ApplyUpgrade(node, totalValue);
            }
        }
    }

    /// <summary>
    /// Central method to apply a value to the correct stat
    /// </summary>
    private void ApplyUpgrade(NodeInstance node, int value)
    {
        switch (node.data.stat)
        {
            case "hp": playerStats.hp += value; break;
            case "hpLossPerSecond": playerStats.hpLossPerSecond += value; break;
            case "damage": playerStats.damage += value; break;
            case "attackSize": playerStats.attackSize += value; break;
            case "exp": playerStats.exp += value; break;
            case "baseReflection": playerStats.baseReflection += value; break;
            case "armor": playerStats.armor += value; break;
            case "bossArmor": playerStats.bossArmor += value; break;
            case "bossDamage": playerStats.bossDamage += value; break;
            default: Debug.LogWarning($"Unknown stat: {node.data.stat}"); break;
        }
    }
}
