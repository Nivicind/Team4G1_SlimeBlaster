using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BreachTerminateManager))]
public class BreachTerminateManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        BreachTerminateManager manager = (BreachTerminateManager)target;

        GUILayout.Space(10);
        GUILayout.Label("Scene Preview Controls", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        // Show Upgrade Scene button
        if (GUILayout.Button("Show Upgrade", GUILayout.Height(30)))
        {
            ShowUpgradeScene(manager);
        }

        // Show Combat Scene button
        if (GUILayout.Button("Show Combat", GUILayout.Height(30)))
        {
            ShowCombatScene(manager);
        }

        // Show All Scenes button
        if (GUILayout.Button("Show All", GUILayout.Height(30)))
        {
            ShowAllScenes(manager);
        }

        GUILayout.EndHorizontal();
    }

    private void ShowUpgradeScene(BreachTerminateManager manager)
    {
        foreach (var scene in manager.upgradeScenes)
            if (scene != null) scene.SetActive(true);

        foreach (var scene in manager.combatScenes)
            if (scene != null) scene.SetActive(false);

        if (manager.breachButton != null) manager.breachButton.gameObject.SetActive(true);
        if (manager.terminateButton != null) manager.terminateButton.gameObject.SetActive(false);
    }

    private void ShowCombatScene(BreachTerminateManager manager)
    {
        foreach (var scene in manager.upgradeScenes)
            if (scene != null) scene.SetActive(false);

        foreach (var scene in manager.combatScenes)
            if (scene != null) scene.SetActive(true);

        if (manager.breachButton != null) manager.breachButton.gameObject.SetActive(false);
        if (manager.terminateButton != null) manager.terminateButton.gameObject.SetActive(true);
    }

    private void ShowAllScenes(BreachTerminateManager manager)
    {
        foreach (var scene in manager.upgradeScenes)
            if (scene != null) scene.SetActive(true);

        foreach (var scene in manager.combatScenes)
            if (scene != null) scene.SetActive(true);

        if (manager.breachButton != null) manager.breachButton.gameObject.SetActive(true);
        if (manager.terminateButton != null) manager.terminateButton.gameObject.SetActive(true);
    }
}
