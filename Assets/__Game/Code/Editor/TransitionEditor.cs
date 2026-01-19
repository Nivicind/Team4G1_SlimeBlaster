using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Transition))]
public class TransitionEditor : Editor
{
    private bool isPreviewingIn = false;
    private bool isPreviewingOut = false;
    private int currentFrame = 0;
    private double lastFrameTime = 0;

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        StopPreview();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Transition transition = (Transition)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test Transitions", EditorStyles.boldLabel);

        // Edit Mode Previews
        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Edit Mode Preview", EditorStyles.miniLabel);
            
            EditorGUI.BeginDisabledGroup(isPreviewingIn || isPreviewingOut);
            
            if (GUILayout.Button("🎬 Preview IN (Edit Mode)"))
            {
                StartPreview(transition, false);
            }
            
            if (GUILayout.Button("🎬 Preview OUT (Edit Mode)"))
            {
                StartPreview(transition, true);
            }
            
            EditorGUI.EndDisabledGroup();
            
            if (isPreviewingIn || isPreviewingOut)
            {
                if (GUILayout.Button("⏹️ Stop Preview"))
                {
                    StopPreview();
                }
            }
            
            EditorGUILayout.Space(5);
        }

        // Play Mode Tests
        EditorGUILayout.LabelField("Play Mode Tests", EditorStyles.miniLabel);

        if (GUILayout.Button("▶️ Play Full Transition"))
        {
            if (Application.isPlaying)
            {
                transition.PlayTransition(() =>
                {
                    Debug.Log("Middle of transition!");
                });
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test transitions!");
            }
        }

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔲 Transition IN"))
        {
            if (Application.isPlaying)
            {
                transition.TransitionIn(() =>
                {
                    Debug.Log("Transition IN complete!");
                });
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test transitions!");
            }
        }

        if (GUILayout.Button("🔲 Transition OUT"))
        {
            if (Application.isPlaying)
            {
                transition.TransitionOut(() =>
                {
                    Debug.Log("Transition OUT complete!");
                });
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test transitions!");
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void StartPreview(Transition transition, bool isOut)
    {
        StopPreview();
        
        if (isOut)
            isPreviewingOut = true;
        else
            isPreviewingIn = true;
            
        currentFrame = 0;
        lastFrameTime = EditorApplication.timeSinceStartup;
        
        // Show and set first frame
        if (transition.transitionImage != null)
        {
            transition.transitionImage.gameObject.SetActive(true);
            List<Sprite> sprites = isOut ? transition.transitionOutSprites : transition.transitionInSprites;
            if (sprites != null && sprites.Count > 0)
            {
                transition.transitionImage.sprite = sprites[0];
                EditorUtility.SetDirty(transition.transitionImage);
            }
        }
    }

    private void StopPreview()
    {
        Transition transition = (Transition)target;
        
        isPreviewingIn = false;
        isPreviewingOut = false;
        currentFrame = 0;
        
        // Hide image
        if (transition.transitionImage != null)
        {
            transition.transitionImage.gameObject.SetActive(false);
            EditorUtility.SetDirty(transition.transitionImage);
        }
    }

    private void OnEditorUpdate()
    {
        if (!isPreviewingIn && !isPreviewingOut)
            return;

        Transition transition = (Transition)target;
        if (transition == null || transition.transitionImage == null)
        {
            StopPreview();
            return;
        }

        List<Sprite> sprites = isPreviewingOut ? transition.transitionOutSprites : transition.transitionInSprites;
        
        if (sprites == null || sprites.Count == 0)
        {
            StopPreview();
            return;
        }

        // Check if enough time has passed
        double currentTime = EditorApplication.timeSinceStartup;
        if (currentTime - lastFrameTime >= transition.frameDuration)
        {
            lastFrameTime = currentTime;
            currentFrame++;

            if (currentFrame >= sprites.Count)
            {
                // Animation finished
                if (isPreviewingOut)
                {
                    transition.transitionImage.gameObject.SetActive(false);
                }
                StopPreview();
            }
            else
            {
                // Set next frame
                transition.transitionImage.sprite = sprites[currentFrame];
                EditorUtility.SetDirty(transition.transitionImage);
            }
        }

        // Force repaint to show animation
        Repaint();
    }
}
