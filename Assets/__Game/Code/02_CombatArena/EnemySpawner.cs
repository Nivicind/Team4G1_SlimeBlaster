using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemySpawnEntry
{
    public ObjectPool enemyPool;
    public SOEnemyData enemyData;
}

[System.Serializable]
public class LevelSpawnConfig
{
    public int level;
    public List<EnemySpawnEntry> enemiesForThisLevel;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;        // For spawn rate
    public BoxCollider2D targetArea;       // Area enemies move toward
    public StoreCurrencyReference currencyReference;  // Currency pools reference

    [Header("Level Configurations")]
    public List<LevelSpawnConfig> levelConfigs;

    [Header("Spawn Settings")]
    public float spawnBuffer = 0.5f;       // How far outside camera to spawn

    private Camera mainCamera;
    private float timer;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private List<EnemySpawnEntry> currentLevelEnemies = new List<EnemySpawnEntry>();

    private void Awake()
    {
        mainCamera = Camera.main;
        Debug.Log($"EnemySpawner Awake - levelConfigs count: {levelConfigs?.Count ?? 0}");
    }

    private void OnEnable()
    {
        LoadCurrentLevelEnemies();
    }

    private void LoadCurrentLevelEnemies()
    {
        currentLevelEnemies.Clear();

        if (Stage.Instance == null)
        {
            Debug.LogWarning("Stage.Instance is null");
            return;
        }

        int currentStage = Stage.Instance.GetStage();
        Debug.Log($"Loading enemies for stage {currentStage}. Total configs: {levelConfigs?.Count ?? 0}");

        // Find the config for current stage
        if (levelConfigs != null)
        {
            foreach (var config in levelConfigs)
            {
                Debug.Log($"Checking config for stage {config.level}, enemies in config: {config.enemiesForThisLevel?.Count ?? 0}");
                if (config.level == currentStage)
                {
                    if (config.enemiesForThisLevel != null && config.enemiesForThisLevel.Count > 0)
                    {
                        // Copy the list to avoid reference issues
                        currentLevelEnemies = new List<EnemySpawnEntry>(config.enemiesForThisLevel);
                        Debug.Log($"✓ Loaded {currentLevelEnemies.Count} enemy types for stage {currentStage}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠ Stage {currentStage} config exists but enemiesForThisLevel list is empty or null!");
                    }
                    return;
                }
            }
        }

        Debug.LogWarning($"⚠ No spawn configuration found for stage {currentStage}");
    }

    private void Update()
    {
        timer += Time.deltaTime;

        foreach (var entry in currentLevelEnemies)
        {
            if (Time.timeSinceLevelLoad < entry.enemyData.startTime) continue;

            float adjustedInterval = entry.enemyData.spawnInterval / Mathf.Max(0.01f, playerStats.GetStatValue(EnumStat.spawnRatePercent) / 100f);
            if (timer >= adjustedInterval)
            {
                SpawnEnemyType(entry);
            }
        }

        if (timer >= GetMaxInterval()) timer = 0f;
    }

    private void SpawnEnemyType(EnemySpawnEntry entry)
    {
        // Clean null or inactive references
        activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        // Count current active enemies of this type
        int activeCount = activeEnemies.FindAll(e => e.enemyData == entry.enemyData).Count;

        // Calculate how many can actually spawn (respect maxCapacity)
        int canSpawn = entry.enemyData.spawnAmount;
        if (entry.enemyData.maxCapacity > 0)
            canSpawn = Mathf.Min(entry.enemyData.spawnAmount, entry.enemyData.maxCapacity - activeCount);

        if (canSpawn <= 0) return;

        for (int i = 0; i < canSpawn; i++)
        {
            Vector2 spawnPos = GetRandomPositionOutsideCamera();
            GameObject enemyObj = entry.enemyPool.Get(spawnPos, Quaternion.identity);
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.pool = entry.enemyPool;
                enemyScript.spawner = this;
                enemyScript.currencyReference = currencyReference;
                enemyScript.playerStats = playerStats;
                enemyScript.targetPosition = GetRandomPointInsideTargetArea();
                activeEnemies.Add(enemyScript);
            }
        }
    }

    private float GetMaxInterval()
    {
        float max = 0f;
        foreach (var entry in currentLevelEnemies)
            max = Mathf.Max(max, entry.enemyData.spawnInterval);
        return max;
    }

    private Vector2 GetRandomPositionOutsideCamera()
    {
        Vector2 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (side)
        {
            case 0: // top
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y + camHeight / 2 + spawnBuffer);
                break;
            case 1: // bottom
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y - camHeight / 2 - spawnBuffer);
                break;
            case 2: // left
                spawnPos = new Vector2(camPos.x - camWidth / 2 - spawnBuffer,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
            case 3: // right
                spawnPos = new Vector2(camPos.x + camWidth / 2 + spawnBuffer,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
        }

        return spawnPos;
    }

    // Returns a random point inside the target area
    public Vector2 GetRandomPointInsideTargetArea()
    {
        Bounds bounds = targetArea.bounds;
        return new Vector2(Random.Range(bounds.min.x, bounds.max.x),
                           Random.Range(bounds.min.y, bounds.max.y));
    }

    // Called by Enemy when it dies or returns to pool
    public void RemoveEnemyFromActiveList(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }
    
    // Called when splitting enemies to add children to active list
    public void AddEnemyToActiveList(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
}
