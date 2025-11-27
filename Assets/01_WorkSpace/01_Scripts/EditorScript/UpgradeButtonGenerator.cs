using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UpgradeButtonGenerator : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public List<SONodeData> allNodeData;

    [Header("Prefabs & References")]
    public GameObject upgradeButtonPrefab; // Prefab with UpgradeButton component + TMP Text
    public Transform parentContainer; // Parent object to hold buttons
    public PlayerStats playerStats; // Reference to player stats
    public List<GameObject> buttons; // List of existing buttons to set icons for

    [Header("Line Settings")]
    public float lineWidth = 0.1f;
    public Material lineMaterial;

    [ContextMenu("Generate Buttons")]
    public void GenerateButtons()
    {
        if (upgradeButtonPrefab == null || parentContainer == null || playerStats == null)
        {
            Debug.LogWarning("Assign prefab, parent container, and PlayerStats first!");
            return;
        }

        // Create a button for each ScriptableObject
        foreach (var nodeData in allNodeData)
        {
            // Check if a button for this node already exists
            bool exists = false;
            foreach (Transform child in parentContainer)
            {
                UpgradeButton existingBtn = child.GetComponent<UpgradeButton>();
                if (existingBtn != null && existingBtn.nodeInstance.data == nodeData)
                {
                    exists = true;
                    break;
                }
            }

            if (exists) continue; // Skip if already exists

            GameObject buttonObj = Instantiate(upgradeButtonPrefab, parentContainer);
            buttonObj.name = nodeData.name;

            UpgradeButton btn = buttonObj.GetComponent<UpgradeButton>();
            if (btn != null)
            {
                NodeInstance nodeInstance = new NodeInstance();
                nodeInstance.data = nodeData;
                btn.nodeInstance = nodeInstance;

                btn.playerStats = playerStats;

                TMP_Text tmp = buttonObj.GetComponentInChildren<TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = nodeData.name; // Show asset name
                }
            }
        }

        Debug.Log("Generated Upgrade Buttons! Total: " + parentContainer.childCount);
    }

    [ContextMenu("Set Button Icons")]
    public void SetButtonIcons()
    {
        if (buttons == null || buttons.Count == 0)
        {
            Debug.LogWarning("No buttons assigned in the list!");
            return;
        }

        foreach (var buttonObj in buttons)
        {
            if (buttonObj == null) continue;

            UpgradeButton btn = buttonObj.GetComponent<UpgradeButton>();
            if (btn != null && btn.nodeInstance != null && btn.nodeInstance.data != null)
            {
                SONodeData nodeData = btn.nodeInstance.data;
                
                // Find the "Icon" child GameObject
                Transform iconTransform = buttonObj.transform.Find("Icon");
                if (iconTransform != null)
                {
                    UnityEngine.UI.Image iconImage = iconTransform.GetComponent<UnityEngine.UI.Image>();
                    if (iconImage != null && nodeData.icon != null)
                    {
                        iconImage.sprite = nodeData.icon;
                        Debug.Log($"Set icon for {nodeData.upgradeName}");
                    }
                    else
                    {
                        Debug.LogWarning($"Image component or icon missing for {buttonObj.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Icon child GameObject not found in {buttonObj.name}");
                }
            }
        }

        Debug.Log("Button icons updated!");
    }

    [ContextMenu("Rename Buttons")]
    public void RenameButtons()
    {
        if (buttons == null || buttons.Count == 0)
        {
            Debug.LogWarning("No buttons assigned in the list!");
            return;
        }

        foreach (var buttonObj in buttons)
        {
            if (buttonObj == null) continue;

            UpgradeButton btn = buttonObj.GetComponent<UpgradeButton>();
            if (btn != null && btn.nodeInstance != null && btn.nodeInstance.data != null)
            {
                SONodeData nodeData = btn.nodeInstance.data;
                
                // Rename the button GameObject to match the SONodeData name
                buttonObj.name = nodeData.name;
                Debug.Log($"Renamed button to {nodeData.name}");
            }
        }

        Debug.Log("Buttons renamed!");
    }

    [ContextMenu("Generate Lines")]
    public void GenerateLines()
    {
        if (buttons == null)
        {
            Debug.LogError("Buttons list is NULL!");
            return;
        }

        if (buttons.Count == 0)
        {
            Debug.LogWarning("Buttons list is EMPTY! Add buttons to the list first.");
            return;
        }

        Debug.Log($"Starting line generation for {buttons.Count} buttons...");
        int linesCreated = 0;

        foreach (var buttonObj in buttons)
        {
            if (buttonObj == null)
            {
                Debug.LogWarning("Button in list is null!");
                continue;
            }

            UpgradeButton btn = buttonObj.GetComponent<UpgradeButton>();
            if (btn == null)
            {
                Debug.LogWarning($"{buttonObj.name} doesn't have UpgradeButton component!");
                continue;
            }

            if (btn.unlockRootObject == null)
            {
                Debug.Log($"{buttonObj.name} has no unlockRootObject - skipping line generation");
                continue;
            }

            if (btn != null && btn.unlockRootObject != null)
            {
                // Create line as sibling (same parent as buttons)
                string lineName = $"Line_{buttonObj.name}_to_{btn.unlockRootObject.name}";
                Transform lineTransform = buttonObj.transform.parent.Find(lineName);
                GameObject lineObj;
                
                if (lineTransform == null)
                {
                    lineObj = new GameObject(lineName);
                    lineObj.transform.SetParent(buttonObj.transform.parent, false); // Same parent as buttons
                    Debug.Log($"Created Line GameObject: {lineName}");
                }
                else
                {
                    lineObj = lineTransform.gameObject;
                    Debug.Log($"Found existing Line GameObject: {lineName}");
                }

                // Add RectTransform for UI
                RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                if (lineRect == null)
                {
                    lineRect = lineObj.AddComponent<RectTransform>();
                }

                // Add UI Image component
                UnityEngine.UI.Image lineImage = lineObj.GetComponent<UnityEngine.UI.Image>();
                if (lineImage == null)
                {
                    lineImage = lineObj.AddComponent<UnityEngine.UI.Image>();
                }

                // Set line behind buttons
                lineRect.SetAsFirstSibling(); // Render behind everything
                
                // Calculate line from button to unlock root
                RectTransform startRect = buttonObj.GetComponent<RectTransform>();
                RectTransform endRect = btn.unlockRootObject.GetComponent<RectTransform>();
                
                Vector2 start = startRect.anchoredPosition;
                Vector2 end = endRect.anchoredPosition;
                
                Vector2 direction = end - start;
                float distance = direction.magnitude;
                
                // Position line at midpoint between buttons
                lineRect.anchoredPosition = (start + end) / 2f;
                
                // Set line size (length = distance, width = lineWidth)
                lineRect.sizeDelta = new Vector2(distance, lineWidth);
                
                // Set pivot to center
                lineRect.pivot = new Vector2(0.5f, 0.5f);
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                
                // Rotate line to point from start to end
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                lineRect.localRotation = Quaternion.Euler(0, 0, angle);
                
                // Set line color
                Color lineColor = btn.nodeInstance.unlocked ? Color.green : Color.red;
                if (btn.nodeInstance.unlockRoot == null)
                    lineColor = Color.yellow;
                else if (btn.nodeInstance.unlockRoot != null && !btn.nodeInstance.unlocked)
                    lineColor = Color.red;
                else
                    lineColor = Color.green;
                    
                lineImage.color = lineColor;
                lineImage.raycastTarget = false; // Don't block button clicks

                // Assign line to button's appearance component
                UpgradeButtonApearance appearance = buttonObj.GetComponent<UpgradeButtonApearance>();
                if (appearance != null)
                {
                    appearance.line = lineObj;
                    Debug.Log($"✓ UI Line CREATED and ASSIGNED: {lineName} (distance: {distance})");
                }
                else
                {
                    Debug.Log($"✓ UI Line CREATED: {lineName} (distance: {distance}) - No UpgradeButtonApearance component");
                }
                linesCreated++;
            }
        }

        if (linesCreated == 0)
        {
            Debug.LogWarning("NO LINES CREATED! Make sure buttons have unlockRootObject assigned.");
        }
        else
        {
            Debug.Log($"<color=green>SUCCESS! Lines generated: {linesCreated}</color>");
        }
    }
}
