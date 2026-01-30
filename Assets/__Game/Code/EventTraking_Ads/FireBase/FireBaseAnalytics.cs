using UnityEngine;
using Firebase.Extensions;
using Firebase.Analytics;

public class FireBaseAnalytics : MonoBehaviour
{
    public static FireBaseAnalytics Instance;
    private bool isFirebaseReady = false;
    private Firebase.FirebaseApp app;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
                isFirebaseReady = true;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void LogLevelComplete(int levelNumber, int score)
    {
        if (!isFirebaseReady) return;
        Debug.Log("LogLevelComplete Firebase");
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "level_complete",
            new Firebase.Analytics.Parameter("level_number", levelNumber),
            new Firebase.Analytics.Parameter("score", score)
        );
        Debug.Log("LogLevelComplete Firebase End");
    }
    public void LogLevelReset(int levelNumber)
    {
        if (!isFirebaseReady) return;
        Debug.Log("LogLevelReset Firebase");
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "level_reset",
            new Firebase.Analytics.Parameter("level_number", levelNumber)
        );
        Debug.Log("LogLevelReset Firebase End");
    }
}
