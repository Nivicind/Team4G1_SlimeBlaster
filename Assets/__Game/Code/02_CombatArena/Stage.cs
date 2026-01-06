using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stage : Singleton<Stage>
{
    [Header("Stage Data")]
    [SerializeField] private int stage = 1;
    [SerializeField] private int unlockedStage = 1; // Maximum stage that can be accessed

    [Header("UI References")]
    public Button increaseButton;
    public Button decreaseButton;
    public TMP_Text stageText;

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
        if (stageText != null)
        {
            stageText.text = $"Stage: {stage}";
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

    public void UnlockStages(int amount)
    {
        unlockedStage += amount;
        stage += amount;
        UpdateStageText();
        UpdateButtonVisibility();
        
        // 💾 Save stage data
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveStageData(stage, unlockedStage);
        }
        
        Debug.Log($"Unlocked {amount} new stages. Current stage: {stage}, Max stage: {unlockedStage}");
    }
}
