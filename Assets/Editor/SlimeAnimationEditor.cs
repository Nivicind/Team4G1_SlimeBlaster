using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SlimeAnimation))]
public class SlimeAnimationEditor : Editor
{
    private bool isPlaying = false;
    private int previewFrame = 0;
    private double lastUpdateTime = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SlimeAnimation slimeAnim = (SlimeAnimation)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);

        // Play/Stop buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play", GUILayout.Height(30)))
        {
            isPlaying = true;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }
        if (GUILayout.Button("Stop", GUILayout.Height(30)))
        {
            isPlaying = false;
        }
        EditorGUILayout.EndHorizontal();

        // Frame slider
        EditorGUILayout.Space(5);
        int maxFrame = GetMaxFrameCount(slimeAnim);
        
        EditorGUI.BeginChangeCheck();
        int newFrame = EditorGUILayout.IntSlider("Preview Frame", previewFrame, 0, Mathf.Max(0, maxFrame - 1));
        if (EditorGUI.EndChangeCheck())
        {
            previewFrame = newFrame;
            isPlaying = false;
            UpdatePreviewFrame(slimeAnim, previewFrame);
        }

        EditorGUILayout.LabelField($"Total Frames: {maxFrame}");

        // Update animation when playing
        if (isPlaying && !Application.isPlaying)
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastUpdateTime >= slimeAnim.frameChangeDuration)
            {
                lastUpdateTime = currentTime;
                previewFrame = (previewFrame + 1) % maxFrame;
                UpdatePreviewFrame(slimeAnim, previewFrame);
                Repaint();
            }
            
            // Keep repainting while playing
            EditorUtility.SetDirty(target);
        }
    }

    private int GetMaxFrameCount(SlimeAnimation slimeAnim)
    {
        int maxCount = 1;
        
        if (slimeAnim.idle != null && slimeAnim.idle.Count > maxCount)
            maxCount = slimeAnim.idle.Count;
        if (slimeAnim.blink != null && slimeAnim.blink.Count > maxCount)
            maxCount = slimeAnim.blink.Count;
        if (slimeAnim.hurt != null && slimeAnim.hurt.Count > maxCount)
            maxCount = slimeAnim.hurt.Count;
        if (slimeAnim.hollowed != null && slimeAnim.hollowed.Count > maxCount)
            maxCount = slimeAnim.hollowed.Count;
        if (slimeAnim.inside != null && slimeAnim.inside.Count > maxCount)
            maxCount = slimeAnim.inside.Count;
        
        return maxCount;
    }

    private void UpdatePreviewFrame(SlimeAnimation slimeAnim, int frame)
    {
        // Update main sprite
        if (slimeAnim.mainSpriteRenderer != null && slimeAnim.idle != null && slimeAnim.idle.Count > 0)
        {
            int idleFrame = frame % slimeAnim.idle.Count;
            slimeAnim.mainSpriteRenderer.sprite = slimeAnim.idle[idleFrame];
        }

        // Update hollowed sprite
        if (slimeAnim.hollowedSpriteRenderer != null && slimeAnim.hollowed != null && slimeAnim.hollowed.Count > 0)
        {
            int hollowedFrame = frame % slimeAnim.hollowed.Count;
            slimeAnim.hollowedSpriteRenderer.sprite = slimeAnim.hollowed[hollowedFrame];
        }

        // Update liquid sprite
        if (slimeAnim.liquidSpriteRenderer != null && slimeAnim.inside != null && slimeAnim.inside.Count > 0)
        {
            int insideFrame = frame % slimeAnim.inside.Count;
            slimeAnim.liquidSpriteRenderer.sprite = slimeAnim.inside[insideFrame];
        }

        // Update sprite mask
        if (slimeAnim.spriteMaskRenderer != null && slimeAnim.spriteMask != null && slimeAnim.spriteMask.Count > 0)
        {
            int maskFrame = frame % slimeAnim.spriteMask.Count;
            slimeAnim.spriteMaskRenderer.sprite = slimeAnim.spriteMask[maskFrame];
        }

        EditorUtility.SetDirty(slimeAnim);
    }

    private void OnDisable()
    {
        isPlaying = false;
    }
}
