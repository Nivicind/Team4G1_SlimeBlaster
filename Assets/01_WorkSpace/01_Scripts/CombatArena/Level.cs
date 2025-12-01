using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level : Singleton<Level>
{
    [Header("Level Data")]
    [SerializeField] private int level = 1;

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
        level++;
        UpdateLevelText();
        Debug.Log($"Level increased to: {level}");
    }

    public void DecreaseLevel()
    {
        level--;
        UpdateLevelText();
        Debug.Log($"Level decreased to: {level}");
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
}
