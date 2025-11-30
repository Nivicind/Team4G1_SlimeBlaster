using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("Node Data")]
    public NodeInstance nodeInstance;

    [Header("Unlock Root")]
    public GameObject unlockRootObject; // Drag the GameObject in inspector
    private UpgradeButton unlockRootButton;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("UI Controller")]
    public UIController uiController;

    [Header("Runtime Line Renderer")]
    public bool drawLineInGame = true;
    public float lineWidth = 0.1f;

    private Button button;
    private Image buttonImage;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        buttonImage = GetComponent<Image>();

        if (unlockRootObject != null)
        {
            unlockRootButton = unlockRootObject.GetComponent<UpgradeButton>();
            if (unlockRootButton != null)
            {
                nodeInstance.unlockRoot = unlockRootButton.nodeInstance;
            }
        }

        UpdateUnlockStatus();
    }

    private void OnClick()
    {
        if (!nodeInstance.CanUpgrade()) return;

        // Show the upgrade popup instead of directly upgrading
        if (uiController != null)
        {
            uiController.ShowUpgradePopup(nodeInstance, this);
        }
        else
        {
            // Fallback: upgrade directly if no UI controller
            PerformUpgrade();
        }
    }

    public void PerformUpgrade()
    {
        if (!nodeInstance.CanUpgrade()) return;

        int cost = nodeInstance.GetCostForNextLevel();

        if (playerStats.HasEnoughCurrency(nodeInstance.data.costUnit, cost))
        {
            playerStats.SpendCurrency(nodeInstance.data.costUnit, cost);
            nodeInstance.Upgrade();
            ApplyUpgradeEffect();
            Debug.Log($"{nodeInstance.data.upgradeName} upgraded to level {nodeInstance.currentLevel}");

            // Update children
            UpdateDependentUnlocks();
        }
        else
        {
            Debug.Log("Not enough resources!");
        }
    }

    private void ApplyUpgradeEffect()
    {
        playerStats.AddStat(nodeInstance.data.stat, nodeInstance.data.perUpgradeValue);
    }

    public void UpdateUnlockStatus()
    {
        if (nodeInstance.unlockRoot == null)
        {
            // Root node is always unlocked
            nodeInstance.unlocked = true;
        }
        else
        {
            // Child node unlocked if root's level >= required level
            nodeInstance.unlocked = nodeInstance.unlockRoot.currentLevel >= nodeInstance.unlockRequirementLevel;
        }

        // Enable/disable button interactability based on unlock status
        if (button != null)
        {
            button.interactable = nodeInstance.unlocked;
        }

        // Update children if unlocked
        UpdateDependentUnlocks();
    }

    public void UpdateDependentUnlocks()
    {
        UpgradeButton[] allButtons = FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.nodeInstance.unlockRoot == nodeInstance)
            {
                btn.UpdateUnlockStatus();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (unlockRootObject != null)
        {
            Color startColor = nodeInstance.unlockRoot == null ? Color.yellow : Color.cyan; // Root = yellow, child = cyan
            Color endColor = nodeInstance.unlocked ? Color.green : Color.red;
            DrawGizmoLine(transform.position, unlockRootObject.transform.position, startColor, endColor);
        }
    }

    private void DrawGizmoLine(Vector3 start, Vector3 end, Color startColor, Color endColor, int segments = 10)
    {
        Vector3 prev = start;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = Vector3.Lerp(start, end, t);
            Gizmos.color = Color.Lerp(startColor, endColor, t);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }
    }
#endif
}
