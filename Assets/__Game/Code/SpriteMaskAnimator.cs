using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskAnimator : MonoBehaviour
{
    public List<Sprite> frames;
    public float frameTime = 0.1f;

    private SpriteMask mask;

    private float timer = 0f;
    private int index = 0;

    void Awake()
    {
        mask = GetComponent<SpriteMask>();

        if (mask == null)
        {
            Debug.LogError("No SpriteMask found on this GameObject!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (frames.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;

            mask.sprite = frames[index];
            index = (index + 1) % frames.Count;
        }
    }
}
