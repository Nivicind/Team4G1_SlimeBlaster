using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 🩷 Pink Slime Animation - Inherits from SlimeAnimation
/// Special death animation for Boss Pink Slime with victory sequence
/// </summary>
public class PinkSlimeAnimation : SlimeAnimation
{
    [Header("Pink Slime Death Settings")]
    public List<Sprite> pinkSlimeDeath;  // Special death animation sprites
    public List<ParticleSystem> pinkSlimeDeathParticles;  // Death particles
    public SpriteRenderer shadowSpriteRenderer;  // Shadow sprite to hide during death
    
    [Header("Death Animation Timing")]
    [Range(0f, 5f)] public float pinkSlimeDeathFrameDuration = 0.15f;  // Time per frame
    [Range(0f, 10f)] public float waitTimeForShowWin = 2f;  // Time to wait before showing win
    
    [Header("Camera Shake on Death")]
    [Range(0f, 5f)] public float deathShakeDuration = 0.5f;
    [Range(0f, 5f)] public float deathShakeIntensity = 0.8f;
    
    // Cached reference (named differently to avoid conflict with Enemy.playerCombatArena)
    private PlayerCombatArena cachedPlayerCombatArena;
    
    /// <summary>
    /// ♻️ Reset death state - show all sprites again
    /// Called when boss is enabled/respawned
    /// </summary>
    public void ResetDeathState()
    {
        isPlayingDeathAnimation = false;
        
        // 👁️ Show all sprites again
        if (liquidSpriteRenderer != null)
            liquidSpriteRenderer.enabled = true;
        if (hollowedSpriteRenderer != null)
            hollowedSpriteRenderer.enabled = true;
        if (spriteMaskRenderer != null)
            spriteMaskRenderer.enabled = true;
        if (shadowSpriteRenderer != null)
            shadowSpriteRenderer.enabled = true;
    }
    
    /// <summary>
    /// 👁️ Hide non-border sprites during death
    /// </summary>
    private void HideOtherSprites()
    {
        if (liquidSpriteRenderer != null)
            liquidSpriteRenderer.enabled = false;
        if (hollowedSpriteRenderer != null)
            hollowedSpriteRenderer.enabled = false;
        if (spriteMaskRenderer != null)
            spriteMaskRenderer.enabled = false;
        if (shadowSpriteRenderer != null)
            shadowSpriteRenderer.enabled = false;
    }
    
    /// <summary>
    /// 🩷 Play Pink Slime Death Animation - Called when boss dies
    /// Stops player damage/attack, plays death anim, waits, then triggers ShowWin
    /// </summary>
    public void PlayPinkSlimeDeathAnimation(System.Action onComplete = null)
    {
        // 🔍 Find player combat arena
        if (cachedPlayerCombatArena == null)
            cachedPlayerCombatArena = FindObjectOfType<PlayerCombatArena>();
        
        // 🛑 Stop player from taking damage and attacking (by setting isDead flag)
        if (cachedPlayerCombatArena != null)
            cachedPlayerCombatArena.StopCombat();
        
        // 🎆 Play death particles
        PlayPinkSlimeDeathParticles();
        
        // 📷 Camera shake on death
        if (CameraShakeManager.Instance != null)
            CameraShakeManager.Instance.Shake(deathShakeDuration, deathShakeIntensity);
        
        // 🚩 Stop normal animation from playing
        isPlayingDeathAnimation = true;
        
        // 👁️ Hide liquid and hollowed sprites (only show border)
        HideOtherSprites();
        
        // 🎬 Start death animation sequence
        StartCoroutine(PinkSlimeDeathSequence(onComplete));
    }
    
    private IEnumerator PinkSlimeDeathSequence(System.Action onComplete)
    {
        // 🎞️ Play death sprite animation (no scale, just frames)
        if (pinkSlimeDeath != null && pinkSlimeDeath.Count > 0 && mainSpriteRenderer != null)
        {
            for (int i = 0; i < pinkSlimeDeath.Count; i++)
            {
                // Only update border sprite, not liquid or hollowed
                mainSpriteRenderer.sprite = pinkSlimeDeath[i];
                
                yield return new WaitForSeconds(pinkSlimeDeathFrameDuration);
            }
        }
        
        // ⏳ Wait for show win time (death anim and wait happen together)
        // Since death animation already played, wait remaining time
        float deathAnimDuration = pinkSlimeDeath != null ? pinkSlimeDeath.Count * pinkSlimeDeathFrameDuration : 0f;
        float remainingWait = Mathf.Max(0f, waitTimeForShowWin - deathAnimDuration);
        
        if (remainingWait > 0f)
            yield return new WaitForSeconds(remainingWait);
        
        // ✅ Trigger completion callback (ShowWin)
        onComplete?.Invoke();
    }
    
    private void PlayPinkSlimeDeathParticles()
    {
        if (pinkSlimeDeathParticles == null) return;
        
        foreach (var ps in pinkSlimeDeathParticles)
        {
            if (ps == null) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }
}
