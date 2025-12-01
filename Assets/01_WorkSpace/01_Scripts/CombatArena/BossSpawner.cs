using UnityEngine;
using UnityEngine.UI;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Reference")]
    public GameObject bossEnemy;        // The boss GameObject (hidden at start)
    public StoreCurrencyReference currencyReference;  // Currency pools reference
    
    [Header("Target")]
    public GameObject targetPosition;   // Where the boss moves to
    
    [Header("Spawn Settings")]
    public float spawnTime = 30f;       // Time before boss spawns
    public float moveSpeed = 3f;        // Boss movement speed

    [Header("Progress Bar")]
    public Image progressBar;           // Progress bar fill image

    private Camera mainCamera;
    private float timer = 0f;
    private bool bossSpawned = false;
    private Vector3 moveDirection;
    private float bossLifeTime = 180f;  // 3 minutes in seconds
    private float bossSpawnedTime = 0f;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        
        // Reset timer and flags
        timer = 0f;
        bossSpawned = false;
        
        // Hide boss at start
        if (bossEnemy != null)
        {
            bossEnemy.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Hide boss when this spawner is disabled
        if (bossEnemy != null)
        {
            bossEnemy.SetActive(false);
        }
        
        // Reset state
        timer = 0f;
        bossSpawned = false;
    }

    private void Update()
    {
        if (!bossSpawned)
        {
            timer += Time.deltaTime;
            
            // Update progress bar fill amount
            if (progressBar != null)
            {
                progressBar.fillAmount = timer / spawnTime;
            }
            
            if (timer >= spawnTime)
            {
                SpawnBoss();
                bossSpawned = true;
                bossSpawnedTime = Time.time;
            }
        }
        else
        {
            // Check if boss should be destroyed after 3 minutes
            if (Time.time - bossSpawnedTime >= bossLifeTime)
            {
                DestroyBoss();
            }
        }
    }

    private void SpawnBoss()
    {
        if (bossEnemy == null || targetPosition == null) return;
        
        // Show boss first
        bossEnemy.SetActive(true);
        
        // Get random position outside camera
        Vector2 spawnPos = GetRandomPositionOutsideCamera();
        bossEnemy.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        
        // Assign currency reference and target to boss
        Boss bossScript = bossEnemy.GetComponent<Boss>();
        if (bossScript != null)
        {
            bossScript.currencyReference = currencyReference;
            bossScript.SetTarget(targetPosition.transform.position);
        }
        
        Debug.Log($"Boss spawned at {spawnPos}, moving toward {targetPosition.transform.position}");
    }

    private void MoveBossToTarget()
    {
        if (bossEnemy == null || !bossEnemy.activeInHierarchy) return;
        
        Vector3 oldPos = bossEnemy.transform.position;
        
        // Keep moving in straight line
        bossEnemy.transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        Vector3 newPos = bossEnemy.transform.position;
        
        // Debug to verify movement
        Debug.DrawRay(bossEnemy.transform.position, moveDirection * 2f, Color.red);
        Debug.DrawLine(oldPos, newPos, Color.green);
    }

    private void DestroyBoss()
    {
        if (bossEnemy != null)
        {
            bossEnemy.SetActive(false);
            Debug.Log("Boss destroyed after 3 minutes!");
        }
    }

    private Vector2 GetRandomPositionOutsideCamera()
    {
        Vector2 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float buffer = 1f;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (side)
        {
            case 0: // top
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y + camHeight / 2 + buffer);
                break;
            case 1: // bottom
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y - camHeight / 2 - buffer);
                break;
            case 2: // left
                spawnPos = new Vector2(camPos.x - camWidth / 2 - buffer,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
            case 3: // right
                spawnPos = new Vector2(camPos.x + camWidth / 2 + buffer,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
        }

        return spawnPos;
    }
}
