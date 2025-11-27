using UnityEngine;
using System.Collections.Generic;

public class UpgradeButtonApearance : MonoBehaviour
{
    [Header("References")]
    public UpgradeButton upgradeButton;
    public GameObject icon;             // Icon that shows when unlocked
    public GameObject line;             // Line from this button to unlock root

    [Header("UI States")]
    public GameObject cannotAffordUI;   // Show when cannot afford
    public GameObject canAffordUI;      // Show when can afford
    public GameObject fullyUpgradedUI;  // Show when fully upgraded
    public GameObject defaultUI;        // Default state

    private void Start()
    {
        // Find line by name pattern if not assigned
        if (line == null && upgradeButton != null && upgradeButton.unlockRootObject != null)
        {
            string lineName = $"Line_{gameObject.name}_to_{upgradeButton.unlockRootObject.name}";
            Transform lineTransform = transform.parent.Find(lineName);
            if (lineTransform != null)
            {
                line = lineTransform.gameObject;
            }
        }
    }

    private void Update()
    {
        if (upgradeButton == null || upgradeButton.playerStats == null) return;

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        // If button is locked, hide all UI
        if (!upgradeButton.nodeInstance.unlocked)
        {
            HideAll();
            return;
        }

        // Check if fully upgraded
        bool isMaxLevel = upgradeButton.nodeInstance.currentLevel >= upgradeButton.nodeInstance.data.maxLevel;
        
        if (isMaxLevel)
        {
            ShowOnly(fullyUpgradedUI);
            return;
        }

        // Get cost and currency type
        int cost = upgradeButton.nodeInstance.GetCostForNextLevel();
        EnumCurrency currencyType = upgradeButton.nodeInstance.data.costUnit;

        // Check if player has enough currency (get playerStats from upgradeButton)
        bool canAfford = upgradeButton.playerStats.HasEnoughCurrency(currencyType, cost);

        if (canAfford)
        {
            ShowOnly(canAffordUI);
        }
        else
        {
            ShowOnly(cannotAffordUI);
        }
    }

    private void HideAll()
    {
        if (canAffordUI != null) canAffordUI.SetActive(false);
        if (cannotAffordUI != null) cannotAffordUI.SetActive(false);
        if (fullyUpgradedUI != null) fullyUpgradedUI.SetActive(false);
        if (defaultUI != null) defaultUI.SetActive(false);
        if (icon != null) icon.SetActive(false);
        if (line != null) line.SetActive(false);
    }

    private void ShowOnly(GameObject uiToShow)
    {
        // Hide all UI first
        if (canAffordUI != null) canAffordUI.SetActive(false);
        if (cannotAffordUI != null) cannotAffordUI.SetActive(false);
        if (fullyUpgradedUI != null) fullyUpgradedUI.SetActive(false);
        if (defaultUI != null) defaultUI.SetActive(false);

        // Show icon and line when unlocked
        if (icon != null) icon.SetActive(true);
        if (line != null) line.SetActive(true);

        // Show only the specified UI
        if (uiToShow != null) uiToShow.SetActive(true);
    }
}
