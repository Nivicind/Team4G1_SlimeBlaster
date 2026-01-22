using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Stage : Singleton<Stage>
{
    [Header("Stage Data")]
    [SerializeField] private int stage = 1;
    [SerializeField] private int unlockedStage = 1; // Maximum stage that can be accessed
    [SerializeField] private int maxStage = 99; // 🏆 Absolute maximum stage limit

    [Header("UI References")]
    public Button increaseButton;
    public Button decreaseButton;
    public List<TMP_Text> stageTexts = new List<TMP_Text>(); // All texts update together

    protected override void Awake()
    {
        base.Awake();
        
        // 📥 Load saved stage data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadStageData(out stage, out unlockedStage);
        }
        
        // Add listeners to buttons
        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(IncreaseStage);
        }

        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(DecreaseStage);
        }
        
        UpdateStageText();
        UpdateButtonVisibility();
    }

    public void IncreaseStage()
    {
        if (stage < unlockedStage)
        {
            stage++;
            UpdateStageText();
            UpdateButtonVisibility();
            
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            
            // 💾 Save stage data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveStageData(stage, unlockedStage);
            }
            
            Debug.Log($"Stage increased to: {stage}");
        }
        else
        {
            Debug.Log($"Cannot increase stage. Maximum unlocked stage is: {unlockedStage}");
        }
    }

    public void DecreaseStage()
    {
        if (stage > 1)
        {
            stage--;
            UpdateStageText();
            UpdateButtonVisibility();
            
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            
            // 💾 Save stage data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveStageData(stage, unlockedStage);
            }
            
            Debug.Log($"Stage decreased to: {stage}");
        }
        else
        {
            Debug.Log("Cannot decrease stage below 1");
        }
    }

    private void UpdateStageText()
    {
        foreach (var txt in stageTexts)
        {
            if (txt != null)
            {
                txt.text = $"Stage {stage}";
            }
        }
    }

    private void UpdateButtonVisibility()
    {
        // Show increase button only if stage can be increased
        if (increaseButton != null)
        {
            increaseButton.gameObject.SetActive(stage < unlockedStage);
        }

        // Show decrease button only if stage can be decreased
        if (decreaseButton != null)
        {
            decreaseButton.gameObject.SetActive(stage > 1);
        }
    }

    public int GetStage()
    {
        return stage;
    }
    
    /// <summary>
    /// 🎯 Set the current stage directly (for stage selection UI)
    /// </summary>
    public void SetStage(int newStage)
    {
        if (newStage >= 1 && newStage <= unlockedStage)
        {
            stage = newStage;
            UpdateStageText();
            UpdateButtonVisibility();
            
            // 💾 Save stage data
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveStageData(stage, unlockedStage);
            }
            
            Debug.Log($"Stage set to: {stage}");
        }
        else
        {
            Debug.LogWarning($"Cannot set stage to {newStage}. Must be between 1 and {unlockedStage}");
        }
    }

    public void UnlockStages(int amount)
    {
        unlockedStage += amount;
        unlockedStage = Mathf.Min(unlockedStage, maxStage); // 🏆 Clamp to max stage
        stage += amount;
        stage = Mathf.Min(stage, maxStage); // 🏆 Clamp to max stage
        UpdateStageText();
        UpdateButtonVisibility();
        
        // 💾 Save stage data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveStageData(stage, unlockedStage);
        }
        
        Debug.Log($"Unlocked {amount} new stages. Current stage: {stage}, Unlocked: {unlockedStage}, Max: {maxStage}");
    }
    
    /// <summary>
    /// 🏆 Get the absolute maximum stage limit
    /// </summary>
    public int GetMaxStage()
    {
        return maxStage;
    }
    
    /// <summary>
    /// 🔓 Get the highest unlocked stage
    /// </summary>
    public int GetUnlockedStage()
    {
        return unlockedStage;
    }
    
    /// <summary>
    /// 🏆 Check if player has reached the max stage
    /// </summary>
    public bool IsAtMaxStage()
    {
        return unlockedStage >= maxStage;
    }
}
