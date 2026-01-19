#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class UIMenuDebugger : EditorWindow
{
    private BreachTerminateManager manager;

    [MenuItem("Window/Menu UI Debugger")]
    public static void ShowWindow()
    {
        GetWindow<UIMenuDebugger>("Menu UI Debugger");
    }

    private void OnGUI()
    {
        GUILayout.Label("Menu UI Switcher (Editor Only)", EditorStyles.boldLabel);

        manager = EditorGUILayout.ObjectField("Manager", manager, typeof(BreachTerminateManager), true) as BreachTerminateManager;

        if (manager == null)
        {
            EditorGUILayout.HelpBox("Assign a BreachTerminateManager to use the Menu UI Debugger.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Show Menu", GUILayout.Height(40)))
        {
            ShowMenu();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Use this button to quickly toggle the menu UI in the editor for testing.", MessageType.Info);
    }

    private void ShowMenu()
    {
        SetSceneActive(manager.menuScene, true);
        SetScenesActive(manager.upgradeScenes, false);
        SetScenesActive(manager.combatScenes, false);
        Debug.Log("Menu UI Debugger: Showing Menu");
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
