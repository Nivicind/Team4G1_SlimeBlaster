using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    public SpriteRenderer mainSpriteRenderer; // Border / glow
    public SpriteRenderer liquidSpriteRenderer;
    public SpriteRenderer hollowedSpriteRenderer;
    public SpriteMask spriteMaskRenderer;

    [Header("Sorting")]
    public UnityEngine.Rendering.SortingGroup sortingGroup;
    public Transform feet;

    [Header("Animation Settings")]
    [Range(0f, 5f)] public float frameChangeDuration = 0.1f;
    [Range(0f, 5f)] public float blinkDuration = 0.3f;
    [Range(0f, 5f)] public float hurtDuration = 0.3f;
    [Range(0f, 5f)] public float blinkMinInterval = 0.5f;
    [Range(0f, 5f)] public float blinkMaxInterval = 1f;

    [Header("Glow Settings")]
    [SerializeField] private float glowAmount = 4f;
    [SerializeField] private float glowDuration = 0.2f;

    protected int currentFrame;  // Protected so PinkSlimeAnimation can sync angry border
    private float frameTimer;
    private float blinkTimer;
    private float nextBlinkTime;
    private float stateTimer;

    private enum AnimState { Idle, Blink, Hurt }
    private AnimState currentState = AnimState.Idle;
    private bool isHurtPlaying;
    protected bool isPlayingDeathAnimation = false;  // 🚩 Flag to stop normal animation during death

    [Header("Death Animation Settings")]
    public float deathScaleMultiplier = 1.2f;
    public float deathScaleDuration = 0f;
    public float deathFlashDuration = 0.1f;
    public Ease deathEase = Ease.OutBack;

    // 🔥 Glow (per-instance)
    private MaterialPropertyBlock liquidBlock;
    private MaterialPropertyBlock mainBlock;
    private Coroutine glowCoroutine;
    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");

    // ================= PARTICLE GROUP SYSTEM =================

    [Header("Hit / Death Particle Effect Groups")]
    public List<ParticleGroup> particleGroups;

    private int currentParticleGroupIndex = 0;

    [System.Serializable]
    public class ParticleGroup
    {
        public List<ParticleSystem> particles;
    }

    // =========================================================

    void Start()
    {
        nextBlinkTime = Random.Range(blinkMinInterval, blinkMaxInterval);
        InitializePropertyBlocks();
    }

    void OnEnable()
    {
        InitializePropertyBlocks();
        currentParticleGroupIndex = 0; // pool-safe reset
    }

    void OnDisable()
    {
        StopAllCoroutines();
        SetGlow(0f);
        SetMainGlow(0f);
    }

    protected virtual void Update()
    {
        // 🚩 Skip normal animation if death animation is playing
        if (isPlayingDeathAnimation) return;
        
        frameTimer += Time.deltaTime;
        if (frameTimer >= frameChangeDuration)
        {
            frameTimer = 0f;
            currentFrame++;
        }

        UpdateSortingOrder();

        UpdateSyncedAnimation(hollowed, hollowedSpriteRenderer);
        UpdateSyncedAnimation(inside, liquidSpriteRenderer);
        UpdateSyncedSpriteMask(spriteMask, spriteMaskRenderer);
        UpdateMainAnimation();
    }

    // ================= ANIMATION =================

    void UpdateSortingOrder()
    {
        if (sortingGroup == null || feet == null) return;
        sortingGroup.sortingOrder = Mathf.RoundToInt(-feet.position.y * 100);
    }

    void UpdateSyncedAnimation(List<Sprite> list, SpriteRenderer renderer)
    {
        if (list == null || list.Count == 0 || renderer == null) return;
        renderer.sprite = list.Count == 1 ? list[0] : list[currentFrame % list.Count];
    }

    void UpdateSyncedSpriteMask(List<Sprite> list, SpriteMask mask)
    {
        if (list == null || list.Count == 0 || mask == null) return;
        mask.sprite = list.Count == 1 ? list[0] : list[currentFrame % list.Count];
    }

    void UpdateMainAnimation()
    {
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

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case AnimState.Idle:
                PlayAnimation(idle);
                break;

            case AnimState.Blink:
                if (stateTimer >= blinkDuration)
                {
                    currentState = AnimState.Idle;
                    stateTimer = 0f;
                }
                else PlayAnimation(blink);
                break;

            case AnimState.Hurt:
                if (stateTimer >= hurtDuration)
                {
                    currentState = AnimState.Idle;
                    stateTimer = 0f;
                    isHurtPlaying = false;
                }
                else PlayAnimation(hurt);
                break;
        }
    }

    void PlayAnimation(List<Sprite> list)
    {
        if (list == null || list.Count == 0 || mainSpriteRenderer == null) return;
        mainSpriteRenderer.sprite = list.Count == 1 ? list[0] : list[currentFrame % list.Count];
    }

    // ================= HIT / DEATH =================

    public void Hurt()
    {
        if (isHurtPlaying) return;

        isHurtPlaying = true;
        currentState = AnimState.Hurt;
        stateTimer = 0f;

        PlayParticleGroup();
        TriggerGlow();
    }

    public void PlayDeathAnimation(System.Action onComplete = null)
    {
        transform.DOKill();

        PlayParticleGroup();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(transform.localScale * deathScaleMultiplier, deathScaleDuration).SetEase(deathEase));
        seq.Join(DOTween.To(() => mainSpriteRenderer.color, c => mainSpriteRenderer.color = c,
            Color.white, deathFlashDuration).SetLoops(2, LoopType.Yoyo));

        seq.OnComplete(() =>
        {
            transform.localScale = Vector3.one;
            onComplete?.Invoke();
        });
    }

    // ================= PARTICLES =================

    private void PlayParticleGroup()
    {
        if (particleGroups == null || particleGroups.Count == 0)
            return;

        if (currentParticleGroupIndex >= particleGroups.Count)
            currentParticleGroupIndex = 0; // loop

        ParticleGroup group = particleGroups[currentParticleGroupIndex];

        if (group?.particles != null)
        {
            foreach (var ps in group.particles)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }

        currentParticleGroupIndex++;
    }

    // ================= GLOW =================

    private void InitializePropertyBlocks()
    {
        if (liquidSpriteRenderer != null && liquidBlock == null)
            liquidBlock = new MaterialPropertyBlock();

        if (mainSpriteRenderer != null && mainBlock == null)
            mainBlock = new MaterialPropertyBlock();

        SetGlow(0f);
        SetMainGlow(0f);
    }

    private void TriggerGlow()
    {
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        SetGlow(glowAmount);
        SetMainGlow(glowAmount);
        yield return new WaitForSeconds(glowDuration);
        SetGlow(0f);
        SetMainGlow(0f);
        glowCoroutine = null;
    }

    private void SetGlow(float amount)
    {
        if (liquidSpriteRenderer == null || liquidBlock == null) return;
        liquidSpriteRenderer.GetPropertyBlock(liquidBlock);
        liquidBlock.SetFloat(GlowAmountID, amount);
        liquidSpriteRenderer.SetPropertyBlock(liquidBlock);
    }

    private void SetMainGlow(float amount)
    {
        if (mainSpriteRenderer == null || mainBlock == null) return;
        mainSpriteRenderer.GetPropertyBlock(mainBlock);
        mainBlock.SetFloat(GlowAmountID, amount);
        mainSpriteRenderer.SetPropertyBlock(mainBlock);
    }
}
