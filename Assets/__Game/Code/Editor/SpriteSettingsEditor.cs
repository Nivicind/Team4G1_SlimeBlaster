using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSettingsEditor : EditorWindow
{
    [MenuItem("Tools/Batch Set Sprite Settings")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSettingsEditor>("Sprite Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Sprite Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Set Selected Folder Sprites", GUILayout.Height(40)))
        {
            ProcessSelectedFolder();
        }

        GUILayout.Space(10);
        GUILayout.Label("Settings Applied:", EditorStyles.label);
        GUILayout.Label("- Pixels Per Unit: 16", EditorStyles.miniLabel);
        GUILayout.Label("- Sprite Mode: Single", EditorStyles.miniLabel);
        GUILayout.Label("- Compression: None", EditorStyles.miniLabel);
        GUILayout.Label("- Filter Mode: Point (no filter)", EditorStyles.miniLabel);
    }

    private void ProcessSelectedFolder()
    {
        // Get selected folder
        Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a folder in the Project window.", "OK");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(selectedObjects[0]);
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Please select a folder, not a file.", "OK");
            return;
        }

        // Find all textures in folder and subfolders
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Sprites Found", "No sprites found in the selected folder.", "OK");
            return;
        }

        int processedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                importer.spritePixelsPerUnit = 16f;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point;

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                processedCount++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"Processed {processedCount} sprites in folder:\n{folderPath}", "OK");
    }
}
