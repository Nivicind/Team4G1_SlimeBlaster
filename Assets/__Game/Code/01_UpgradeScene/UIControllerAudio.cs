using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class AudioSliderSetting
{
    public string name;                    // For identification in inspector
    public MizuSlider slider;              // Custom Mizu Slider (0 to 1)
    public AudioSource audioSource;        // Audio source to control
    [Range(0f, 2f)] public float minVolume = 0f;   // Volume when slider = 0
    [Range(0f, 2f)] public float maxVolume = 1f;   // Volume when slider = 1
}

[System.Serializable]
public class VolumeSettings
{
    public float masterVolume = 1f;
    public List<VolumeEntry> volumes = new List<VolumeEntry>();
}

[System.Serializable]
public class VolumeEntry
{
    public string name;
    public float sliderValue;
}

/// <summary>
/// 🔊 UI Controller Audio - Controls audio volume via sliders
/// Maps slider value (0-1) to custom min/max volume range
/// </summary>
public class UIControllerAudio : MonoBehaviour
{
    private static UIControllerAudio instance;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "Save", "setting.json");

    [Header("Master Volume")]
    public MizuSlider masterSlider;                // Master volume slider
    [Range(0f, 1f)] public float masterMinVolume = 0f;
    [Range(0f, 1f)] public float masterMaxVolume = 1f;

    [Header("Audio Settings")]
    public List<AudioSliderSetting> audioSettings = new List<AudioSliderSetting>();

    [Header("Debug Display")]
    [TextArea(3, 10)]
    public string volumeDisplayText;  // Text area to show all volume values

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Load saved settings first
        LoadVolumeSettings();

        // Setup master slider
        if (masterSlider != null)
        {
            UpdateMasterVolume();
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        // Setup all sliders
        foreach (var setting in audioSettings)
        {
            if (setting.slider != null && setting.audioSource != null)
            {
                // Set initial volume based on slider value
                UpdateVolume(setting);

                // Add listener for slider changes
                var capturedSetting = setting; // Capture for closure
                setting.slider.onValueChanged.AddListener((value) => OnSliderChanged(capturedSetting));
            }
        }

        // Initial display update
        UpdateVolumeDisplay();
    }

    private void OnMasterSliderChanged(float value)
    {
        UpdateMasterVolume();
        UpdateVolumeDisplay();
    }

    private void OnSliderChanged(AudioSliderSetting setting)
    {
        UpdateVolume(setting);
        UpdateVolumeDisplay();
    }

    private void UpdateMasterVolume()
    {
        if (masterSlider == null) return;
        
        // Map slider (0-1) to master volume (min-max)
        float normalized = Mathf.InverseLerp(masterSlider.minValue, masterSlider.maxValue, masterSlider.Value);
        float volume = Mathf.Lerp(masterMinVolume, masterMaxVolume, normalized);
        AudioListener.volume = volume;
    }

    private void UpdateVolume(AudioSliderSetting setting)
    {
        if (setting.slider == null || setting.audioSource == null) return;

        // Map slider (0-1) to volume (min-max)
        float normalized = Mathf.InverseLerp(setting.slider.minValue, setting.slider.maxValue, setting.slider.Value);
        float volume = Mathf.Lerp(setting.minVolume, setting.maxVolume, normalized);
        setting.audioSource.volume = volume;
    }

    private void UpdateVolumeDisplay()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Master volume
        sb.AppendLine($"Master: {AudioListener.volume:F2}");
        
        // Individual volumes
        foreach (var setting in audioSettings)
        {
            if (setting.audioSource != null)
            {
                sb.AppendLine($"{setting.name}: {setting.audioSource.volume:F2}");
            }
        }

        volumeDisplayText = sb.ToString();
    }

    /// <summary>
    /// Set slider value by name
    /// </summary>
    public void SetSliderValue(string name, float value)
    {
        foreach (var setting in audioSettings)
        {
            if (setting.name == name && setting.slider != null)
            {
                setting.slider.SetValue(value);
                return;
            }
        }
    }

    /// <summary>
    /// Get current volume by name
    /// </summary>
    public float GetVolume(string name)
    {
        foreach (var setting in audioSettings)
        {
            if (setting.name == name && setting.audioSource != null)
            {
                return setting.audioSource.volume;
            }
        }
        return 0f;
    }

    private void OnDestroy()
    {
        // Remove master slider listener
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        }

        // Remove all listeners
        foreach (var setting in audioSettings)
        {
            if (setting.slider != null)
            {
                setting.slider.onValueChanged.RemoveAllListeners();
            }
        }
    }

    #region Save/Load Volume Settings

    /// <summary>
    /// Save current volume settings to JSON
    /// </summary>
    public static void SaveVolumeSettings()
    {
        if (instance == null) return;

        VolumeSettings settings = new VolumeSettings();
        
        // Save master volume
        if (instance.masterSlider != null)
        {
            settings.masterVolume = instance.masterSlider.Value;
        }

        // Save individual volumes
        foreach (var setting in instance.audioSettings)
        {
            if (setting.slider != null)
            {
                settings.volumes.Add(new VolumeEntry
                {
                    name = setting.name,
                    sliderValue = setting.slider.Value
                });
            }
        }

        string json = JsonUtility.ToJson(settings, true);
        
        // Ensure Save folder exists
        string saveFolder = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        File.WriteAllText(SavePath, json);
        Debug.Log($"💾 Volume settings saved to: {SavePath}");
    }

    /// <summary>
    /// Load volume settings from JSON
    /// </summary>
    private void LoadVolumeSettings()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("📂 No saved volume settings found, using defaults.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            VolumeSettings settings = JsonUtility.FromJson<VolumeSettings>(json);

            // Apply master volume
            if (masterSlider != null)
            {
                masterSlider.SetValueWithoutNotify(settings.masterVolume);
            }

            // Apply individual volumes
            foreach (var entry in settings.volumes)
            {
                foreach (var setting in audioSettings)
                {
                    if (setting.name == entry.name && setting.slider != null)
                    {
                        setting.slider.SetValueWithoutNotify(entry.sliderValue);
                        break;
                    }
                }
            }

            Debug.Log("📂 Volume settings loaded.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to load volume settings: {e.Message}");
        }
    }

    #endregion
}
