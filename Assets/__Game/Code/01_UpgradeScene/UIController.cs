using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CurrencySprite
{
    public EnumCurrency currencyType;
    public TMP_SpriteAsset spriteAsset;
}

public class UIController : MonoBehaviour
{
    [Header("Resource Panel")]
    public Button resourceButton;
    public GameObject resourcePanel;

    [Header("⚙️ Settings Panel")]
    public GameObject settingPanel;
    public Button settingButton;
    public Button resumeButton;
    public Button terminateButton;

    [Header("Upgrade Popup Panel")]
    public GameObject upgradePopupPanel;
    public GameObject popupContentPanel; // The inner panel with upgrade info
    public TMP_Text nameText;
    public TMP_Text description1Text;
    public TMP_Text description2Text;
    public TMP_Text levelText;
    public TMP_Text moneyText;
    public Button confirmUpgradeButton;
    public PlayerStats playerStats;

    [Header("Currency Sprites")]
    public CurrencySprite[] currencySprites;

    [Header("Resource Panel Animation")]
    public float resourcePanelMoveAmount = 195f;
    public float resourcePanelAnimationDuration = 0.5f;
    public Ease resourcePanelEaseType = Ease.OutQuad;

    [Header("Upgrade Popup Animation")]
    public float popupMoveAmount = 300f;
    public float popupAnimationDuration = 0.3f;
    public Ease popupEaseType = Ease.OutBack;

    [Header("Button Scale Animation")]
    public float selectedButtonScale = 1.15f;
    public float buttonScaleDuration = 0.2f;
    public Ease buttonScaleEase = Ease.OutBack;

    [Header("Confirm Button Jiggle")]
    public float confirmJiggleRotation = 10f;
    public float confirmJiggleScale = 0.2f;
    public float confirmJiggleDuration = 0.3f;

    [Header("Not Enough Money Flash")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashCount = 1;
    public float shakeStrength = 10f;
    public int shakeVibrato = 20;
    public float shakeRandomness = 90f;

    [Header("Max Level Flash")]
    public Color maxLevelFlashColor = Color.yellow;

    [Header("Confirm Button Sprites")]
    public Sprite confirmSpriteEnoughMoney;   // Green - can afford
    public Sprite confirmSpriteNotEnough;     // Red - can't afford
    public Sprite confirmSpriteMaxLevel;      // Yellow - already max
    private Image confirmButtonImage;

    [Header("🍿 Breach Buttons")]
    public Button[] breachButtons;
    public float breachJumpHeight = 30f;
    public float breachJumpDuration = 0.15f;
    public int breachJumpCount = 3;
    public Ease breachJumpEase = Ease.OutQuad;

    private bool isPanelOpen = false;
    private Dictionary<Button, bool> breachButtonAnimating = new Dictionary<Button, bool>();
    private bool isSettingPanelOpen = false;
    private Vector3 originalPosition;
    private Vector3 popupOriginalPosition;
    private Vector3 settingPanelOriginalScale;
    private NodeInstance currentNodeInstance;
    private UpgradeButton currentUpgradeButton;
    private Color originalMoneyTextColor;
    private Vector3 originalMoneyTextPosition;

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

        // ⚙️ Setup setting panel
        if (settingButton != null)
        {
            settingButton.onClick.AddListener(ToggleSettingPanel);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(CloseSettingPanel);
        }

        if (terminateButton != null)
        {
            terminateButton.onClick.AddListener(CloseSettingPanel);
        }

        if (settingPanel != null)
        {
            settingPanelOriginalScale = settingPanel.transform.localScale;
            settingPanel.SetActive(false);
        }

        // Store original money text color and position
        if (moneyText != null)
        {
            originalMoneyTextColor = moneyText.color;
            originalMoneyTextPosition = moneyText.transform.localPosition;
        }

        // Cache confirm button image
        if (confirmUpgradeButton != null)
        {
            confirmButtonImage = confirmUpgradeButton.GetComponent<Image>();
        }

        // 🍿 Setup breach buttons
        if (breachButtons != null)
        {
            foreach (var btn in breachButtons)
            {
                if (btn != null)
                {
                    breachButtonAnimating[btn] = false;
                    btn.onClick.AddListener(() => OnBreachButtonClicked(btn));
                }
            }
        }
    }

    private void Update()
    {
        // Close popup when clicking anywhere except upgrade buttons
        if (PlayerInputHandler.Instance.IsInputDown() && popupContentPanel != null)
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
            if (popupRect != null && RectTransformUtility.RectangleContainsScreenPoint(popupRect, PlayerInputHandler.Instance.GetInputScreenPosition(), uiCamera))
            {
                return true;
            }
        }

        // Then check all buttons
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            RectTransform btnRect = btn.GetComponent<RectTransform>();
            if (btnRect != null && RectTransformUtility.RectangleContainsScreenPoint(btnRect, PlayerInputHandler.Instance.GetInputScreenPosition(), uiCamera))
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

        // Reset previous button scale
        if (currentUpgradeButton != null && currentUpgradeButton != upgradeButton)
        {
            currentUpgradeButton.transform.DOScale(Vector3.one, buttonScaleDuration).SetEase(Ease.OutQuad);
        }

        currentNodeInstance = nodeInstance;
        currentUpgradeButton = upgradeButton;

        // Make selected button bigger
        if (upgradeButton != null)
        {
            upgradeButton.transform.DOScale(Vector3.one * selectedButtonScale, buttonScaleDuration).SetEase(buttonScaleEase);
        }

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
            levelText.text = $"Lv. {nodeInstance.currentLevel} / {nodeInstance.data.maxLevel}";
        }

        if (moneyText != null && playerStats != null)
        {
            // Check if at max level
            if (nodeInstance.currentLevel >= nodeInstance.data.maxLevel)
            {
                moneyText.text = "Max";
                // Set max level sprite (yellow)
                if (confirmButtonImage != null && confirmSpriteMaxLevel != null)
                {
                    confirmButtonImage.sprite = confirmSpriteMaxLevel;
                }
            }
            else
            {
                int cost = nodeInstance.GetCostForNextLevel();
                int currentMoney = playerStats.GetCurrency(nodeInstance.data.costUnit);
                moneyText.text = $"<sprite index=0> {currentMoney} / {cost}";
                
                // Set sprite based on whether player can afford
                bool canAfford = playerStats.HasEnoughCurrency(nodeInstance.data.costUnit, cost);
                if (confirmButtonImage != null)
                {
                    if (canAfford && confirmSpriteEnoughMoney != null)
                    {
                        confirmButtonImage.sprite = confirmSpriteEnoughMoney;
                    }
                    else if (!canAfford && confirmSpriteNotEnough != null)
                    {
                        confirmButtonImage.sprite = confirmSpriteNotEnough;
                    }
                }
            }

            // Reset money text color and position
            moneyText.DOKill();
            moneyText.transform.DOKill();
            moneyText.color = originalMoneyTextColor;
            moneyText.transform.localPosition = originalMoneyTextPosition;
        }

        // Update currency icon sprite based on upgrade cost type
        UpdateCurrencyIcon(nodeInstance.data.costUnit);

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

        // Reset button scale
        if (currentUpgradeButton != null)
        {
            currentUpgradeButton.transform.DOScale(Vector3.one, buttonScaleDuration).SetEase(Ease.OutQuad);
        }

        currentNodeInstance = null;
        currentUpgradeButton = null;
    }

    private void ConfirmUpgrade()
    {
        if (currentUpgradeButton != null && currentNodeInstance != null)
        {
            // Check if at max level
            if (currentNodeInstance.currentLevel >= currentNodeInstance.data.maxLevel)
            {
                // Flash yellow once
                if (moneyText != null)
                {
                    moneyText.DOKill();
                    moneyText.color = originalMoneyTextColor;

                    moneyText.DOColor(maxLevelFlashColor, flashDuration / 2)
                        .SetLoops(2, LoopType.Yoyo)
                        .OnComplete(() => moneyText.color = originalMoneyTextColor);
                }
                return;
            }

            // Check if player has enough money
            int cost = currentNodeInstance.GetCostForNextLevel();
            bool hasEnoughMoney = playerStats != null && playerStats.HasEnoughCurrency(currentNodeInstance.data.costUnit, cost);

            if (!hasEnoughMoney)
            {
                // 🔊 Play not enough money sound
                GlobalSoundManager.PlaySound(SoundType.clickbutNotEnoughMoney);
                
                // Flash money text red and shake if not enough money
                if (moneyText != null)
                {
                    // Kill all tweens and reset
                    moneyText.DOKill();
                    moneyText.transform.DOKill();
                    moneyText.color = originalMoneyTextColor;
                    moneyText.transform.localPosition = originalMoneyTextPosition;
                    moneyText.transform.rotation = Quaternion.identity;

                    // Flash color
                    moneyText.DOColor(flashColor, flashDuration / (flashCount * 2))
                        .SetLoops(flashCount * 2, LoopType.Yoyo)
                        .OnComplete(() => moneyText.color = originalMoneyTextColor);

                    // Shake text and reset to original position when done
                    moneyText.transform.DOShakePosition(flashDuration, strength: shakeStrength, vibrato: shakeVibrato, randomness: shakeRandomness)
                        .OnComplete(() => moneyText.transform.localPosition = originalMoneyTextPosition);
                }
                return;
            }

            // 🔊 Play upgrade success sound
            GlobalSoundManager.PlaySound(SoundType.clickEnoughMoney);

            // Kill any existing tweens on these objects to prevent stacking
            if (confirmUpgradeButton != null)
            {
                confirmUpgradeButton.transform.DOKill();
                confirmUpgradeButton.transform.rotation = Quaternion.identity;
                confirmUpgradeButton.transform.localScale = Vector3.one;

                // Jiggle the confirm button
                confirmUpgradeButton.transform.DOPunchRotation(new Vector3(0, 0, confirmJiggleRotation), confirmJiggleDuration, 10, 1f);
                confirmUpgradeButton.transform.DOPunchScale(Vector3.one * confirmJiggleScale, confirmJiggleDuration, 5, 0.5f);
            }

            // Kill any existing tweens on upgrade button and reset
            currentUpgradeButton.transform.DOKill();
            currentUpgradeButton.transform.rotation = Quaternion.identity;

            // Restore to selected scale, then jiggle
            currentUpgradeButton.transform.localScale = Vector3.one * selectedButtonScale;
            currentUpgradeButton.transform.DOPunchRotation(new Vector3(0, 0, confirmJiggleRotation), confirmJiggleDuration, 10, 1f);
            currentUpgradeButton.transform.DOPunchScale(Vector3.one * selectedButtonScale * confirmJiggleScale, confirmJiggleDuration, 5, 0.5f);

            currentUpgradeButton.PerformUpgrade();

            // Update the popup info after upgrade
            if (currentNodeInstance != null)
            {
                ShowUpgradePopup(currentNodeInstance, currentUpgradeButton);
            }
        }
    }

    private void UpdateCurrencyIcon(EnumCurrency activeCurrency)
    {
        if (moneyText == null || currencySprites == null) return;

        // Find and set the TMP sprite asset for the active currency
        foreach (var currencySprite in currencySprites)
        {
            if (currencySprite.currencyType == activeCurrency && currencySprite.spriteAsset != null)
            {
                moneyText.spriteAsset = currencySprite.spriteAsset;
                return;
            }
        }
    }

    // ⚙️ Toggle setting panel (open/close)
    private void ToggleSettingPanel()
    {
        if (isSettingPanelOpen)
        {
            CloseSettingPanel();
        }
        else
        {
            OpenSettingPanel();
        }
    }

    // ⚙️ Open setting panel
    private void OpenSettingPanel()
    {
        if (settingPanel != null)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            
            settingPanel.SetActive(true);
            settingPanel.transform.localScale = Vector3.zero;
            settingPanel.transform.DOScale(settingPanelOriginalScale, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            Time.timeScale = 0f;
            isSettingPanelOpen = true;
        }
    }

    // ⚙️ Close setting panel
    private void CloseSettingPanel()
    {
        if (settingPanel != null)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            
            // 💾 Save volume settings
            UIControllerAudio.SaveVolumeSettings();
            
            settingPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                settingPanel.SetActive(false);
                Time.timeScale = 1f;
            });
            isSettingPanelOpen = false;
        }
    }

    // 🍿 Breach button popcorn animation
    private void OnBreachButtonClicked(Button btn)
    {
        if (btn == null) return;
        
        // Check if this specific button is already animating
        if (breachButtonAnimating.ContainsKey(btn) && breachButtonAnimating[btn]) return;

        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);

        breachButtonAnimating[btn] = true;

        // Kill any existing tweens and reset
        btn.transform.DOKill();
        Vector3 originalPos = btn.transform.localPosition;

        // Create popcorn jumping sequence
        Sequence popcornSequence = DOTween.Sequence();

        for (int i = 0; i < breachJumpCount; i++)
        {
            // Jump up
            popcornSequence.Append(btn.transform.DOLocalMoveY(originalPos.y + breachJumpHeight, breachJumpDuration).SetEase(breachJumpEase));
            // Fall down
            popcornSequence.Append(btn.transform.DOLocalMoveY(originalPos.y, breachJumpDuration).SetEase(Ease.InQuad));
        }

        // Reset position and allow pressing again when complete
        popcornSequence.OnComplete(() =>
        {
            btn.transform.localPosition = originalPos;
            breachButtonAnimating[btn] = false;
        });
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

        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(ToggleSettingPanel);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(CloseSettingPanel);
        }

        if (terminateButton != null)
        {
            terminateButton.onClick.RemoveListener(CloseSettingPanel);
        }

        if (breachButtons != null)
        {
            foreach (var btn in breachButtons)
            {
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
        }
    }
}
