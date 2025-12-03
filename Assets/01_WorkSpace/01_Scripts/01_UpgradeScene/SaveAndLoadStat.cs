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
                // Total upgrade value = per upgrade * current level
                int totalValue = node.data.perUpgradeValue * node.currentLevel;
                playerStats.AddStat(node.data.stat, totalValue);
            }
        }
    }
}
