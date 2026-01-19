using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveSystem))]
public class SaveSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveSystem saveSystem = (SaveSystem)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Save System Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("📥 Load Game"))
        {
            if (Application.isPlaying)
            {
                SaveData data = saveSystem.LoadGame();
                Debug.Log($"Loaded {data.upgradeLevels.Count} upgrades");
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to load game!");
            }
        }

        if (GUILayout.Button("🗑️ Delete Save File"))
        {
            if (EditorUtility.DisplayDialog("Delete Save", 
                "Are you sure you want to delete the save file?", 
                "Yes", "No"))
            {
                saveSystem.DeleteSave();
            }
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "💾 Save file location:\n" + 
            System.IO.Path.Combine(Application.persistentDataPath, "Save", "savedata.json"),
            MessageType.Info);
    }
}
