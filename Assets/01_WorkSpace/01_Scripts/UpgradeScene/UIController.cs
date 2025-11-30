using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    [Header("References")]
    public Button resourceButton;
    public GameObject resourcePanel;

    [Header("Animation Settings")]
    public float moveAmount = 300f;
    public float animationDuration = 0.5f;
    public Ease easeType = Ease.OutQuad;

    private bool isPanelOpen = true;
    private Vector3 originalPosition;

    private void Start()
    {
        if (resourcePanel != null)
        {
            originalPosition = resourcePanel.transform.localPosition;
        }

        if (resourceButton != null)
        {
            resourceButton.onClick.AddListener(ToggleResourcePanel);
        }
    }

    private void ToggleResourcePanel()
    {
        if (resourcePanel == null) return;

        if (isPanelOpen)
        {
            // Close panel - move to the right
            resourcePanel.transform.DOLocalMoveX(originalPosition.x + moveAmount, animationDuration).SetEase(easeType);
            isPanelOpen = false;
        }
        else
        {
            // Open panel - move to the left
            resourcePanel.transform.DOLocalMoveX(originalPosition.x, animationDuration).SetEase(easeType);
            isPanelOpen = true;
        }
    }

    private void OnDestroy()
    {
        if (resourceButton != null)
        {
            resourceButton.onClick.RemoveListener(ToggleResourcePanel);
        }
    }
}
