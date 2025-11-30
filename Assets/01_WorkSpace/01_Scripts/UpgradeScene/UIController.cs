using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Resource Panel")]
    public Button resourceButton;
    public GameObject resourcePanel;

    [Header("Upgrade Popup Panel")]
    public GameObject upgradePopupPanel;
    public GameObject popupContentPanel; // The inner panel with upgrade info
    public GameObject nowhereArea; // Click this area to close popup
    public TMP_Text nameText;
    public TMP_Text description1Text;
    public TMP_Text description2Text;
    public TMP_Text levelText;
    public TMP_Text moneyText;
    public Button confirmUpgradeButton;
    public PlayerStats playerStats;

    [Header("Resource Panel Animation")]
    public float resourcePanelMoveAmount = 195f;
    public float resourcePanelAnimationDuration = 0.5f;
    public Ease resourcePanelEaseType = Ease.OutQuad;

    [Header("Upgrade Popup Animation")]
    public float popupMoveAmount = 300f;
    public float popupAnimationDuration = 0.3f;
    public Ease popupEaseType = Ease.OutBack;

    private bool isPanelOpen = false;
    private Vector3 originalPosition;
    private Vector3 popupOriginalPosition;
    private NodeInstance currentNodeInstance;
    private UpgradeButton currentUpgradeButton;

    private void Start()
    {
        if (resourcePanel != null)
        {
            originalPosition = resourcePanel.transform.localPosition;
            // Start hidden - move panel to the right immediately
            resourcePanel.transform.localPosition = new Vector3(originalPosition.x + resourcePanelMoveAmount, originalPosition.y, originalPosition.z);
        }

        if (resourceButton != null)
        {
            resourceButton.onClick.AddListener(ToggleResourcePanel);
        }

        if (popupContentPanel != null)
        {
            popupOriginalPosition = popupContentPanel.transform.localPosition;
            // Start hidden - move panel up immediately
            popupContentPanel.transform.localPosition = new Vector3(popupOriginalPosition.x, popupOriginalPosition.y + popupMoveAmount, popupOriginalPosition.z);
        }

        if (confirmUpgradeButton != null)
        {
            confirmUpgradeButton.onClick.AddListener(ConfirmUpgrade);
        }
    }

    private void Update()
    {
        // Close popup when clicking anywhere except upgrade buttons
        if (Input.GetMouseButtonDown(0) && popupContentPanel != null)
        {
            // Check if popup is currently down (visible)
            bool isPopupDown = Mathf.Abs(popupContentPanel.transform.localPosition.y - popupOriginalPosition.y) < 1f;
            
            if (isPopupDown)
            {
                // Check if we didn't click on an upgrade button
                if (!IsClickOnUpgradeButton())
                {
                    CloseUpgradePopup();
                }
            }
        }
    }

    private bool IsClickOnUpgradeButton()
    {
        // Check if we hit any button or the popup content panel
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
        
        // First check if we clicked on the popup content panel itself
        if (popupContentPanel != null)
        {
            RectTransform popupRect = popupContentPanel.GetComponent<RectTransform>();
            if (popupRect != null && RectTransformUtility.RectangleContainsScreenPoint(popupRect, Input.mousePosition, uiCamera))
            {
                return true;
            }
        }
        
        // Then check all buttons
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            RectTransform btnRect = btn.GetComponent<RectTransform>();
            if (btnRect != null && RectTransformUtility.RectangleContainsScreenPoint(btnRect, Input.mousePosition, uiCamera))
            {
                return true;
            }
        }
        return false;
    }

    private void ToggleResourcePanel()
    {
        if (resourcePanel == null) return;

        if (isPanelOpen)
        {
            // Close panel - move to the right
            resourcePanel.transform.DOLocalMoveX(originalPosition.x + resourcePanelMoveAmount, resourcePanelAnimationDuration).SetEase(resourcePanelEaseType);
            isPanelOpen = false;
        }
        else
        {
            // Open panel - move to the left
            resourcePanel.transform.DOLocalMoveX(originalPosition.x, resourcePanelAnimationDuration).SetEase(resourcePanelEaseType);
            isPanelOpen = true;
        }
    }

    public void ShowUpgradePopup(NodeInstance nodeInstance, UpgradeButton upgradeButton)
    {
        if (popupContentPanel == null) return;

        // Check if popup is already at the down position
        bool isAlreadyDown = Mathf.Abs(popupContentPanel.transform.localPosition.y - popupOriginalPosition.y) < 1f;

        currentNodeInstance = nodeInstance;
        currentUpgradeButton = upgradeButton;

        // Fill in the popup information
        if (nameText != null)
        {
            nameText.text = nodeInstance.data.upgradeName;
        }

        if (description1Text != null)
        {
            description1Text.text = nodeInstance.data.description1;
        }

        if (description2Text != null)
        {
            description2Text.text = nodeInstance.data.description2;
        }

        if (levelText != null)
        {
            levelText.text = $"{nodeInstance.currentLevel} / {nodeInstance.data.maxLevel}";
        }

        if (moneyText != null && playerStats != null)
        {
            int cost = nodeInstance.GetCostForNextLevel();
            int currentMoney = playerStats.GetCurrency(nodeInstance.data.costUnit);
            moneyText.text = $"{currentMoney} / {cost}";
        }

        // If already down, just update the info (no animation)
        if (isAlreadyDown)
        {
            return;
        }

        // Move down to show
        popupContentPanel.transform.DOLocalMoveY(popupOriginalPosition.y, popupAnimationDuration).SetEase(popupEaseType);
    }

    public void CloseUpgradePopup()
    {
        if (popupContentPanel != null)
        {
            // Move up to hide
            popupContentPanel.transform.DOLocalMoveY(popupOriginalPosition.y + popupMoveAmount, popupAnimationDuration).SetEase(popupEaseType);
        }

        currentNodeInstance = null;
        currentUpgradeButton = null;
    }

    private void ConfirmUpgrade()
    {
        if (currentUpgradeButton != null)
        {
            currentUpgradeButton.PerformUpgrade();
            
            // Update the popup info after upgrade
            if (currentNodeInstance != null)
            {
                ShowUpgradePopup(currentNodeInstance, currentUpgradeButton);
            }
        }
    }

    private void OnDestroy()
    {
        if (resourceButton != null)
        {
            resourceButton.onClick.RemoveListener(ToggleResourcePanel);
        }

        if (confirmUpgradeButton != null)
        {
            confirmUpgradeButton.onClick.RemoveListener(ConfirmUpgrade);
        }
    }
}
