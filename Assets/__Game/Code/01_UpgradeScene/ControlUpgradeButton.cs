using UnityEngine;

public class ControlUpgradeButton : MonoBehaviour
{
    [Header("Target to Control")]
    public GameObject targetObject; // The object/map to move
    public GameObject background;   // Background that moves slower (parallax effect)

    [Header("Pan Settings")]
    public float panSpeed = 1f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;
    
    [Header("Movement Bounds - Target (UI Local Position)")]
    public bool limitTargetMovement = true;
    public Vector2 targetMinPosition = new Vector2(-500f, -800f);
    public Vector2 targetMaxPosition = new Vector2(500f, 800f);
    
    [Header("Movement Bounds - Background (UI Local Position)")]
    public bool limitBackgroundMovement = true;
    public Vector2 backgroundMinPosition = new Vector2(-250f, -400f);
    public Vector2 backgroundMaxPosition = new Vector2(250f, 400f);

    [Header("🚫 Ignore Region (Stage Select)")]
    public RectTransform ignoreRegion; // Touches in this region won't affect upgrade controller

    private Vector3 lastMousePosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;

    // For mobile pinch zoom
    private float lastPinchDistance = 0f;
    
    // Cached RectTransforms for UI
    private RectTransform targetRect;
    private RectTransform backgroundRect;
    
    private Camera uiCamera;
    
    private void Start()
    {
        // Cache RectTransforms for UI elements
        if (targetObject != null)
            targetRect = targetObject.GetComponent<RectTransform>();
        if (background != null)
            backgroundRect = background.GetComponent<RectTransform>();
        
        // Cache UI camera
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCamera = canvas.worldCamera;
    }

    private void Update()
    {
        if (targetObject == null) return;

        // Check all controls every frame
        HandleMouseDrag();
        HandleMouseZoom();
        HandleTouchDrag();
        HandlePinchZoom();
        
        // Clamp positions within bounds
        ClampPositions();
    }
    
    /// <summary>
    /// Check if position is inside the ignore region (stage select area)
    /// </summary>
    private bool IsInIgnoreRegion(Vector2 screenPosition)
    {
        if (ignoreRegion == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(ignoreRegion, screenPosition, uiCamera);
    }
    
    private void ClampPositions()
    {
        // Get current scale factor (use X scale as reference)
        float targetScale = targetObject != null ? targetObject.transform.localScale.x : 1f;
        float bgScale = background != null ? background.transform.localScale.x : 1f;
        
        // Clamp target object position (bounds scale with zoom)
        if (limitTargetMovement && targetRect != null)
        {
            // Scale bounds based on zoom level
            Vector2 scaledMin = targetMinPosition * targetScale;
            Vector2 scaledMax = targetMaxPosition * targetScale;
            
            Vector2 pos = targetRect.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, scaledMin.x, scaledMax.x);
            pos.y = Mathf.Clamp(pos.y, scaledMin.y, scaledMax.y);
            targetRect.anchoredPosition = pos;
        }
        
        // Clamp background position (bounds scale with zoom)
        if (limitBackgroundMovement && backgroundRect != null)
        {
            // Scale bounds based on zoom level
            Vector2 scaledBgMin = backgroundMinPosition * bgScale;
            Vector2 scaledBgMax = backgroundMaxPosition * bgScale;
            
            Vector2 bgPos = backgroundRect.anchoredPosition;
            bgPos.x = Mathf.Clamp(bgPos.x, scaledBgMin.x, scaledBgMax.x);
            bgPos.y = Mathf.Clamp(bgPos.y, scaledBgMin.y, scaledBgMax.y);
            backgroundRect.anchoredPosition = bgPos;
        }
    }

    // PC: Mouse drag to pan
    private void HandleMouseDrag()
    {
        if (PlayerInputHandler.Instance.IsInputDown())
        {
            Vector2 inputPos = PlayerInputHandler.Instance.GetInputScreenPosition();
            
            // Ignore if touch started in ignore region
            if (IsInIgnoreRegion(inputPos))
                return;
            
            isDragging = true;
            lastMousePosition = inputPos;
        }

        if (PlayerInputHandler.Instance.IsInputActive() && isDragging)
        {
            Vector3 delta = PlayerInputHandler.Instance.GetInputScreenPosition() - lastMousePosition;
            
            // For UI, move using anchoredPosition instead of world position
            if (targetRect != null)
            {
                targetRect.anchoredPosition += new Vector2(delta.x, delta.y) * panSpeed;
            }
            if (backgroundRect != null)
            {
                backgroundRect.anchoredPosition += new Vector2(delta.x, delta.y) * panSpeed * 0.5f;
            }
            
            lastMousePosition = PlayerInputHandler.Instance.GetInputScreenPosition();
        }

        if (PlayerInputHandler.Instance.IsInputUp())
        {
            isDragging = false;
        }
    }

    // PC: Mouse scroll wheel to zoom
    private void HandleMouseZoom()
    {
        float scroll = PlayerInputHandler.Instance.GetScrollWheel();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 scale = targetObject.transform.localScale;
            scale += Vector3.one * scroll * zoomSpeed;
            scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
            scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
            scale.z = Mathf.Clamp(scale.z, minZoom, maxZoom);
            targetObject.transform.localScale = scale;

            if (background != null)
            {
                Vector3 bgScale = background.transform.localScale;
                bgScale += Vector3.one * scroll * zoomSpeed * 0.5f;
                bgScale.x = Mathf.Clamp(bgScale.x, minZoom, maxZoom);
                bgScale.y = Mathf.Clamp(bgScale.y, minZoom, maxZoom);
                bgScale.z = Mathf.Clamp(bgScale.z, minZoom, maxZoom);
                background.transform.localScale = bgScale;
            }
        }
    }

    // Mobile: Touch drag to pan
    private void HandleTouchDrag()
    {
        if (PlayerInputHandler.Instance.GetTouchCount() == 1)
        {
            Touch? touch = PlayerInputHandler.Instance.GetTouch(0);
            if (!touch.HasValue) return;

            if (touch.Value.phase == TouchPhase.Began)
            {
                // Ignore if touch started in ignore region
                if (IsInIgnoreRegion(touch.Value.position))
                    return;
                
                isDragging = true;
                lastMousePosition = touch.Value.position;
            }
            else if (touch.Value.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 delta = (Vector3)touch.Value.position - lastMousePosition;
                
                // For UI, move using anchoredPosition instead of world position
                if (targetRect != null)
                {
                    targetRect.anchoredPosition += new Vector2(delta.x, delta.y) * panSpeed;
                }
                if (backgroundRect != null)
                {
                    backgroundRect.anchoredPosition += new Vector2(delta.x, delta.y) * panSpeed * 0.5f;
                }
                
                lastMousePosition = touch.Value.position;
            }
            else if (touch.Value.phase == TouchPhase.Ended || touch.Value.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    // Mobile: Pinch to zoom
    private void HandlePinchZoom()
    {
        if (PlayerInputHandler.Instance.GetTouchCount() == 2)
        {
            Touch? touch1 = PlayerInputHandler.Instance.GetTouch(0);
            Touch? touch2 = PlayerInputHandler.Instance.GetTouch(1);
            
            if (!touch1.HasValue || !touch2.HasValue) return;

            Vector2 touch1PrevPos = touch1.Value.position - touch1.Value.deltaPosition;
            Vector2 touch2PrevPos = touch2.Value.position - touch2.Value.deltaPosition;

            float prevDistance = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentDistance = (touch1.Value.position - touch2.Value.position).magnitude;

            float difference = currentDistance - prevDistance;

            if (Mathf.Abs(difference) > 0.01f)
            {
                Vector3 scale = targetObject.transform.localScale;
                scale += Vector3.one * difference * zoomSpeed * 0.01f;
                scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
                scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
                scale.z = Mathf.Clamp(scale.z, minZoom, maxZoom);
                targetObject.transform.localScale = scale;

                if (background != null)
                {
                    Vector3 bgScale = background.transform.localScale;
                    bgScale += Vector3.one * difference * zoomSpeed * 0.01f * 0.5f;
                    bgScale.x = Mathf.Clamp(bgScale.x, minZoom, maxZoom);
                    bgScale.y = Mathf.Clamp(bgScale.y, minZoom, maxZoom);
                    bgScale.z = Mathf.Clamp(bgScale.z, minZoom, maxZoom);
                    background.transform.localScale = bgScale;
                }
            }
        }
    }
}

