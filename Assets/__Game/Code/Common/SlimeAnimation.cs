using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Ensure DOTween is imported if using it

public class SlimeAnimation : MonoBehaviour
{
    [Header("Sprite Lists")]
    public List<Sprite> idle;
    public List<Sprite> blink;
    public List<Sprite> hurt;
    public List<Sprite> hollowed;
    public List<Sprite> inside;
    public List<Sprite> spriteMask;

    [Header("Sprite Renderers")]
    public SpriteRenderer mainSpriteRenderer; // Border - has glow effect
    public SpriteRenderer liquidSpriteRenderer;
    public SpriteRenderer hollowedSpriteRenderer;
    public SpriteMask spriteMaskRenderer;

    [Header("Sorting")]
    public UnityEngine.Rendering.SortingGroup sortingGroup;
    public Transform feet;

    [Header("Animation Settings")]
    [Range(0f, 5f)]
    public float frameChangeDuration = 0.1f; // Time between frame changes
    [Range(0f, 5f)]
    public float blinkDuration = 0.3f; // How long blink animation plays
    [Range(0f, 5f)]
    public float hurtDuration = 0.3f; // How long hurt animation plays
    [Range(0f, 5f)]
    public float blinkMinInterval = 0.5f; // Min time before next blink
    [Range(0f, 5f)]
    public float blinkMaxInterval = 1f; // Max time before next blink

    [Header("Glow Settings")]
    [SerializeField] private float glowAmount = 4f; // Glow intensity when hurt
    [SerializeField] private float glowDuration = 0.2f; // How long glow lasts

    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float blinkTimer = 0f;
    private float nextBlinkTime;

    private enum AnimState { Idle, Blink, Hurt }
    private AnimState currentState = AnimState.Idle;
    private bool isHurtPlaying = false;
    private float stateTimer = 0f;

    [Header("Death Animation Settings")]
    public float deathScaleMultiplier = 1.2f; // How much bigger it pops
    public float deathScaleDuration = 0f; // Time to scale up
    public float deathFlashDuration = 0.1f; // Time to flash white
    public Ease deathEase = Ease.OutBack; // E

    // 🔥 Glow effect using MaterialPropertyBlock (per-object, not shared)
    private MaterialPropertyBlock glowPropertyBlock;
    private MaterialPropertyBlock mainPropertyBlock;
    private Coroutine glowCoroutine;
    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");

    [Header("Hit / Death Particle Effects")]
    public List<ParticleSystem> hitParticles;
    private int currentParticleIndex = 0; // Track which particle to play next

    void Start()
    {
        nextBlinkTime = Random.Range(blinkMinInterval, blinkMaxInterval);
        InitializePropertyBlocks();
    }

    void OnEnable()
    {
        // 🔥 Re-initialize property blocks when re-enabled (for pooled objects)
        InitializePropertyBlocks();
    }

    // 🔥 Initialize MaterialPropertyBlocks for both renderers
    private void InitializePropertyBlocks()
    {
        if (liquidSpriteRenderer != null && glowPropertyBlock == null)
        {
            glowPropertyBlock = new MaterialPropertyBlock();
            SetGlow(0f);
        }

        if (mainSpriteRenderer != null && mainPropertyBlock == null)
        {
            mainPropertyBlock = new MaterialPropertyBlock();
            SetMainGlow(0f);
        }
    }

    void OnDisable()
    {
        // 🔥 Reset glow when disabled (only if liquidSpriteRenderer exists)
        if (liquidSpriteRenderer != null)
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }
            SetGlow(0f);
        }

        // 🔥 Reset main/border glow when disabled
        if (mainSpriteRenderer != null)
        {
            SetMainGlow(0f);
        }
    }

    void Update()
    {
        // Advance global frame counter
        frameTimer += Time.deltaTime;
        if (frameTimer >= frameChangeDuration)
        {
            frameTimer = 0f;
            currentFrame++;
        }

        // Update sorting order based on feet position
        UpdateSortingOrder();

        // Update all animations with synchronized frame
        UpdateSyncedAnimation(hollowed, hollowedSpriteRenderer);
        UpdateSyncedAnimation(inside, liquidSpriteRenderer);
        UpdateSyncedSpriteMask(spriteMask, spriteMaskRenderer);
        UpdateMainAnimation();
    }

    void UpdateSortingOrder()
    {
        if (sortingGroup == null || feet == null) return;

        // Lower Y position = higher sorting order (appears in front)
        sortingGroup.sortingOrder = Mathf.RoundToInt(-feet.position.y * 100);
    }

    void UpdateSyncedAnimation(List<Sprite> spriteList, SpriteRenderer renderer)
    {
        if (spriteList == null || spriteList.Count == 0 || renderer == null) return;

        // If only 1 sprite, just display it (no animation)
        if (spriteList.Count == 1)
        {
            renderer.sprite = spriteList[0];
            return;
        }

        int frame = currentFrame % spriteList.Count;
        renderer.sprite = spriteList[frame];
    }

    void UpdateSyncedSpriteMask(List<Sprite> spriteList, SpriteMask mask)
    {
        if (spriteList == null || spriteList.Count == 0 || mask == null) return;

        // If only 1 sprite, just display it (no animation)
        if (spriteList.Count == 1)
        {
            mask.sprite = spriteList[0];
            return;
        }

        int frame = currentFrame % spriteList.Count;
        mask.sprite = spriteList[frame];
    }

    void UpdateMainAnimation()
    {
        if (mainSpriteRenderer == null) return;

        // Handle blink trigger (only if not hurt)
        if (!isHurtPlaying)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= nextBlinkTime && currentState == AnimState.Idle)
            {
                currentState = AnimState.Blink;
                stateTimer = 0f;
                blinkTimer = 0f;
                nextBlinkTime = Random.Range(blinkMinInterval, blinkMaxInterval);
            }
        }

        // Update current animation state
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case AnimState.Idle:
                PlayAnimation(idle, frameChangeDuration);
                break;

            case AnimState.Blink:
                if (stateTimer >= blinkDuration)
                {
                    currentState = AnimState.Idle;
                    stateTimer = 0f;
                }
                else
                {
                    PlayAnimation(blink, frameChangeDuration);
                }
                break;

            case AnimState.Hurt:
                if (stateTimer >= hurtDuration)
                {
                    currentState = AnimState.Idle;
                    stateTimer = 0f;
                    isHurtPlaying = false;
                }
                else
                {
                    PlayAnimation(hurt, frameChangeDuration);
                }
                break;
        }
    }

    void PlayAnimation(List<Sprite> spriteList, float frameTime)
    {
        if (spriteList == null || spriteList.Count == 0) return;

        // If only 1 sprite, just display it (no animation)
        if (spriteList.Count == 1)
        {
            mainSpriteRenderer.sprite = spriteList[0];
            return;
        }

        // Use synchronized frame - all animations stay in sync
        int frame = currentFrame % spriteList.Count;
        mainSpriteRenderer.sprite = spriteList[frame];
    }

    public void Hurt()
    {
        if (!isHurtPlaying)
        {
            currentState = AnimState.Hurt;
            stateTimer = 0f;
            isHurtPlaying = true;
            PlayParticles();
            // 🔥 Trigger glow effect (if either renderer exists)
            if (liquidSpriteRenderer != null || mainSpriteRenderer != null)
            {
                if (glowCoroutine != null)
                    StopCoroutine(glowCoroutine);
                glowCoroutine = StartCoroutine(GlowRoutine());
            }
        }
    }

    // 🔥 Glow effect coroutine
    private IEnumerator GlowRoutine()
    {
        SetMainGlow(glowAmount); // Turn on main/border glow
        SetGlow(glowAmount); // Turn on liquid glow
        yield return new WaitForSeconds(glowDuration); // Wait 0.2s
        SetMainGlow(0f); // Turn off main/border glow
        SetGlow(0f); // Turn off liquid glow
        glowCoroutine = null;
    }

    // 🔥 Set glow using MaterialPropertyBlock (only affects THIS object)
    private void SetGlow(float amount)
    {
        if (liquidSpriteRenderer == null || glowPropertyBlock == null) return;

        // Get existing property block first to preserve other properties
        liquidSpriteRenderer.GetPropertyBlock(glowPropertyBlock);
        glowPropertyBlock.SetFloat(GlowAmountID, amount);
        liquidSpriteRenderer.SetPropertyBlock(glowPropertyBlock);
    }

    // 🔥 Set main/border glow using MaterialPropertyBlock (only affects THIS object)
    private void SetMainGlow(float amount)
    {
        if (mainSpriteRenderer == null || mainPropertyBlock == null) return;

        // Get existing property block first to preserve other properties
        mainSpriteRenderer.GetPropertyBlock(mainPropertyBlock);
        mainPropertyBlock.SetFloat(GlowAmountID, amount);
        mainSpriteRenderer.SetPropertyBlock(mainPropertyBlock);
    }

    public void PlayDeathAnimation(System.Action onComplete = null)
    {
        // Stop any ongoing animations if needed
        transform.DOKill();

        // Sequence: Scale up, flash white, then deactivate
        PlayParticles();
        Sequence deathSeq = DOTween.Sequence();
        deathSeq.Append(transform.DOScale(transform.localScale * deathScaleMultiplier, deathScaleDuration).SetEase(deathEase));
        deathSeq.Join(DOTween.To(() => mainSpriteRenderer.color, x => mainSpriteRenderer.color = x, Color.white, deathFlashDuration).SetLoops(2, LoopType.Yoyo)); // Flash white twice
        deathSeq.OnComplete(() =>
        {
            transform.localScale = Vector3.one;  // Reset scale
            onComplete?.Invoke(); // Call the callback if provided
        });
    }

    private void PlayParticles()
    {
        if (hitParticles == null || hitParticles.Count == 0) return;

        // Play current particle, then move to next for next call
        var ps = hitParticles[currentParticleIndex];
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        // Move to next particle (loop back to 0 if at end)
        currentParticleIndex = (currentParticleIndex + 1) % hitParticles.Count;
    }

}
