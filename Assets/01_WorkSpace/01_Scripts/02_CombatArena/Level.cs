using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level : Singleton<Level>
{
    [Header("Level Data")]
    [SerializeField] private int level = 1;
    [SerializeField] private int unlockedLevel = 1; // Maximum level that can be accessed

    [Header("UI References")]
    public Button increaseButton;
    public Button decreaseButton;
    public TMP_Text levelText;

    protected override void Awake()
    {
        base.Awake();
        
        // Add listeners to buttons
        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(IncreaseLevel);
        }

        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(DecreaseLevel);
        }
        
        UpdateLevelText();
    }

    public void IncreaseLevel()
    {
        if (level < unlockedLevel)
        {
            level++;
            UpdateLevelText();
            Debug.Log($"Level increased to: {level}");
        }
        else
        {
            Debug.Log($"Cannot increase level. Maximum unlocked level is: {unlockedLevel}");
        }
    }

    public void DecreaseLevel()
    {
        if (level > 1)
        {
            level--;
            UpdateLevelText();
            Debug.Log($"Level decreased to: {level}");
        }
        else
        {
            Debug.Log("Cannot decrease level below 1");
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    public int GetLevel()
    {
        return level;
    }

    public void UnlockLevels(int amount)
    {
        unlockedLevel += amount;
        level += amount;
        UpdateLevelText();
        Debug.Log($"Unlocked {amount} new levels. Current level: {level}, Max level: {unlockedLevel}");
    }
}
