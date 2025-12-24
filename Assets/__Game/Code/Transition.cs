using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    [Header("UI")]
    public Image transitionImage;
    
    [Header("Sprites")]
    public List<Sprite> transitionSprites;
    
    [Header("Settings")]
    public float frameDuration = 0.05f;
    
    private Coroutine currentTransition;

    public void TransitionIn()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        
        currentTransition = StartCoroutine(PlayTransition(false));
    }

    public void TransitionOut()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        
        currentTransition = StartCoroutine(PlayTransition(true));
    }

    private IEnumerator PlayTransition(bool reverse)
    {
        if (transitionImage == null || transitionSprites == null || transitionSprites.Count == 0)
            yield break;

        transitionImage.gameObject.SetActive(true);

        if (reverse)
        {
            // Play backwards (last to first)
            for (int i = transitionSprites.Count - 1; i >= 0; i--)
            {
                transitionImage.sprite = transitionSprites[i];
                yield return new WaitForSecondsRealtime(frameDuration);
            }
        }
        else
        {
            // Play forwards (first to last)
            for (int i = 0; i < transitionSprites.Count; i++)
            {
                transitionImage.sprite = transitionSprites[i];
                yield return new WaitForSecondsRealtime(frameDuration);
            }
        }

        currentTransition = null;
    }
}