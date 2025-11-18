using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreachTerminateManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button breachButton;
    public Button terminateButton;

    [Header("Targets")]
    public List<GameObject> upgradeScenes;
    public List<GameObject> combatScenes;

    private void Awake()
    {
        // Add listeners for both buttons
        breachButton.onClick.AddListener(OnBreachClicked);
        terminateButton.onClick.AddListener(OnTerminateClicked);
    }

    private void Start()
    {
        // ✅ Show Breach/Upgrade first
        foreach (var scene in upgradeScenes)
            if (scene != null) scene.SetActive(true);
        
        foreach (var scene in combatScenes)
            if (scene != null) scene.SetActive(false);

        // ✅ Show Breach button, hide Terminate button
        if (breachButton != null) breachButton.gameObject.SetActive(true);
        if (terminateButton != null) terminateButton.gameObject.SetActive(false);
    }

    private void OnBreachClicked()
    {
        // ✅ Switch to Combat Scene
        foreach (var scene in upgradeScenes)
            if (scene != null) scene.SetActive(false);
        
        foreach (var scene in combatScenes)
            if (scene != null) scene.SetActive(true);

        // ✅ Hide Breach button, show Terminate button
        if (breachButton != null) breachButton.gameObject.SetActive(false);
        if (terminateButton != null) terminateButton.gameObject.SetActive(true);
    }

    private void OnTerminateClicked()
    {
        // ✅ Switch back to Upgrade Scene
        foreach (var scene in combatScenes)
            if (scene != null) scene.SetActive(false);
        
        foreach (var scene in upgradeScenes)
            if (scene != null) scene.SetActive(true);

        // ✅ Hide Terminate button, show Breach button
        if (terminateButton != null) terminateButton.gameObject.SetActive(false);
        if (breachButton != null) breachButton.gameObject.SetActive(true);
    }
}
