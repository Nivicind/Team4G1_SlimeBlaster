using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 💾 Handles saving and loading game data using JSON
/// Works on all platforms including Android
/// </summary>
public class SaveSystem : Singleton<SaveSystem>
{
    private string saveFilePath;
    private SaveData currentSaveData;

    protected override void Awake()
    {
        base.Awake();
        
        // Create Save folder in persistent data path
        string saveFolder = Path.Combine(Application.persistentDataPath, "Save");
        
        // Create folder if it doesn't exist
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        // Save file inside Save folder
        saveFilePath = Path.Combine(saveFolder, "savedata.json");
        Debug.Log($"💾 Save file path: {saveFilePath}");
    }

    /// <summary>
    /// 📥 Load save data from file
    /// </summary>
    public SaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"✅ Game loaded successfully! {currentSaveData.upgradeLevels.Count} upgrades found.");
                return currentSaveData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load game: {e.Message}");
                return CreateNewSave();
            }
        }
        else
        {
            Debug.Log("📁 No save file found, creating new save.");
            return CreateNewSave();
        }
    }

    /// <summary>
    /// 💾 Save current game data to file
    /// </summary>
    public void SaveGame(SaveData data)
    {
        try
        {
            currentSaveData = data;
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("💾 Game saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// 🔄 Update a specific upgrade level in save data
    /// </summary>
    public void UpdateUpgradeLevel(string upgradeName, int level)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        // Update or add the upgrade level
        bool found = false;
        for (int i = 0; i < currentSaveData.upgradeLevels.Count; i++)
        {
            if (currentSaveData.upgradeLevels[i].upgradeName == upgradeName)
            {
                currentSaveData.upgradeLevels[i].level = level;
                found = true;
                break;
            }
        }

        if (!found)
        {
            currentSaveData.upgradeLevels.Add(new UpgradeLevelData
            {
                upgradeName = upgradeName,
                level = level
            });
        }

        SaveGame(currentSaveData);
    }

    /// <summary>
    /// 🔍 Get upgrade level from save data
    /// </summary>
    public int GetUpgradeLevel(string upgradeName)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        foreach (var upgrade in currentSaveData.upgradeLevels)
        {
            if (upgrade.upgradeName == upgradeName)
            {
                return upgrade.level;
            }
        }

        return 0; // Default level if not found
    }

    /// <summary>
    /// 🆕 Create new save data
    /// </summary>
    private SaveData CreateNewSave()
    {
        currentSaveData = new SaveData
        {
            upgradeLevels = new List<UpgradeLevelData>(),
            currentStageSelected = 1,
            maxUnlockedStage = 1,
            playerLevel = 1
        };
        SaveGame(currentSaveData);
        return currentSaveData;
    }

    /// <summary>
    /// 🎮 Save stage data
    /// </summary>
    public void SaveStageData(int currentStage, int maxUnlocked)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.currentStageSelected = currentStage;
        currentSaveData.maxUnlockedStage = maxUnlocked;
        SaveGame(currentSaveData);
        Debug.Log($"💾 Stage data saved: Current={currentStage}, Max={maxUnlocked}");
    }

    /// <summary>
    /// 📥 Load stage data
    /// </summary>
    public void LoadStageData(out int currentStage, out int maxUnlocked)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentStage = currentSaveData.currentStageSelected;
        maxUnlocked = currentSaveData.maxUnlockedStage;
        Debug.Log($"📥 Stage data loaded: Current={currentStage}, Max={maxUnlocked}");
    }

    /// <summary>
    /// 👤 Save player level
    /// </summary>
    public void SavePlayerLevel(int level)
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        currentSaveData.playerLevel = level;
        SaveGame(currentSaveData);
        Debug.Log($"💾 Player level saved: {level}");
    }

    /// <summary>
    /// 📥 Get player level
    /// </summary>
    public int GetPlayerLevel()
    {
        if (currentSaveData == null)
        {
            currentSaveData = LoadGame();
        }

        return currentSaveData.playerLevel;
    }

    /// <summary>
    /// 🗑️ Delete save file (for testing)
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentSaveData = null;
            Debug.Log("🗑️ Save file deleted!");
        }
    }
}

/// <summary>
/// 📦 Main save data structure
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<UpgradeLevelData> upgradeLevels = new List<UpgradeLevelData>();
    
    // 🎮 Level/Stage data
    public int currentStageSelected = 1;
    public int maxUnlockedStage = 1;
    
    // 👤 Player level
    public int playerLevel = 1;
}

/// <summary>
/// 📊 Individual upgrade level data
/// </summary>
[System.Serializable]
public class UpgradeLevelData
{
    public string upgradeName;
    public int level;
}
