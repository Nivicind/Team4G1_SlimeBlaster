using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(UpgradeButtonGenerator))]
public class UpgradeButtonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpgradeButtonGenerator generator = (UpgradeButtonGenerator)target;

        if (GUILayout.Button("Generate Upgrade Buttons"))
        {
            generator.GenerateButtons();
        }

        if (GUILayout.Button("Set Button Icons"))
        {
            generator.SetButtonIcons();
        }

        if (GUILayout.Button("Rename Buttons"))
        {
            generator.RenameButtons();
        }

        if (GUILayout.Button("Generate Lines"))
        {
            generator.GenerateLines();
        }
    }
}
