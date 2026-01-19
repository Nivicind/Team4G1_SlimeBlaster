using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cheat : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Currency Cheat Buttons")]
    public Button currencyPlusButton;
    public Button currencyMinusButton;

    [Header("Level Cheat Buttons")]
    public Button stagePlusButton;
    public Button stageMinusButton;

    [Header("Settings")]
    public int currencyAmount = 100;

    void Start()
    {
        // Setup button listeners
        if (currencyPlusButton != null)
            currencyPlusButton.onClick.AddListener(AddCurrency);

        if (currencyMinusButton != null)
            currencyMinusButton.onClick.AddListener(RemoveCurrency);

        if (stagePlusButton != null)
            stagePlusButton.onClick.AddListener(UnlockStage);

        if (stageMinusButton != null)
            stageMinusButton.onClick.AddListener(LockStage);
    }

    // ========== CURRENCY CHEATS ==========
    public void AddCurrency()
    {
        if (playerStats == null) return;

        playerStats.AddCurrency(EnumCurrency.blueBits, currencyAmount);
        playerStats.AddCurrency(EnumCurrency.pinkBits, currencyAmount);
        playerStats.AddCurrency(EnumCurrency.yellowBits, currencyAmount);
        playerStats.AddCurrency(EnumCurrency.greenBits, currencyAmount);
        playerStats.AddCurrency(EnumCurrency.xpBits, currencyAmount);

        Debug.Log($"[Cheat] Added {currencyAmount} to all currencies!");
    }

    public void RemoveCurrency()
    {
        if (playerStats == null) return;

        playerStats.SpendCurrency(EnumCurrency.blueBits, currencyAmount);
        playerStats.SpendCurrency(EnumCurrency.pinkBits, currencyAmount);
        playerStats.SpendCurrency(EnumCurrency.yellowBits, currencyAmount);
        playerStats.SpendCurrency(EnumCurrency.greenBits, currencyAmount);
        playerStats.SpendCurrency(EnumCurrency.xpBits, currencyAmount);

        Debug.Log($"[Cheat] Removed {currencyAmount} from all currencies!");
    }

    // ========== STAGE CHEATS ==========
    public void UnlockStage()
    {
        Debug.Log("[Cheat] UnlockStage button pressed!");
        
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("[Cheat] SaveSystem.Instance is NULL!");
            return;
        }

        SaveSystem.Instance.LoadStageData(out int currentStage, out int maxUnlocked);
        Debug.Log($"[Cheat] Before: currentStage={currentStage}, maxUnlocked={maxUnlocked}");
        maxUnlocked++;
        currentStage = maxUnlocked; // Also set current stage to the new unlocked stage
        SaveSystem.Instance.SaveStageData(currentStage, maxUnlocked);
        Debug.Log($"[Cheat] After: Unlocked stage! Max unlocked stage: {maxUnlocked}");
    }

    public void LockStage()
    {
        if (SaveSystem.Instance == null) return;

        SaveSystem.Instance.LoadStageData(out int currentStage, out int maxUnlocked);
        maxUnlocked = Mathf.Max(1, maxUnlocked - 1);
        currentStage = Mathf.Min(currentStage, maxUnlocked);
        SaveSystem.Instance.SaveStageData(currentStage, maxUnlocked);
        Debug.Log($"[Cheat] Locked stage! Max unlocked stage: {maxUnlocked}");
    }
}
