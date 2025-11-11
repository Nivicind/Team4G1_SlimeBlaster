using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class NodeInstance
{
    public SONodeData data;               // Reference to the blueprint
    public NodeInstance unlockRoot;     // Reference to another NodeInstance it depends on
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

    public void Upgrade()
    {
        if (CanUpgrade())
            currentLevel++;
    }

    public int GetCostForNextLevel()
    {
        int n = currentLevel + 1;
        string formula = data.costFormula.Replace("n", n.ToString());
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
}
