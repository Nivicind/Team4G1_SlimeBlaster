using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIStageControl : MonoBehaviour
{
    [Header("🎮 Stage Setup")]
    public RectTransform stagePrefab; // Prefab with Image + TMP_Text child
    public RectTransform stageContainer; // Parent that holds all stage images, will slide
    
    [Header("📱 Swipe Region")]
    public RectTransform swipeRegion; // Only swipes in this region will work
    
    [Header("⬅️➡️ Arrow Buttons (Optional)")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    
    [Header("🎮 Play Button")]
    public Button playButton;
    public Image playButtonImage;
    public Color normalColor = Color.white;
    public Color lockedColor = Color.red;
    public float errorShakeDuration = 0.3f;
    public float errorShakeStrength = 10f;
    
    [Header("🎬 Slide Animation")]
    public float slideDuration = 0.3f;
    public Ease slideEase = Ease.OutQuad;
    public float swipeThreshold = 50f; // Minimum swipe distance to change stage
    
    [Header("🎬 Scale Animation")]
    public float selectedScale = 1.2f;
    public float unselectedScale = 1f;
    public float scaleDuration = 0.2f;
    public Ease scaleEase = Ease.OutBack;
    
    [Header("🔒 Locked Stage Alpha")]
    public float lockedAlpha = 0.5f;
    public float unlockedAlpha = 1f;
    
    // Runtime generated stage images
    private List<RectTransform> stageImages = new List<RectTransform>();
    private List<CanvasGroup> stageCanvasGroups = new List<CanvasGroup>();
    private float stageWidth;  // Width of each stage element
    private float stageSpacing;  // Spacing between stages (from layout group)
    
    private int currentIndex = 0;
    private int selectedStage = 1; // The stage number player selected (1-based)
    private bool isAnimating = false;
    private bool isCurrentStageUnlocked = true; // Track if current viewed stage is unlocked
    private int cachedUnlockedStage = 1; // Track unlocked stage count to detect changes
    private float containerStartX;
    
    // Swipe tracking
    private bool isDragging = false;
    private float dragStartX;
    private float containerDragStartX;
    
    private Camera uiCamera;
    private bool isInitialized = false;
    
    private void Start()
    {
        // Cache UI camera (null is correct for ScreenSpaceOverlay)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = canvas.worldCamera;
            else
                uiCamera = null; // ScreenSpaceOverlay uses null camera
        }
        
        // Generate stage images based on Stage singleton
        GenerateStageImages();
        
        // Setup arrow buttons (optional)
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrowClicked);
        
        // Setup play button
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        
        // Start at current stage from Stage singleton
        if (Stage.Instance != null)
        {
            currentIndex = Stage.Instance.GetStage() - 1; // Convert 1-based to 0-based
            currentIndex = Mathf.Clamp(currentIndex, 0, stageImages.Count - 1);
        }
        
        selectedStage = currentIndex + 1;
        
        // Wait for layout to fully build, then initialize positions
        StartCoroutine(InitializeAfterLayout());
    }
    
    /// <summary>
    /// 🔄 Wait for layout to be fully calculated, then store positions and initialize
    /// </summary>
    private IEnumerator InitializeAfterLayout()
    {
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(stageContainer);
        
        // Wait for end of frame to ensure all layout calculations are done
        yield return new WaitForEndOfFrame();
        
        // Now recalculate and store positions
        RecalculatePositions();
        
        // Initialize lock status
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        
        // Initialize - show current stage selected
        UpdateSelection(false);
        UpdateArrowVisibility();
        UpdateLockedStates();
        UpdatePlayButtonColor();
        
        // Cache current unlocked stage
        if (Stage.Instance != null)
            cachedUnlockedStage = Stage.Instance.GetUnlockedStage();
        
        isInitialized = true;
    }
    
    /// <summary>
    /// 🔄 Recalculate container position and stage dimensions
    /// </summary>
    private void RecalculatePositions()
    {
        // Store container's original position
        if (stageContainer != null)
            containerStartX = stageContainer.anchoredPosition.x;
        
        // Get stage width from prefab
        if (stagePrefab != null)
            stageWidth = stagePrefab.sizeDelta.x;
        
        // Get spacing from HorizontalLayoutGroup if exists
        HorizontalLayoutGroup layoutGroup = stageContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            stageSpacing = layoutGroup.spacing;
        }
        else
        {
            stageSpacing = 0f;
        }
    }
    
    /// <summary>
    /// 🎮 Generate stage images based on max stage from Stage singleton
    /// </summary>
    private void GenerateStageImages()
    {
        if (stagePrefab == null || stageContainer == null) return;
        
        // Get max stage from Stage singleton
        int maxStage = 8; // Default fallback
        if (Stage.Instance != null)
        {
            maxStage = Stage.Instance.GetMaxStage();
        }
        
        // Set container height to match prefab height (so it aligns in middle)
        Vector2 prefabSize = stagePrefab.sizeDelta;
        stageContainer.sizeDelta = new Vector2(stageContainer.sizeDelta.x, prefabSize.y);
        
        // Clear any existing children (except prefab if it's in container)
        stageImages.Clear();
        stageCanvasGroups.Clear();
        
        // Generate stage images
        for (int i = 0; i < maxStage; i++)
        {
            int stageNumber = i + 1;
            
            RectTransform stageImage = Instantiate(stagePrefab, stageContainer);
            stageImage.gameObject.name = $"Stage_{stageNumber}";
            stageImage.gameObject.SetActive(true);
            
            // Ensure size matches prefab
            stageImage.sizeDelta = prefabSize;
            
            // Find TMP_Text child and set text to "Stage X"
            TMP_Text stageText = stageImage.GetComponentInChildren<TMP_Text>();
            if (stageText != null)
            {
                stageText.text = $"Stage {stageNumber}";
            }
            
            // Add or get CanvasGroup for alpha control
            CanvasGroup canvasGroup = stageImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = stageImage.gameObject.AddComponent<CanvasGroup>();
            }
            
            stageImages.Add(stageImage);
            stageCanvasGroups.Add(canvasGroup);
        }
        
        // Hide prefab if it's in container
        if (stagePrefab.parent == stageContainer)
        {
            stagePrefab.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 🔒 Update locked/unlocked alpha states based on Stage singleton
    /// </summary>
    private void UpdateLockedStates()
    {
        // Always get fresh unlocked stage from Stage singleton
        int unlockedStage = 1;
        if (Stage.Instance != null)
        {
            unlockedStage = Stage.Instance.GetUnlockedStage();
        }
        
        // Update cached value
        cachedUnlockedStage = unlockedStage;
        
        for (int i = 0; i < stageCanvasGroups.Count; i++)
        {
            int stageNumber = i + 1;
            bool isUnlocked = stageNumber <= unlockedStage;
            
            if (stageCanvasGroups[i] != null)
            {
                // Set alpha directly - unlocked = 1, locked = 0.5
                float targetAlpha = isUnlocked ? unlockedAlpha : lockedAlpha;
                stageCanvasGroups[i].DOKill();
                stageCanvasGroups[i].DOFade(targetAlpha, 0.2f);
            }
        }
    }
    
    /// <summary>
    /// 🔓 Check if a stage index is unlocked
    /// </summary>
    private bool IsStageUnlocked(int index)
    {
        int stageNumber = index + 1;
        int unlockedStage = 1;
        if (Stage.Instance != null)
        {
            unlockedStage = Stage.Instance.GetUnlockedStage();
        }
        return stageNumber <= unlockedStage;
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        HandleSwipe();
        
        // Check for unlock changes in real-time
        CheckForUnlockChanges();
    }
    
    /// <summary>
    /// 🔄 Check if unlocked stage changed and refresh UI
    /// </summary>
    private void CheckForUnlockChanges()
    {
        if (Stage.Instance == null) return;
        
        int currentUnlockedStage = Stage.Instance.GetUnlockedStage();
        if (currentUnlockedStage != cachedUnlockedStage)
        {
            // Stages were unlocked - refresh everything
            cachedUnlockedStage = currentUnlockedStage;
            UpdateLockedStates();
            isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
            UpdatePlayButtonColor();
        }
    }
    
    // ========== SWIPE HANDLING (Manual) ==========
    
    private void HandleSwipe()
    {
        if (isAnimating) return;
        
        // Use touch if available, otherwise use mouse
        bool hasTouch = Input.touchCount > 0;
        
        if (hasTouch)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }
    
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInSwipeRegion(mousePos))
            {
                isDragging = true;
                dragStartX = mousePos.x;
                containerDragStartX = stageContainer.anchoredPosition.x;
            }
        }
        
        if (Input.GetMouseButton(0) && isDragging)
        {
            float deltaX = Input.mousePosition.x - dragStartX;
            stageContainer.anchoredPosition = new Vector2(containerDragStartX + deltaX, stageContainer.anchoredPosition.y);
        }
        
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndSwipe(Input.mousePosition.x - dragStartX);
        }
    }
    
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            if (IsInSwipeRegion(touch.position))
            {
                isDragging = true;
                dragStartX = touch.position.x;
                containerDragStartX = stageContainer.anchoredPosition.x;
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            float deltaX = touch.position.x - dragStartX;
            stageContainer.anchoredPosition = new Vector2(containerDragStartX + deltaX, stageContainer.anchoredPosition.y);
        }
        else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
        {
            EndSwipe(touch.position.x - dragStartX);
        }
    }
    
    private void EndSwipe(float deltaX)
    {
        isDragging = false;
        
        int previousIndex = currentIndex;
        
        if (Mathf.Abs(deltaX) > swipeThreshold)
        {
            // Swipe detected
            if (deltaX > 0 && currentIndex > 0)
            {
                // Swiped right → go to previous (left)
                currentIndex--;
            }
            else if (deltaX < 0 && currentIndex < stageImages.Count - 1)
            {
                // Swiped left → go to next (right)
                currentIndex++;
            }
        }
        
        // Only allow selection of unlocked stages
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
            if (currentIndex != previousIndex)
            {
                GlobalSoundManager.PlaySound(SoundType.buttonClick);
            }
        }
        else
        {
            // Can view locked stages but selection stays on last unlocked
            // Don't update selectedStage
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdatePlayButtonColor();
        
        // Snap to current index
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private bool IsInSwipeRegion(Vector2 screenPosition)
    {
        if (swipeRegion == null) return true; // If no region set, allow everywhere
        return RectTransformUtility.RectangleContainsScreenPoint(swipeRegion, screenPosition, uiCamera);
    }
    
    // ========== PLAY BUTTON ==========
    
    private void OnPlayButtonClicked()
    {
        if (!isCurrentStageUnlocked)
        {
            // Stage is locked - show error effect
            PlayLockedErrorEffect();
            GlobalSoundManager.PlaySound(SoundType.buttonClick); // Or error sound if you have one
            return;
        }
        
        // Stage is unlocked - can proceed (BreachTerminateManager handles actual play)
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
    }
    
    /// <summary>
    /// 🔴 Play error shake effect when trying to play locked stage
    /// </summary>
    private void PlayLockedErrorEffect()
    {
        if (playButton == null) return;
        
        RectTransform buttonRect = playButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.DOKill();
            buttonRect.DOShakeAnchorPos(errorShakeDuration, errorShakeStrength, 20, 90, false, true);
        }
    }
    
    /// <summary>
    /// 🎨 Update play button color based on current stage lock status
    /// </summary>
    private void UpdatePlayButtonColor()
    {
        if (playButtonImage == null) return;
        
        playButtonImage.DOKill();
        Color targetColor = isCurrentStageUnlocked ? normalColor : lockedColor;
        playButtonImage.DOColor(targetColor, 0.2f);
    }
    
    // ========== ARROW BUTTONS ==========
    
    private void OnLeftArrowClicked()
    {
        if (isAnimating || currentIndex <= 0) return;
        
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex--;
        
        // Only update selected stage if unlocked
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdatePlayButtonColor();
        
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private void OnRightArrowClicked()
    {
        if (isAnimating || currentIndex >= stageImages.Count - 1) return;
        
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex++;
        
        // Only update selected stage if unlocked
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdatePlayButtonColor();
        
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    // ========== SELECTION & ANIMATION ==========
    
    private void UpdateSelection(bool animate)
    {
        if (stageImages == null || stageImages.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= stageImages.Count) return;
        if (stageImages[currentIndex] == null) return;
        
        // Calculate target position based on stage width and spacing
        // Move container left by (index * (width + spacing)) to center selected stage
        float offsetFromFirst = currentIndex * (stageWidth + stageSpacing);
        float targetX = containerStartX - offsetFromFirst;
        
        if (animate)
        {
            isAnimating = true;
            stageContainer.DOKill();
            stageContainer.DOAnchorPosX(targetX, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            stageContainer.anchoredPosition = new Vector2(targetX, stageContainer.anchoredPosition.y);
        }
        
        // Scale images - only selected stage gets big scale
        for (int i = 0; i < stageImages.Count; i++)
        {
            if (stageImages[i] == null) continue;
            
            Transform imgTransform = stageImages[i].transform;
            // Scale up only if this is the current viewed stage AND it's unlocked and selected
            bool isSelected = (i == currentIndex) && IsStageUnlocked(i);
            float targetScale = isSelected ? selectedScale : unselectedScale;
            
            if (animate)
            {
                imgTransform.DOKill();
                imgTransform.DOScale(targetScale, scaleDuration).SetEase(scaleEase);
            }
            else
            {
                imgTransform.localScale = Vector3.one * targetScale;
            }
        }
    }
    
    private void UpdateArrowVisibility()
    {
        // Hide left arrow if at first stage
        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(currentIndex > 0);
        
        // Hide right arrow if at last stage
        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(currentIndex < stageImages.Count - 1);
    }
    
    /// <summary>
    /// 🎯 Get the currently selected stage number (1-based)
    /// </summary>
    public int GetSelectedStage()
    {
        return selectedStage;
    }
    
    /// <summary>
    /// 🔓 Check if current viewed stage is playable (unlocked)
    /// </summary>
    public bool IsCurrentStagePlayable()
    {
        return isCurrentStageUnlocked;
    }
    
    /// <summary>
    /// 🎯 Get current view index (0-based)
    /// </summary>
    public int GetCurrentStageIndex()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// 🎯 Set stage index and update view
    /// </summary>
    public void SetStageIndex(int index)
    {
        if (index >= 0 && index < stageImages.Count)
        {
            currentIndex = index;
            if (IsStageUnlocked(index))
            {
                selectedStage = index + 1;
            }
            UpdateSelection(false);
            UpdateArrowVisibility();
        }
    }
    
    /// <summary>
    /// 🔄 Refresh locked states (call after unlocking new stages)
    /// </summary>
    public void RefreshLockedStates()
    {
        UpdateLockedStates();
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdatePlayButtonColor();
        UpdateSelection(false);
        
        // Update cached value
        if (Stage.Instance != null)
            cachedUnlockedStage = Stage.Instance.GetUnlockedStage();
    }
    
    /// <summary>
    /// 🔄 Called when object becomes active - refresh unlock states
    /// </summary>
    private void OnEnable()
    {
        if (!isInitialized) return;
        
        // Always refresh when enabled - get fresh data from Stage singleton
        if (Stage.Instance != null)
        {
            int currentUnlockedStage = Stage.Instance.GetUnlockedStage();
            
            // Always update to ensure sync with Stage singleton
            cachedUnlockedStage = currentUnlockedStage;
            
            // Refresh all locked states
            UpdateLockedStates();
            
            // Update current stage unlock status
            isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
            UpdatePlayButtonColor();
            UpdateSelection(false);
        }
    }
    
    private void OnDestroy()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.RemoveListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.RemoveListener(OnRightArrowClicked);
        
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
    }
}
