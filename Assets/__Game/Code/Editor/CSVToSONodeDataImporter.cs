using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 📥 Editor tool to import CSV data into SONodeData ScriptableObjects
/// Menu: Tools > Import Upgrades from CSV
/// </summary>
public class CSVToSONodeDataImporter : EditorWindow
{
    private string csvFilePath = "D:\\UnityProjects\\SlimeBlaster\\Assets\\UpgradeData.csv";
    private string outputFolder = "Assets/__Game/Design/Config/Upgrades";

    [MenuItem("Tools/Import Upgrades from CSV")]
    public static void ShowWindow()
    {
        GetWindow<CSVToSONodeDataImporter>("CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("📥 CSV to SONodeData Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // CSV File Path
        GUILayout.Label("CSV File Path:");
        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField(csvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
            if (!string.IsNullOrEmpty(path))
                csvFilePath = path;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Output Folder
        GUILayout.Label("Output Folder:");
        outputFolder = EditorGUILayout.TextField(outputFolder);

        GUILayout.Space(20);

        if (GUILayout.Button("🚀 Import CSV", GUILayout.Height(40)))
        {
            ImportCSV();
        }

        GUILayout.Space(10);
        GUILayout.Label("CSV Format Expected:", EditorStyles.boldLabel);
        GUILayout.Label(",#,Icon,Stats,Name,Description,PerUpgrade,MaxThreshold,Cost,CostUnit,...");
        GUILayout.Label("Extra columns will be ignored.");
    }

    private void ImportCSV()
    {
        if (string.IsNullOrEmpty(csvFilePath) || !File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid CSV file!", "OK");
            return;
        }

        // Create output folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] folders = outputFolder.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath = nextPath;
            }
        }

        string[] lines = File.ReadAllLines(csvFilePath);
        int created = 0;
        int updated = 0;

        // Skip header rows (first 2 lines based on your CSV)
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] columns = ParseCSVLine(line);
            
            // CSV columns: 0=empty, 1=#, 2=Icon, 3=Stats, 4=Name, 5=Description, 6=PerUpgrade, 7=MaxThreshold, 8=Cost, 9=CostUnit, ...
            if (columns.Length < 10) continue;
            
            string idStr = columns[1].Trim();
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int id)) continue;

            string statName = columns[3].Trim();
            string upgradeName = columns[4].Trim();
            string description = columns[5].Trim();
            string perUpgradeStr = columns[6].Trim();
            string maxLevelStr = columns[7].Trim();
            string costFormula = columns[8].Trim();
            string costUnitStr = columns[9].Trim();

            if (string.IsNullOrEmpty(upgradeName)) continue;

            // Create or load existing asset
            string assetPath = $"{outputFolder}/{id}_{upgradeName}.asset";
            SONodeData nodeData = AssetDatabase.LoadAssetAtPath<SONodeData>(assetPath);
            
            bool isNew = nodeData == null;
            if (isNew)
            {
                nodeData = ScriptableObject.CreateInstance<SONodeData>();
            }

            // Set values
            nodeData.upgradeName = upgradeName;
            nodeData.description = description;
            
            // Parse stat enum
            nodeData.stat = ParseEnumStat(statName);
            
            // Parse per upgrade value
            if (int.TryParse(perUpgradeStr, out int perUpgrade))
                nodeData.perUpgradeValue = perUpgrade;
            
            // Parse max level
            if (int.TryParse(maxLevelStr, out int maxLevel))
                nodeData.maxLevel = maxLevel;
            
            // Cost formula
            nodeData.costFormula = costFormula;
            
            // Parse currency enum
            nodeData.costUnit = ParseEnumCurrency(costUnitStr);

            // Save asset
            if (isNew)
            {
                AssetDatabase.CreateAsset(nodeData, assetPath);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(nodeData);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ Import Complete", 
            $"Created: {created} new upgrades\nUpdated: {updated} existing upgrades", "OK");
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current);

        return result.ToArray();
    }

    private EnumStat ParseEnumStat(string statName)
    {
        statName = statName.ToLower().Trim().Replace(" ", "");
        
        // Map CSV stat names to EnumStat
        switch (statName)
        {
            case "damage": return EnumStat.damage;
            case "spawnrate": return EnumStat.spawnRatePercent;
            case "maxhealth": return EnumStat.hp;
            case "addtionaldamage": 
            case "additionaldamage": return EnumStat.additionalDamagePerEnemyInAreaPercent;
            case "attackaoe":
            case "attacksize": return EnumStat.attackSizeCount;
            case "armor": return EnumStat.armor;
            case "bossarmor": return EnumStat.bossArmor;
            case "attackspeed": 
            case "secondperattack": return EnumStat.secondPerAttack;
            case "damageagaintboss":
            case "damageagainstboss": return EnumStat.bossDamage;
            default:
                Debug.LogWarning($"Unknown stat: {statName}, defaulting to damage");
                return EnumStat.damage;
        }
    }

    private EnumCurrency ParseEnumCurrency(string currencyName)
    {
        currencyName = currencyName.ToLower().Trim().Replace(" ", "");
        
        switch (currencyName)
        {
            case "bluebits": return EnumCurrency.blueBits;
            case "pinkbits": return EnumCurrency.pinkBits;
            case "yellowbits": return EnumCurrency.yellowBits;
            case "greenbits": return EnumCurrency.greenBits;
            default:
                Debug.LogWarning($"Unknown currency: {currencyName}, defaulting to blueBits");
                return EnumCurrency.blueBits;
        }
    }
}
