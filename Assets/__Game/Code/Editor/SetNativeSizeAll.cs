using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SetNativeSizeAll : Editor
{
    [MenuItem("Tools/UI/Set Native Size for All UI Elements")]
    static void SetNativeSizeForAllUIElements()
    {
        // Get all Image components in the scene
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Image image in images)
        {
            if (image != null && image.sprite != null)
            {
                Undo.RecordObject(image.rectTransform, "Set Native Size");
                image.SetNativeSize();
                count++;
            }
        }

        // Get all RawImage components in the scene
        RawImage[] rawImages = FindObjectsByType<RawImage>(FindObjectsSortMode.None);
        foreach (RawImage rawImage in rawImages)
        {
            if (rawImage != null && rawImage.texture != null)
            {
                Undo.RecordObject(rawImage.rectTransform, "Set Native Size");
                rawImage.rectTransform.sizeDelta = new Vector2(rawImage.texture.width, rawImage.texture.height);
                count++;
            }
        }

        Debug.Log($"Set native size for {count} UI elements.");
    }

    [MenuItem("Tools/UI/Set Native Size for Selected UI Elements")]
    static void SetNativeSizeForSelected()
    {
        int count = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            // Check for Image component
            Image image = obj.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                Undo.RecordObject(image.rectTransform, "Set Native Size");
                image.SetNativeSize();
                count++;
                continue;
            }

            // Check for RawImage component
            RawImage rawImage = obj.GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture != null)
            {
                Undo.RecordObject(rawImage.rectTransform, "Set Native Size");
                rawImage.rectTransform.sizeDelta = new Vector2(rawImage.texture.width, rawImage.texture.height);
                count++;
            }
        }

        if (count > 0)
            Debug.Log($"Set native size for {count} selected UI elements.");
        else
            Debug.LogWarning("No UI elements with native size support found in selection.");
    }
}
