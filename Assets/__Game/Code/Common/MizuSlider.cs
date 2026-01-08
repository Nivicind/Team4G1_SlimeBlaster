using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 🎚️ Mizu Slider - Custom slider using Image fill
/// Place on parent, children: Background, Fill, Handle
/// Works in both Editor and Play mode
/// </summary>
[ExecuteAlways]
public class MizuSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Components")]
    [SerializeField] private Image background;  // Background image
    [SerializeField] private Image fill;        // Fill image (must be set to Filled type)
    [SerializeField] private RectTransform handle;  // Handle transform

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float value = 1f;
    [SerializeField] private bool wholeNumbers = false;
    [SerializeField] private float _minValue = 0f;
    [SerializeField] private float _maxValue = 1f;

    [Header("Events")]
    public UnityEvent<float> onValueChanged;

    private RectTransform backgroundRect;

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float minValue => _minValue;
    public float maxValue => _maxValue;

    public float NormalizedValue
    {
        get => Mathf.InverseLerp(_minValue, _maxValue, value);
        set => SetValue(Mathf.Lerp(_minValue, _maxValue, value));
    }

    private void Awake()
    {
        if (background != null)
        {
            backgroundRect = background.GetComponent<RectTransform>();
        }
        UpdateVisuals();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void OnValidate()
    {
        // Cache rect in editor
        if (background != null)
        {
            backgroundRect = background.GetComponent<RectTransform>();
        }
        UpdateVisuals();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Keep visuals updated in editor mode
        if (!Application.isPlaying)
        {
            UpdateVisuals();
        }
    }
#endif

    public void SetValue(float newValue)
    {
        // Clamp value
        newValue = Mathf.Clamp(newValue, minValue, maxValue);

        // Round if whole numbers
        if (wholeNumbers)
        {
            newValue = Mathf.Round(newValue);
        }

        // Only update if changed
        if (Mathf.Approximately(value, newValue)) return;

        value = newValue;
        UpdateVisuals();
        onValueChanged?.Invoke(value);
    }

    public void SetValueWithoutNotify(float newValue)
    {
        newValue = Mathf.Clamp(newValue, minValue, maxValue);
        if (wholeNumbers)
        {
            newValue = Mathf.Round(newValue);
        }
        value = newValue;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float normalized = Mathf.InverseLerp(minValue, maxValue, value);

        // Update fill amount
        if (fill != null)
        {
            fill.fillAmount = normalized;
        }

        // Update handle position
        if (handle != null && backgroundRect != null)
        {
            float width = backgroundRect.rect.width;
            float xPos = normalized * width - (width * 0.5f);
            handle.anchoredPosition = new Vector2(xPos, handle.anchoredPosition.y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateValueFromPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateValueFromPosition(eventData);
    }

    private void UpdateValueFromPosition(PointerEventData eventData)
    {
        if (backgroundRect == null) return;

        // Get local position relative to background
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Calculate normalized position (0-1)
            float width = backgroundRect.rect.width;
            float normalized = (localPoint.x + width * 0.5f) / width;
            normalized = Mathf.Clamp01(normalized);

            // Convert to actual value
            float newValue = Mathf.Lerp(minValue, maxValue, normalized);
            SetValue(newValue);
        }
    }
}
