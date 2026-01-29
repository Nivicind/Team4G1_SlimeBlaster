using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreachTerminateManager : MonoBehaviour
{
    private Transition transition;

    [Header("Buttons")]
    public Button startButton;
    public Button breachButton;
    public Button terminateButton;

    [Header("GameOver Buttons")]
    public List<Button> toHomeButtons;
    public List<Button> restartButtons;
    public List<Button> nextLevelButtons;

    [Header("Targets")]
    public GameObject menuScene;
    public List<GameObject> upgradeScenes;
    public List<GameObject> combatScenes;
    public GameObject gameOverPanel;

    [Header("Audio")]
    public BackgroundMusic backgroundMusic;

    [Header("UI")]
    public UIController uiController;
    public UIStageControl uiStageControl; // 🎮 Stage selection UI

    private void Awake()
    {
        transition = GetComponent<Transition>();

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        breachButton.onClick.AddListener(OnBreachClicked);
        terminateButton.onClick.AddListener(OnTerminateClicked);

        foreach (var button in toHomeButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnToHomeClicked);
        }

        foreach (var button in restartButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnRestartClicked);
        }
        
        foreach (var button in nextLevelButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnNextLevelClicked);
        }
    }

    private void Start()
    {
        // Start in Menu state
        if (menuScene != null)
            menuScene.SetActive(true);

        foreach (var scene in upgradeScenes)
            if (scene != null) scene.SetActive(false);

        foreach (var scene in combatScenes)
            if (scene != null) scene.SetActive(false);

        breachButton.gameObject.SetActive(false);
        terminateButton.gameObject.SetActive(false);

        // 🎵 Menu music
        if (backgroundMusic != null)
            backgroundMusic.TransitionToMenuMusic();
    }

    private void OnStartClicked()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        transition.PlayTransition(() =>
        {
            // Hide menu
            if (menuScene != null)
                menuScene.SetActive(false);

            // Show upgrade scene
            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(true);

            breachButton.gameObject.SetActive(true);
            terminateButton.gameObject.SetActive(false);

            // 🎵 Upgrade music
            if (backgroundMusic != null)
                backgroundMusic.TransitionToUpgradeMusic();
        });
    }

    private void OnBreachClicked()
    {
        // � Check if current stage is playable (not locked)
        if (uiStageControl != null && !uiStageControl.IsCurrentStagePlayable())
        {
            // Stage is locked - don't proceed, UIStageControl will show error effect
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            return;
        }
        
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);

        // 🎯 Reset UI panels to hidden state before leaving upgrade scene
        if (uiController != null)
            uiController.ResetAllPanelsToHiddenState();
        
        // 🎮 Set the selected stage from UIStageControl (only unlocked stages)
        if (uiStageControl != null && Stage.Instance != null)
        {
            int selectedStage = uiStageControl.GetSelectedStage();
            Stage.Instance.SetStage(selectedStage);
        }
        
        transition.PlayTransition(() =>
        {
            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(false);

            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(true);

            breachButton.gameObject.SetActive(false);
            terminateButton.gameObject.SetActive(true);

            // 🎵 Combat music
            if (backgroundMusic != null)
                backgroundMusic.TransitionToCombatMusic();
        });
    }

    private void OnTerminateClicked()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        transition.PlayTransition(() =>
        {
            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(false);

            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(true);

            terminateButton.gameObject.SetActive(false);
            breachButton.gameObject.SetActive(true);
            if (backgroundMusic != null)
                backgroundMusic.TransitionToUpgradeMusic();
        });
    }

    private void OnToHomeClicked()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        transition.PlayTransition(() =>
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(false);

            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(true);

            terminateButton.gameObject.SetActive(false);
            breachButton.gameObject.SetActive(true);

            // 🎵 Upgrade music
            if (backgroundMusic != null)
                backgroundMusic.TransitionToUpgradeMusic();
        });
    }

    private void OnRestartClicked()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        TriggerRestart();
    }
    
    /// <summary>
    /// 🔄 Public method to trigger restart (used by Next Level button)
    /// </summary>
    public void TriggerRestart()
    {
        transition.PlayTransition(() =>
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            foreach (var scene in combatScenes)
            {
                if (scene != null)
                {
                    scene.SetActive(false);
                    scene.SetActive(true);
                }
            }

            // 🎵 Combat music again
            if (backgroundMusic != null)
                backgroundMusic.TransitionToCombatMusic();
        });
    }
    
    /// <summary>
    /// ➡️ Called when Next Level button is clicked - advances to next unlocked stage and restarts
    /// </summary>
    private void OnNextLevelClicked()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        // Advance to next stage (which was just unlocked from winning)
        if (Stage.Instance != null)
        {
            int currentStage = Stage.Instance.GetStage();
            int unlockedStage = Stage.Instance.GetUnlockedStage();
            
            // Go to next stage if available and unlocked
            int nextStage = currentStage + 1;
            if (nextStage <= unlockedStage)
            {
                Stage.Instance.SetStage(nextStage);
                MizuLog.General($"[NextLevel] Moving to stage {nextStage}");
            }
        }
        
        // Restart with new stage
        TriggerRestart();
    }
}
