using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class CurrencyDisplay
{
    public EnumCurrency currencyType;
    public GameObject iconObject;
    public TextMeshProUGUI amountText;
}

public class PlayerCombatUI : MonoBehaviour
{
    [SerializeField] private PlayerCombatArena playerCombat;
    [SerializeField] private PlayerStats playerStats;

    [Header("UI")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image expBarImage;
    [SerializeField] private GameObject gameOverAndWinPanel;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject loseText;

    [Header("Currency Display")]
    [SerializeField] private List<CurrencyDisplay> currencyDisplays;

    private void OnEnable()
    {
        // Hide UI panels
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(false);
        if (winText != null)
            winText.SetActive(false);
        if (loseText != null)
            loseText.SetActive(false);
        
        // Hide all currency displays
        foreach (var display in currencyDisplays)
        {
            if (display.iconObject != null)
                display.iconObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateExpBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarImage != null && playerStats != null && playerCombat != null)
        {
            int maxHp = playerStats.GetStatValue(EnumStat.hp);
            int currentHp = playerCombat.GetCurrentHp();
            if (maxHp > 0)
            {
                float fillAmount = (float)currentHp / maxHp;
                healthBarImage.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }

    private void UpdateExpBar()
    {
        if (expBarImage != null && playerCombat != null)
        {
            int currentExp = playerCombat.GetCurrentExp();
            // Assuming max exp is 100 for now (you can change this logic)
            float fillAmount = (float)currentExp / 100f;
            expBarImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }

    public void ShowWin()
    {
        UpdateCurrencyDisplays();
        
        // Show game over panel with win text
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(true);
        if (winText != null)
            winText.SetActive(true);
        if (loseText != null)
            loseText.SetActive(false);
    }

    public void ShowLose()
    {
        UpdateCurrencyDisplays();
        
        // Show game over panel with lose text
        if (gameOverAndWinPanel != null)
            gameOverAndWinPanel.SetActive(true);
        if (loseText != null)
            loseText.SetActive(true);
        if (winText != null)
            winText.SetActive(false);
    }

    private void UpdateCurrencyDisplays()
    {
        if (playerCombat == null) return;

        Dictionary<EnumCurrency, int> collectedCurrency = playerCombat.GetCollectedCurrency();

        foreach (var display in currencyDisplays)
        {
            // Check if this currency type was collected
            if (collectedCurrency.ContainsKey(display.currencyType) && collectedCurrency[display.currencyType] > 0)
            {
                // Show icon and update text
                if (display.iconObject != null)
                    display.iconObject.SetActive(true);
                if (display.amountText != null)
                    display.amountText.text = collectedCurrency[display.currencyType].ToString();
            }
            else
            {
                // Hide if not collected or = 0
                if (display.iconObject != null)
                    display.iconObject.SetActive(false);
            }
        }
    }
}
