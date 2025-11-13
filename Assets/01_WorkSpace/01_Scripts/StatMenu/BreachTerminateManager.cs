using UnityEngine;
using UnityEngine.UI;

public class BreachTerminateManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button breachButton;
    public Button terminateButton;

    [Header("Targets")]
    public GameObject upgradeScene;
    public GameObject combatScene;

    private void Awake()
    {
        // Add listeners for both buttons
        breachButton.onClick.AddListener(OnBreachClicked);
        terminateButton.onClick.AddListener(OnTerminateClicked);
    }

    private void Start()
    {
        // ✅ Show Breach/Upgrade first
        if (upgradeScene != null) upgradeScene.SetActive(true);
        if (combatScene != null) combatScene.SetActive(false);

        // ✅ Show Breach button, hide Terminate button
        if (breachButton != null) breachButton.gameObject.SetActive(true);
        if (terminateButton != null) terminateButton.gameObject.SetActive(false);
    }

    private void OnBreachClicked()
    {
        // ✅ Switch to Combat Scene
        if (upgradeScene != null) upgradeScene.SetActive(false);
        if (combatScene != null) combatScene.SetActive(true);

        // ✅ Hide Breach button, show Terminate button
        if (breachButton != null) breachButton.gameObject.SetActive(false);
        if (terminateButton != null) terminateButton.gameObject.SetActive(true);
    }

    private void OnTerminateClicked()
    {
        // ✅ Switch back to Upgrade Scene
        if (combatScene != null) combatScene.SetActive(false);
        if (upgradeScene != null) upgradeScene.SetActive(true);

        // ✅ Hide Terminate button, show Breach button
        if (terminateButton != null) terminateButton.gameObject.SetActive(false);
        if (breachButton != null) breachButton.gameObject.SetActive(true);
    }
}
