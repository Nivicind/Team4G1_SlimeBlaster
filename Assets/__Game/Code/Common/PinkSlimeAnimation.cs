using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 🩷 Pink Slime Animation - Inherits from SlimeAnimation
/// Special death animation for Boss Pink Slime with victory sequence
/// Also supports Angry mode with aura and double reflection damage
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
    
    [Header("😠 Angry Animation")]
    public List<Sprite> angryBorder;  // Angry border sprites (loops like idle)
    [Range(0f, 60f)] public float angryDelay = 10f;  // Seconds before angry starts
    
    [Header("🔥 Aura Settings")]
    public SpriteRenderer auraSpriteRenderer;  // Aura sprite renderer
    public List<Sprite> auraSprites;  // Aura animation sprites
    [Range(0.01f, 1f)] public float auraFrameDuration = 0.1f;  // Aura's own frame speed (independent)
    [Range(1, 20)] public int auraFrameCycle = 3;  // How many full loops of aura
    [Range(0f, 60f)] public float auraCooldown = 10f;  // Seconds after angry ends before next angry
    
    [Header("💥 Angry Damage Settings")]
    public float angryReflectionMultiplier = 2f;  // Reflection damage multiplier when angry
    
    // Angry state
    private bool isAngry = false;
    
    // Angry timing
    private float angryDelayTimer = 0f;  // Counts up to angryDelay
    private bool isOnCooldown = false;  // True after angry ends
    private float cooldownTimer = 0f;  // Counts up to auraCooldown
    
    // Aura state (independent timer)
    private bool isAuraPlaying = false;
    private int auraFrameIndex = 0;
    private float auraFrameTimer = 0f;
    private int auraCompletedCycles = 0;
    
    // Cached reference (named differently to avoid conflict with Enemy.playerCombatArena)
    private PlayerCombatArena cachedPlayerCombatArena;
    
    /// <summary>
    /// ♻️ Reset death state - show all sprites again
    /// Called when boss is enabled/respawned
    /// </summary>
    public void ResetDeathState()
    {
        isPlayingDeathAnimation = false;
        
        // Reset angry state
        ResetAngry();
        
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
    
    protected override void Update()
    {
        // Skip if death animation playing
        if (isPlayingDeathAnimation) return;
        
        // Handle angry delay timer
        UpdateAngryDelay();
        
        // Always call base animation (liquid, hollowed, blink still play)
        base.Update();
        
        // If angry, override ONLY the border sprite after base update
        if (isAngry)
        {
            UpdateAngryAnimation();
        }
    }
    
    /// <summary>
    /// ⏱️ Handle angry delay timer - starts angry after delay
    /// </summary>
    private void UpdateAngryDelay()
    {
        // If already angry, skip
        if (isAngry) return;
        
        // If on cooldown, count cooldown first
        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= auraCooldown)
            {
                cooldownTimer = 0f;
                isOnCooldown = false;
            }
            return;
        }
        
        // Count up delay timer
        angryDelayTimer += Time.deltaTime;
        
        // Delay finished → start angry
        if (angryDelayTimer >= angryDelay)
        {
            angryDelayTimer = 0f;
            StartAngry();
        }
    }
    
    // ========== ANGRY ANIMATION ==========
    
    /// <summary>
    /// 😠 Start angry mode - show angry border and aura
    /// </summary>
    public void StartAngry()
    {
        if (isAngry) return;
        
        isAngry = true;
        
        // Start aura immediately
        StartAura();
    }
    
    /// <summary>
    /// 😌 Stop angry mode - return to normal, start cooldown
    /// </summary>
    public void StopAngry()
    {
        isAngry = false;
        
        // Start cooldown
        isOnCooldown = true;
        cooldownTimer = 0f;
        
        // Stop aura
        StopAura();
    }
    
    /// <summary>
    /// 🔄 Reset angry (for respawn)
    /// </summary>
    public void ResetAngry()
    {
        angryDelayTimer = 0f;
        isOnCooldown = false;
        cooldownTimer = 0f;
        StopAngry();
        isOnCooldown = false;  // Don't start cooldown on reset
    }
    
    /// <summary>
    /// 😠 Update angry border animation (syncs with base class idle frame)
    /// </summary>
    private void UpdateAngryAnimation()
    {
        if (angryBorder == null || angryBorder.Count == 0) return;
        
        // Sync with base class frame index (use modulo for different sprite counts)
        int syncedIndex = currentFrame % angryBorder.Count;
        
        // Apply angry sprite to border
        if (mainSpriteRenderer != null)
            mainSpriteRenderer.sprite = angryBorder[syncedIndex];
        
        // Update aura animation (independent timer)
        UpdateAuraAnimation();
    }
    
    // ========== AURA ANIMATION ==========
    
    /// <summary>
    /// 🔥 Start aura animation from first frame
    /// </summary>
    private void StartAura()
    {
        if (auraSpriteRenderer == null || auraSprites == null || auraSprites.Count == 0) return;
        
        isAuraPlaying = true;
        auraFrameIndex = 0;
        auraFrameTimer = 0f;
        auraCompletedCycles = 0;
        
        auraSpriteRenderer.enabled = true;
        auraSpriteRenderer.sprite = auraSprites[0];
    }
    
    /// <summary>
    /// 🔥 Stop aura animation
    /// </summary>
    private void StopAura()
    {
        isAuraPlaying = false;
        auraFrameIndex = 0;
        auraFrameTimer = 0f;
        auraCompletedCycles = 0;
        
        if (auraSpriteRenderer != null)
            auraSpriteRenderer.enabled = false;
    }
    
    /// <summary>
    /// 🔥 Update aura animation - independent timer, counts cycles to end angry
    /// </summary>
    private void UpdateAuraAnimation()
    {
        if (!isAuraPlaying || auraSprites == null || auraSprites.Count == 0) return;
        
        // Aura has its own independent timer
        auraFrameTimer += Time.deltaTime;
        
        if (auraFrameTimer >= auraFrameDuration)
        {
            auraFrameTimer = 0f;
            
            // Advance to next frame
            auraFrameIndex++;
            
            // Check if completed one cycle
            if (auraFrameIndex >= auraSprites.Count)
            {
                auraFrameIndex = 0;
                auraCompletedCycles++;
                
                // Check if all cycles completed → end angry
                if (auraCompletedCycles >= auraFrameCycle)
                {
                    StopAngry();
                    return;
                }
            }
        }
        
        if (auraSpriteRenderer != null)
            auraSpriteRenderer.sprite = auraSprites[auraFrameIndex];
    }
    
    // ========== PUBLIC GETTERS ==========
    
    /// <summary>
    /// 😠 Check if currently angry
    /// </summary>
    public bool IsAngry() => isAngry;
    
    /// <summary>
    /// 💥 Get the reflection damage multiplier (2x when angry, 1x otherwise)
    /// </summary>
    public float GetAngryReflectionMultiplier()
    {
        return isAngry ? angryReflectionMultiplier : 1f;
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
