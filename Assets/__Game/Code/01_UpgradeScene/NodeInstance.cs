using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NodeInstance
{
    public SONodeData data;               // Reference to the blueprint
    public NodeInstance unlockRoot;       // Reference to another NodeInstance it depends on
    public int unlockRequirementLevel = 1;

    public bool unlocked = false;
    public int currentLevel = 0;

    /// <summary>
    /// Check if this node can be upgraded
    /// </summary>
    public bool CanUpgrade()
    {
        if (!unlocked) return false;

        // Check if unlockRoot meets the requirement
        if (unlockRoot != null && unlockRoot.currentLevel < unlockRequirementLevel)
            return false;

        return currentLevel < data.maxLevel;
    }

    /// <summary>
    /// Upgrade this node
    /// </summary>
    public void Upgrade()
    {
        if (CanUpgrade())
            currentLevel++;
    }

    /// <summary>
    /// Get cost for next level using formula in the SONodeData
    /// </summary>
    public int GetCostForNextLevel()
    {
        int nextLevel = currentLevel + 1;

        // Replace 'n' in formula with current level
        string formula = data.costFormula.Replace("n", nextLevel.ToString());

        try
        {
            var dt = new System.Data.DataTable();
            var result = dt.Compute(formula, "");
            return Mathf.Max(1, Mathf.RoundToInt(System.Convert.ToSingle(result)));
        }
        catch
        {
            Debug.LogWarning($"Invalid cost formula for {data.upgradeName}: {data.costFormula}");
            return 1;
        }
    }

    /// <summary>
    /// Apply this node's upgrade effect to PlayerStats
    /// </summary>
    public void ApplyUpgradeEffect(PlayerStats playerStats)
    {
        // Use enum-based stat
        if (playerStats != null)
        {
            playerStats.AddStat(data.stat, data.perUpgradeValue);
        }
    }

    /// <summary>
    /// Update unlock status based on unlockRoot
    /// </summary>
    public void UpdateUnlockStatus()
    {
        if (unlockRoot == null)
        {
            unlocked = true; // root node always unlocked
        }
        else
        {
            unlocked = unlockRoot.currentLevel >= unlockRequirementLevel;
        }
    }
}
