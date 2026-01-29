using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class CurrencyDisplay
{
    public EnumCurrency currencyType;
    public GameObject currencyParent;  // Parent object to show/hide
    public GameObject iconObject;
    public TextMeshProUGUI amountText;
}

public class PlayerCombatUI : MonoBehaviour
{
    [SerializeField] private PlayerCombatArena playerCombat;
    [SerializeField] private PlayerStats playerStats;

    [Header("UI")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image expBarImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject gameOverAndWinPanel;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject loseText;
    [SerializeField] private GameObject collectionText;
    
    [Header("Game Over Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;

    [Header("Currency Display")]
    [SerializeField] private List<CurrencyDisplay> currencyDisplays;
    [SerializeField] private float countAnimationDuration = 1f;  // Duration for counting animation
    
    [Header("Win/Lose Text Pop Up Animation")]
    [SerializeField] private float winLosePopUpDuration = 0.4f;
    [SerializeField] private float winLoseStartScale = 0f;
    [SerializeField] private float winLoseOvershootScale = 1.2f;  // Scale overshoot before settling
    [SerializeField] private float winLoseFinalScale = 1f;
    [SerializeField] private Ease winLoseEase = Ease.OutBack;
    
    [Header("Currency Pop Up Animation")]
    [SerializeField] private float currencyPopUpDuration = 0.3f;
    [SerializeField] private float currencyStartScale = 0f;
    [SerializeField] private float currencyOvershootScale = 1.15f;  // Scale overshoot before settling
    [SerializeField] private float currencyFinalScale = 1f;
    [SerializeField] private Ease currencyEase = Ease.OutBack;
    
    [Header("Timing")]
    [SerializeField] private float delayAfterWinLose = 0.3f;  // Delay after win/lose text before currencies
    [SerializeField] private float delayBetweenCurrencies = 0.15f;  // Delay between each currency

    private void OnEnable()
    {
        // Hide UI panels
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(false);
        if (winText != null)
            winText.SetActive(false);
        if (loseText != null)
            loseText.SetActive(false);
        if (collectionText != null)
            collectionText.SetActive(false);
        
        // Hide all game over buttons initially
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);
        
        // Hide all currency displays (both parent and icon)
        HideAllCurrencyDisplays();
    }
    
    private void OnDisable()
    {
        // Hide all currency displays when disabled
        HideAllCurrencyDisplays();
    }
    
    
    
    /// <summary>
    /// 🔒 Hide all currency icons and parents
    /// </summary>
    private void HideAllCurrencyDisplays()
    {
        foreach (var display in currencyDisplays)
        {
            if (display.currencyParent != null)
                display.currencyParent.SetActive(false);
            if (display.iconObject != null)
                display.iconObject.SetActive(false);
            if (display.amountText != null)
                display.amountText.text = "";
        }
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
    }

    private void UpdateHealthBar()
    {
        if (healthBarImage != null && playerStats != null && playerCombat != null)
        {
            int baseHpValue = playerStats.GetStatValue(EnumStat.baseHp);
            int hpValue = playerStats.GetStatValue(EnumStat.hp);
            int maxHp = baseHpValue + hpValue;
            int currentHp = playerCombat.GetCurrentHp();
            if (maxHp > 0)
            {
                float fillAmount = (float)currentHp / maxHp;
                healthBarImage.fillAmount = Mathf.Clamp01(fillAmount);
            }

            // Update health text display
            if (healthText != null)
                healthText.text = $"{currentHp}/{maxHp}";
        }
    }

    private void UpdateExpBar()
    {
        if (expBarImage != null && playerStats != null)
        {
            int currentExp = playerStats.GetStatValue(EnumStat.exp);
            int currentLevel = playerStats.GetStatValue(EnumStat.level);
            int expRequired = currentLevel * 100; // Same formula as PlayerStats
            
            if (expRequired > 0)
            {
                float fillAmount = (float)currentExp / expRequired;
                expBarImage.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null && playerStats != null)
        {
            int currentLevel = playerStats.GetStatValue(EnumStat.level);
            levelText.text = $"Lv.{currentLevel}";
        }
    }

    public void ShowWin()
    {
        // Show game over panel first
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(true);
        if (loseText != null)
            loseText.SetActive(false);
        
        // Start sequence: win text pop up -> then currencies one by one
        StartCoroutine(ShowWinSequence());
    }

    public void ShowLose()
    {
        // Show game over panel first
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(true);
        if (winText != null)
            winText.SetActive(false);
        
        // Start sequence: lose text pop up -> then currencies one by one
        StartCoroutine(ShowLoseSequence());
    }
    
    /// <summary>
    /// 🏆 Sequence: Win text pop up -> collection text pop up -> currencies pop up one by one with count
    /// </summary>
    private IEnumerator ShowWinSequence()
    {
        // Pop up win text with smooth animation
        if (winText != null)
        {
            winText.SetActive(true);
            PlayWinLosePopUpAnimation(winText);
        }
        
        yield return new WaitForSeconds(winLosePopUpDuration + delayAfterWinLose);
        
        // Pop up collection text with smooth animation
        if (collectionText != null)
        {
            collectionText.SetActive(true);
            PlayWinLosePopUpAnimation(collectionText);
        }
        
        yield return new WaitForSeconds(winLosePopUpDuration + delayAfterWinLose);
        
        // Show currencies one by one
        yield return StartCoroutine(ShowCurrenciesSequence());
        
        // 🎯 Show Upgrade button first
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(true);
            PlayCurrencyPopUpAnimation(upgradeButton.gameObject);
        }
        
        yield return new WaitForSeconds(currencyPopUpDuration + delayBetweenCurrencies);
        
        // 🎯 Show Next Level button (WIN only)
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
            PlayCurrencyPopUpAnimation(nextLevelButton.gameObject);
        }
    }
    
    /// <summary>
    /// 💀 Sequence: Lose text pop up -> collection text pop up -> currencies pop up one by one with count
    /// </summary>
    private IEnumerator ShowLoseSequence()
    {
        // Pop up lose text with smooth animation
        if (loseText != null)
        {
            loseText.SetActive(true);
            PlayWinLosePopUpAnimation(loseText);
        }
        
        yield return new WaitForSeconds(winLosePopUpDuration + delayAfterWinLose);
        
        // Pop up collection text with smooth animation
        if (collectionText != null)
        {
            collectionText.SetActive(true);
            PlayWinLosePopUpAnimation(collectionText);
        }
        
        yield return new WaitForSeconds(winLosePopUpDuration + delayAfterWinLose);
        
        // Show currencies one by one
        yield return StartCoroutine(ShowCurrenciesSequence());
        
        // 🎯 Show Upgrade button first
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(true);
            PlayCurrencyPopUpAnimation(upgradeButton.gameObject);
        }
        
        yield return new WaitForSeconds(currencyPopUpDuration + delayBetweenCurrencies);
        
        // 🎯 Show Restart button (LOSE only)
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            PlayCurrencyPopUpAnimation(restartButton.gameObject);
        }
    }
    
    /// <summary>
    /// 💰 Show currencies one by one with pop up and count animation
    /// </summary>
    private IEnumerator ShowCurrenciesSequence()
    {
        if (playerCombat == null) yield break;

        Dictionary<EnumCurrency, int> collectedCurrency = playerCombat.GetCollectedCurrency();

        foreach (var display in currencyDisplays)
        {
            // Check if this currency type was collected and has value > 0
            if (collectedCurrency.ContainsKey(display.currencyType) && collectedCurrency[display.currencyType] > 0)
            {
                // Show and pop up currency parent
                if (display.currencyParent != null)
                {
                    display.currencyParent.SetActive(true);
                    PlayCurrencyPopUpAnimation(display.currencyParent);
                }
                if (display.iconObject != null)
                    display.iconObject.SetActive(true);
                
                // Set text to 0 first, then animate count
                if (display.amountText != null)
                {
                    display.amountText.text = "0";
                    int targetValue = collectedCurrency[display.currencyType];
                    
                    // Wait for pop up to finish, then start counting
                    yield return new WaitForSeconds(currencyPopUpDuration);
                    AnimateCountText(display.amountText, targetValue);
                    
                    // Wait for count animation to finish before next currency
                    yield return new WaitForSeconds(countAnimationDuration + delayBetweenCurrencies);
                }
            }
            else
            {
                // Hide if not collected or = 0
                if (display.currencyParent != null)
                    display.currencyParent.SetActive(false);
                if (display.iconObject != null)
                    display.iconObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 🏆 Play smooth pop up animation for win/lose text (with overshoot)
    /// </summary>
    private void PlayWinLosePopUpAnimation(GameObject obj)
    {
        obj.transform.localScale = Vector3.one * winLoseStartScale;
        
        // Smooth sequence: start -> overshoot -> settle to final
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.transform.DOScale(Vector3.one * winLoseOvershootScale, winLosePopUpDuration * 0.6f).SetEase(Ease.OutQuad));
        seq.Append(obj.transform.DOScale(Vector3.one * winLoseFinalScale, winLosePopUpDuration * 0.4f).SetEase(Ease.OutQuad));
    }
    
    /// <summary>
    /// 💰 Play smooth pop up animation for currency (with overshoot)
    /// </summary>
    private void PlayCurrencyPopUpAnimation(GameObject obj)
    {
        obj.transform.localScale = Vector3.one * currencyStartScale;
        
        // Smooth sequence: start -> overshoot -> settle to final
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.transform.DOScale(Vector3.one * currencyOvershootScale, currencyPopUpDuration * 0.6f).SetEase(Ease.OutQuad));
        seq.Append(obj.transform.DOScale(Vector3.one * currencyFinalScale, currencyPopUpDuration * 0.4f).SetEase(Ease.OutQuad));
    }
    
    /// <summary>
    /// 🔢 Animate text counting from 0 to target value
    /// </summary>
    private void AnimateCountText(TextMeshProUGUI text, int targetValue)
    {
        int currentValue = 0;
        DOTween.To(() => currentValue, x => 
        {
            currentValue = x;
            text.text = currentValue.ToString();
        }, targetValue, countAnimationDuration).SetEase(Ease.OutQuad);
    }
}
