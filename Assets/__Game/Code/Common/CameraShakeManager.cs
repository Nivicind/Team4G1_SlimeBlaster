using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 📷 Camera Shake Manager - Shakes camera and optionally UI backgrounds
/// Inherits from Singleton for easy global access
/// Usage: CameraShakeManager.Instance.Shake(0.3f, 0.5f);
/// </summary>
public class CameraShakeManager : Singleton<CameraShakeManager>
{
    [Header("Shake Targets")]
    [SerializeField] private Transform cameraTransform;     // Main camera transform
    [SerializeField] private List<RectTransform> backgroundShake = new List<RectTransform>(); // Screen space overlay UI elements to shake
    
    [Header("Default Settings")]
    [SerializeField] private float defaultDuration = 0.15f;
    [SerializeField] private float defaultIntensity = 0.5f;  // 🔥 Increased for more visible shake
    
    [Header("UI Shake Settings")]
    [SerializeField] private bool shakeUI = true;           // Whether to shake UI backgrounds
    [SerializeField] private float uiShakeIntensityMultiply = 15f;  // UI shake intensity (pixels)
    
    private Vector3 originalCameraPos;
    private List<Vector3> originalBackgroundPositions = new List<Vector3>();
    
    // Camera shake state
    private float currentDuration = 0f;
    private float currentIntensity = 0f;
    private float maxDuration = 0f;
    private bool isShaking = false;
    
    // UI shake state (separate from camera)
    private float uiCurrentDuration = 0f;
    private float uiCurrentIntensity = 0f;
    private float uiMaxDuration = 0f;
    private bool isUIShaking = false;

    protected override void Awake()
    {
        base.Awake();
        
        // Auto-find camera if not assigned
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Store original positions
        if (cameraTransform != null)
            originalCameraPos = cameraTransform.localPosition;
        
        // Store original positions for all background UI elements
        StoreOriginalBackgroundPositions();
    }
    
    private void StoreOriginalBackgroundPositions()
    {
        originalBackgroundPositions.Clear();
        foreach (var bg in backgroundShake)
        {
            if (bg != null)
                originalBackgroundPositions.Add(bg.localPosition);
            else
                originalBackgroundPositions.Add(Vector3.zero);
        }
    }

    private void Update()
    {
        // Stop shake when game is paused (Time.timeScale = 0)
        if (Time.timeScale == 0f)
        {
            StopShake();
            return;
        }
        
        // Update camera shake
        if (isShaking)
        {
            if (currentDuration > 0)
            {
                float progress = currentDuration / maxDuration;
                float currentShake = currentIntensity * progress;
                Vector2 shakeOffset = Random.insideUnitCircle * currentShake;
                
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = originalCameraPos + (Vector3)shakeOffset;
                }
                
                currentDuration -= Time.deltaTime;
            }
            else
            {
                StopCameraShake();
            }
        }
        
        // Update UI shake (separate timing)
        if (isUIShaking && shakeUI)
        {
            if (uiCurrentDuration > 0)
            {
                float uiProgress = uiCurrentDuration / uiMaxDuration;
                float uiCurrentShake = uiCurrentIntensity * uiProgress;
                Vector2 uiShakeOffset = Random.insideUnitCircle * uiCurrentShake;
                
                for (int i = 0; i < backgroundShake.Count; i++)
                {
                    if (backgroundShake[i] != null && i < originalBackgroundPositions.Count)
                    {
                        backgroundShake[i].localPosition = originalBackgroundPositions[i] + (Vector3)uiShakeOffset;
                    }
                }
                
                uiCurrentDuration -= Time.deltaTime;
            }
            else
            {
                StopUIShake();
            }
        }
    }

    /// <summary>
    /// Start a camera shake with default values
    /// </summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultIntensity);
    }

    /// <summary>
    /// Start a camera shake with custom duration and intensity
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds)</param>
    /// <param name="intensity">How strong the shake is (units)</param>
    public void Shake(float duration, float intensity)
    {
        // Camera shake - just restart with new values
        if (!isShaking && cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }
        
        currentDuration = duration;
        currentIntensity = intensity;
        maxDuration = duration;
        isShaking = true;
        
        // UI shake (uses its own parameters)
        if (shakeUI)
        {
            if (!isUIShaking)
            {
                StoreOriginalBackgroundPositions();
            }
            
            uiCurrentDuration = duration;
            uiCurrentIntensity = intensity * uiShakeIntensityMultiply;
            uiMaxDuration = duration;
            isUIShaking = true;
        }
    }

    /// <summary>
    /// Stop camera shake only
    /// </summary>
    private void StopCameraShake()
    {
        isShaking = false;
        currentDuration = 0f;
        
        if (cameraTransform != null)
            cameraTransform.localPosition = originalCameraPos;
    }
    
    /// <summary>
    /// Stop UI shake only
    /// </summary>
    private void StopUIShake()
    {
        isUIShaking = false;
        uiCurrentDuration = 0f;
        
        for (int i = 0; i < backgroundShake.Count; i++)
        {
            if (backgroundShake[i] != null && i < originalBackgroundPositions.Count)
            {
                backgroundShake[i].localPosition = originalBackgroundPositions[i];
            }
        }
    }

    /// <summary>
    /// Immediately stop all shakes
    /// </summary>
    public void StopShake()
    {
        StopCameraShake();
        StopUIShake();
    }

    /// <summary>
    /// Quick shake presets for common use cases
    /// </summary>
    public void ShakeLight() => Shake(0.1f, 0.15f);
    public void ShakeMedium() => Shake(0.2f, 0.3f);
    public void ShakeHeavy() => Shake(0.35f, 0.6f);
    public void ShakeExplosion() => Shake(0.5f, 1f);
}
