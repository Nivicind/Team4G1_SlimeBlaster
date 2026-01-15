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

    [Header("Targets")]
    public GameObject menuScene;
    public List<GameObject> upgradeScenes;
    public List<GameObject> combatScenes;
    public GameObject gameOverPanel;

    [Header("Audio")]
    public BackgroundMusic backgroundMusic;

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

        // No music during menu
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
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
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

            // 🎵 Back to upgrade music
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
}
