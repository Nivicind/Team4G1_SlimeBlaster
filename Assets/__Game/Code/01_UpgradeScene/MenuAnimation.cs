using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MenuAnimation : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private RectTransform title; // e.g., "SlimeBlaster" text
    [SerializeField] private float titleOvershootScale = 1.25f;
    [SerializeField] private float titleScaleDuration = 0.9f;
    [SerializeField] private float delayAfterTitle = 0.2f; // when splash+slimes begin relative to title start

    [Header("Splash")]
    [SerializeField] private RectTransform splash;
    [SerializeField] private float splashDuration = 0.9f;
    [SerializeField] private float delayAfterSplash = 0.25f;

    [System.Serializable]
    public class SlimeTween
    {
        public RectTransform slime;
        public RectTransform fromPoint; // start anchor (UI)
        public RectTransform toPoint;   // end anchor (UI)
        public float startScale = 0.65f;
        public float endScale = 1f;
    }

    [Header("Slimes")]
    [SerializeField] private List<SlimeTween> slimes = new List<SlimeTween>();
    [SerializeField] private float slimeMoveDuration = 0.9f;

    [Header("Idle Wobble")]
    [SerializeField] private float idleMoveRadius = 8f;
    [SerializeField] private float idleMoveDuration = 2.2f;

    [Header("Playback")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private float startDelay = 1.5f;

    private Vector2 titleStartPos;
    private Vector3 titleStartScale;
    private Vector3 splashStartScale;
    private Sequence sequence;
    private readonly List<Tweener> idleTweens = new List<Tweener>();

    private void Awake()
    {
        CacheDefaults();
    }

    private void OnDestroy()
    {
        sequence?.Kill();
        KillIdleTweens();
    }

    private void Start()
    {
        if (autoPlayOnStart)
            PlaySequence();
    }

    public void PlaySequence()
    {
        sequence?.Kill();
        KillIdleTweens();
        ResetToStartState();

        // Hide all elements at start
        HideAllElements();

        sequence = DOTween.Sequence();

        // Wait for start delay
        sequence.AppendInterval(startDelay);

        // Show all elements after delay
        sequence.AppendCallback(ShowAllElements);

        // Now run the original animation sequence
        if (title != null)
        {
            sequence.Append(title.DOScale(titleStartScale, titleScaleDuration).SetEase(Ease.OutBack));
        }

        if (splash != null)
        {
            // Splash and slimes start delayAfterTitle seconds after title STARTS (not ends)
            // Title starts at startDelay, so splash starts at startDelay + delayAfterTitle
            float splashStartTime = startDelay + delayAfterTitle;
            sequence.Insert(splashStartTime, splash.DOScale(splashStartScale, splashDuration).SetEase(Ease.OutElastic));
            foreach (var slime in slimes)
            {
                if (slime == null || slime.slime == null)
                    continue;

                var to = slime.toPoint != null ? slime.toPoint.anchoredPosition : slime.slime.anchoredPosition;

                sequence.Insert(splashStartTime, slime.slime.DOAnchorPos(to, slimeMoveDuration).SetEase(Ease.OutBack));
                sequence.Insert(splashStartTime, slime.slime.DOScale(slime.endScale, slimeMoveDuration * 0.9f).SetEase(Ease.OutElastic));
            }
        }

        sequence.AppendInterval(delayAfterSplash);

        sequence.OnComplete(StartIdleWobble);
    }

    private void HideAllElements()
    {
        if (title != null)
            title.gameObject.SetActive(false);

        if (splash != null)
            splash.gameObject.SetActive(false);

        foreach (var slime in slimes)
        {
            if (slime?.slime != null)
                slime.slime.gameObject.SetActive(false);
        }
    }

    private void ShowAllElements()
    {
        if (title != null)
            title.gameObject.SetActive(true);

        if (splash != null)
            splash.gameObject.SetActive(true);

        foreach (var slime in slimes)
        {
            if (slime?.slime != null)
                slime.slime.gameObject.SetActive(true);
        }
    }

    private void CacheDefaults()
    {
        if (title != null)
        {
            titleStartPos = title.anchoredPosition;
            titleStartScale = title.localScale;
        }

        if (splash != null)
            splashStartScale = splash.localScale;
    }

    private void ResetToStartState()
    {
        if (title != null)
        {
            title.anchoredPosition = titleStartPos;
            title.localScale = titleStartScale * titleOvershootScale;
        }

        if (splash != null)
            splash.localScale = Vector3.zero;

        foreach (var slime in slimes)
        {
            if (slime == null || slime.slime == null)
                continue;

            var from = slime.fromPoint != null ? slime.fromPoint.anchoredPosition : slime.slime.anchoredPosition;

            slime.slime.anchoredPosition = from;
            slime.slime.localScale = Vector3.one * slime.startScale;
        }
    }

    private void StartIdleWobble()
    {
        KillIdleTweens();

        foreach (var slime in slimes)
        {
            if (slime == null || slime.slime == null)
                continue;

            var basePos = slime.toPoint != null ? slime.toPoint.anchoredPosition : slime.slime.anchoredPosition;
            var offset = Random.insideUnitCircle * idleMoveRadius;

            slime.slime.anchoredPosition = basePos;

            var tween = slime.slime.DOAnchorPos(basePos + offset, idleMoveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            idleTweens.Add(tween);
        }
    }

    private void KillIdleTweens()
    {
        foreach (var tween in idleTweens)
        {
            tween?.Kill();
        }

        idleTweens.Clear();
    }
}