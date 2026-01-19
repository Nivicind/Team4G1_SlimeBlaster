using UnityEditor;
using UnityEngine;
using TMPro;

public class FontSetterWindow : EditorWindow
{
    private TMP_FontAsset selectedFont;

    [MenuItem("Window/Font Setter")]
    public static void ShowWindow()
    {
        GetWindow<FontSetterWindow>("Font Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Font for All Text", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Font selection field
        selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font:", selectedFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        // Apply button
        if (GUILayout.Button("Apply Font to All Text", GUILayout.Height(40)))
        {
            if (selectedFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a font first!", "OK");
                return;
            }

            ApplyFontToAllText();
        }
    }

    private void ApplyFontToAllText()
    {
        // Find all TextMeshProUGUI components including hidden and disabled ones
        TextMeshProUGUI[] allTextObjects = FindObjectsOfType<TextMeshProUGUI>(includeInactive: true);
        int count = 0;

        foreach (var textObject in allTextObjects)
        {
            textObject.font = selectedFont;
            EditorUtility.SetDirty(textObject);
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", $"Applied font to {count} text objects (including hidden/disabled)!", "OK");
    }
}
