using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public ObjectPool enemyPool;           // Pool that contains prefab
    public PlayerStats playerStats;        // For spawn rate
    public BoxCollider2D targetArea;       // Area enemies move toward
    public StoreCurrencyReference currencyReference;  // Currency pools reference
    public List<SOEnemyData> enemiesToSpawn;

    [Header("Spawn Settings")]
    public float spawnBuffer = 0.5f;       // How far outside camera to spawn

    private Camera mainCamera;
    private float timer;
    private List<Enemy> activeEnemies = new List<Enemy>();

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        foreach (var data in enemiesToSpawn)
        {
            if (Time.timeSinceLevelLoad < data.startTime) continue;

            float adjustedInterval = data.spawnInterval / Mathf.Max(0.01f, playerStats.GetStatValue(EnumStat.spawnRatePercent) / 100f);
            if (timer >= adjustedInterval)
            {
                SpawnEnemyType(data);
            }
        }

        if (timer >= GetMaxInterval()) timer = 0f;
    }

    private void SpawnEnemyType(SOEnemyData data)
    {
        // Clean null or inactive references
        activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        // Count current active enemies of this type
        int activeCount = activeEnemies.FindAll(e => e.enemyData == data).Count;

        // Calculate how many can actually spawn (respect maxCapacity)
        int canSpawn = data.spawnAmount;
        if (data.maxCapacity > 0)
            canSpawn = Mathf.Min(data.spawnAmount, data.maxCapacity - activeCount);

        if (canSpawn <= 0) return;

        for (int i = 0; i < canSpawn; i++)
        {
            Vector2 spawnPos = GetRandomPositionOutsideCamera();
            GameObject enemyObj = enemyPool.Get(spawnPos, Quaternion.identity);
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.enemyData = data;
                enemyScript.pool = enemyPool;
                enemyScript.spawner = this;
                enemyScript.currencyReference = currencyReference;
                activeEnemies.Add(enemyScript);
            }
        }
    }

    private float GetMaxInterval()
    {
        float max = 0f;
        foreach (var data in enemiesToSpawn)
            max = Mathf.Max(max, data.spawnInterval);
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
}
