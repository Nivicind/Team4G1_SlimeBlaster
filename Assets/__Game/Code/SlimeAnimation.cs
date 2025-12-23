using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public SpriteRenderer mainSpriteRenderer; // Shared by idle, blink, hurt
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

    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float blinkTimer = 0f;
    private float nextBlinkTime;
    
    private enum AnimState { Idle, Blink, Hurt }
    private AnimState currentState = AnimState.Idle;
    private bool isHurtPlaying = false;
    private float stateTimer = 0f;

    void Start()
    {
        nextBlinkTime = Random.Range(blinkMinInterval, blinkMaxInterval);
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
        }
    }
}
