using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 10;
    private int unlockedLevel = 1; // Highest unlocked level

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLevelData();
    }

    /// <summary>
    /// Get the current level player is on
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// Get the highest unlocked level
    /// </summary>
    public int GetUnlockedLevel()
    {
        return unlockedLevel;
    }

    /// <summary>
    /// Manually set the current level
    /// </summary>
    public void SetCurrentLevel(int level)
    {
        if (level >= 1 && level <= maxLevel)
        {
            currentLevel = level;
            Debug.Log($"Level Manager: Current level changed to {currentLevel}");
        }
    }

    /// <summary>
    /// Complete current level - unlocks next level but doesn't change current
    /// </summary>
    public void CompleteLevel()
    {
        if (currentLevel + 1 <= maxLevel && currentLevel + 1 > unlockedLevel)
        {
            unlockedLevel = currentLevel + 1;
            Debug.Log($"Level Manager: Level {unlockedLevel} unlocked! Current level stays at {currentLevel}");
        }
        SaveLevelData();
    }

    /// <summary>
    /// Check if boss should spawn - only spawns in current level
    /// </summary>
    public bool ShouldBossSpawn()
    {
        return currentLevel == unlockedLevel;
    }

    /// <summary>
    /// Check if a level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int level)
    {
        return level <= unlockedLevel;
    }

    private void SaveLevelData()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
        PlayerPrefs.Save();
    }

    private void LoadLevelData()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
    }
}
