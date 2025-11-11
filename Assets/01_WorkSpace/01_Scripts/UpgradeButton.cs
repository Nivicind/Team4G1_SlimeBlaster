using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("Node Data")]
    public NodeInstance nodeInstance;          // Assign in inspector
    public UpgradeButton unlockRootButton;     // Drag the button this node depends on

    [Header("Player Stats")]
    public PlayerStats playerStats;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        // Assign the unlockRoot reference at runtime
        if (unlockRootButton != null)
            nodeInstance.unlockRoot = unlockRootButton.nodeInstance;

        // Initialize unlock state at start
        UpdateUnlockStatus();
    }

    private void OnClick()
    {
        if (!nodeInstance.CanUpgrade()) return;

        int cost = nodeInstance.GetCostForNextLevel();
        if (playerStats.HasEnoughBits(nodeInstance.data.costUnit, cost))
        {
            playerStats.SpendBits(nodeInstance.data.costUnit, cost);
            nodeInstance.Upgrade();
            ApplyUpgradeEffect();
            Debug.Log($"{nodeInstance.data.upgradeName} upgraded to level {nodeInstance.currentLevel}");

            // After upgrade, update unlocks for dependent nodes
            if (unlockRootButton != null)
                unlockRootButton.UpdateDependentUnlocks();
        }
        else
        {
            Debug.Log("Not enough resources!");
        }
    }

    private void ApplyUpgradeEffect()
    {
        int value = nodeInstance.data.perUpgradeValue;

        switch (nodeInstance.data.stat)
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
            default: Debug.LogWarning($"Unknown stat: {nodeInstance.data.stat}"); break;
        }
    }

    /// <summary>
    /// Update this node's unlock state
    /// </summary>
    public void UpdateUnlockStatus()
    {
        if (nodeInstance.unlockRoot == null)
        {
            nodeInstance.unlocked = true; // root node always unlocked
        }
        else
        {
            nodeInstance.unlocked = nodeInstance.unlockRoot.currentLevel >= nodeInstance.unlockRequirementLevel;
        }
    }

    /// <summary>
    /// Update all dependent nodes after this node upgrades
    /// </summary>
    public void UpdateDependentUnlocks()
    {
        // Find all UpgradeButtons in the scene (or you can store a list of dependents)
        UpgradeButton[] allButtons = FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.nodeInstance.unlockRoot == nodeInstance)
            {
                btn.UpdateUnlockStatus();
            }
        }
    }
}
