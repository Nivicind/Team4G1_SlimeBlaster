using UnityEngine;
using UnityEngine.UI;
using Firebase.Analytics;


/// 📺 Test Show Ads
public class FirebaseTestButton : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(AdsEvents);
        }
    }

    [ContextMenu("📺 Test Show Ads")]
    public void AdsEvents()
    {
        Debug.Log("ADS event Started");
        FirebaseAnalytics.LogEvent("PressedAds", new Parameter("Ads", 1));
        Debug.Log("ADS event Ended");
    }
}
