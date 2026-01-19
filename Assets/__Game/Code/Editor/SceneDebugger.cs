#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SceneDebugger : EditorWindow
{
    private BreachTerminateManager manager;

    [MenuItem("Window/Scene Debugger")]
    public static void ShowWindow()
    {
        GetWindow<SceneDebugger>("Scene Debugger");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Switcher (Editor Only)", EditorStyles.boldLabel);

        manager = EditorGUILayout.ObjectField("Manager", manager, typeof(BreachTerminateManager), true) as BreachTerminateManager;

        if (manager == null)
        {
            EditorGUILayout.HelpBox("Assign a BreachTerminateManager to use the Scene Switcher.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Show Menu", GUILayout.Height(40)))
        {
            ShowMenu();
        }

        if (GUILayout.Button("Show Upgrade", GUILayout.Height(40)))
        {
            ShowUpgrade();
        }

        if (GUILayout.Button("Show Combat", GUILayout.Height(40)))
        {
            ShowCombat();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Use these buttons to quickly toggle scenes in the editor for testing.", MessageType.Info);
    }

    private void ShowMenu()
    {
        SetSceneActive(manager.menuScene, true);
        SetScenesActive(manager.upgradeScenes, false);
        SetScenesActive(manager.combatScenes, false);
        Debug.Log("Scene Debugger: Showing Menu");
    }

    private void ShowUpgrade()
    {
        SetSceneActive(manager.menuScene, false);
        SetScenesActive(manager.upgradeScenes, true);
        SetScenesActive(manager.combatScenes, false);
        Debug.Log("Scene Debugger: Showing Upgrade");
    }

    private void ShowCombat()
    {
        SetSceneActive(manager.menuScene, false);
        SetScenesActive(manager.upgradeScenes, false);
        SetScenesActive(manager.combatScenes, true);
        Debug.Log("Scene Debugger: Showing Combat");
    }

    private void SetSceneActive(GameObject scene, bool active)
    {
        if (scene != null)
            scene.SetActive(active);
    }

    private void SetScenesActive(System.Collections.Generic.List<GameObject> scenes, bool active)
    {
        foreach (var scene in scenes)
        {
            if (scene != null)
                scene.SetActive(active);
        }
    }
}
#endif
