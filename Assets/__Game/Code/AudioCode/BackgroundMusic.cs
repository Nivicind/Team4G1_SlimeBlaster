using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Music Lists")]
    public List<AudioClip> menuMusic;
    public List<AudioClip> upgradeMusic;
    public List<AudioClip> combatMusic;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float targetVolume = 1f;

    public float timeDecreaseVolume = 0.5f;
    public float timeIncreaseVolume = 0.5f;

    // Internal state
    private List<AudioClip> menuPool = new();
    private List<AudioClip> upgradePool = new();
    private List<AudioClip> combatPool = new();

    private Tween volumeTween;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        ResetPools();
    }

    private void ResetPools()
    {
        menuPool = new List<AudioClip>(menuMusic);
        upgradePool = new List<AudioClip>(upgradeMusic);
        combatPool = new List<AudioClip>(combatMusic);
    }

    // ---------------- PUBLIC API ----------------

    public void TransitionToMenuMusic()
    {
        PlayFromList(menuMusic, menuPool);
    }

    public void TransitionToUpgradeMusic()
    {
        PlayFromList(upgradeMusic, upgradePool);
    }

    public void TransitionToCombatMusic()
    {
        PlayFromList(combatMusic, combatPool);
    }

    // Generic background switch if you want later
    public void TransitionToBackgroundMusic(List<AudioClip> sourceList, List<AudioClip> pool)
    {
        PlayFromList(sourceList, pool);
    }

    // ---------------- CORE LOGIC ----------------

    private void PlayFromList(List<AudioClip> sourceList, List<AudioClip> pool)
    {
        if (sourceList == null || sourceList.Count == 0)
            return;

        if (pool.Count == 0)
            pool.AddRange(sourceList); // reset non-repeat pool

        AudioClip nextClip = pool[Random.Range(0, pool.Count)];
        pool.Remove(nextClip);

        FadeOutAndSwitch(nextClip);
    }

    private void FadeOutAndSwitch(AudioClip nextClip)
    {
        volumeTween?.Kill();

        // Capture current volume before fading (from slider setting)
        float currentVolume = audioSource.volume;

        volumeTween = audioSource
            .DOFade(0f, timeDecreaseVolume)
            .OnComplete(() =>
            {
                audioSource.clip = nextClip;
                audioSource.Play();

                // Fade back to the volume that was set by slider, not targetVolume
                volumeTween = audioSource
                    .DOFade(currentVolume, timeIncreaseVolume);
            });
    }
}
