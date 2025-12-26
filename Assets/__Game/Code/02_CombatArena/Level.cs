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
        
        // 📥 Load saved stage data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadStageData(out level, out unlockedLevel);
        }
        
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
        UpdateButtonVisibility();
    }

    public void IncreaseLevel()
    {
        if (level < unlockedLevel)
        {
            level++;
            UpdateLevelText();
            UpdateButtonVisibility();
            
            // 💾 Save stage data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveStageData(level, unlockedLevel);
            }
            
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
            UpdateButtonVisibility();
            
            // 💾 Save stage data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveStageData(level, unlockedLevel);
            }
            
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

    private void UpdateButtonVisibility()
    {
        // Show increase button only if level can be increased
        if (increaseButton != null)
        {
            increaseButton.gameObject.SetActive(level < unlockedLevel);
        }

        // Show decrease button only if level can be decreased
        if (decreaseButton != null)
        {
            decreaseButton.gameObject.SetActive(level > 1);
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
        UpdateButtonVisibility();
        
        // 💾 Save stage data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveStageData(level, unlockedLevel);
        }
        
        Debug.Log($"Unlocked {amount} new levels. Current level: {level}, Max level: {unlockedLevel}");
    }
}
