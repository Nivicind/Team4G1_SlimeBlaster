using UnityEngine;
using UnityEngine.UI;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Reference")]
    public GameObject bossEnemy;        // The boss GameObject (hidden at start)
    public StoreCurrencyReference currencyReference;  // Currency pools reference
    
    [Header("Path Points")]
    public Transform[] pointListA;      // List A of movement points
    public Transform[] pointListB;      // List B of movement points
    
    [Header("Spawn Settings")]
    public float spawnTime = 30f;       // Time before boss spawns
    public float moveSpeed = 3f;        // Boss movement speed
    public float spawnOffsetX = 3f;     // Horizontal distance outside camera to spawn boss
    public float spawnOffsetY = 3f;     // Vertical distance outside camera to spawn boss

    [Header("Progress Bar")]
    public Image progressBar;           // Progress bar fill image
    
    [Header("Debug Visualization")]
    public bool showSpawnGizmos = true; // Show spawn zones in Scene view
    public Color gizmoColor = Color.magenta;  // Color for boss spawn zone gizmos

    private Camera mainCamera;
    private float timer = 0f;
    private bool bossSpawned = false;
    private Vector3 moveDirection;
    private float bossLifeTime = 180f;  // 3 minutes in seconds
    private float bossSpawnedTime = 0f;
    private Vector3 bossTargetPos;
    private bool isMovingFromAToB = true;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        
        // Reset timer and flags
        timer = 0f;
        bossSpawned = false;
        
        // If not newest stage (boss already defeated), fill progress bar and skip spawn
        if (Stage.Instance != null && Stage.Instance.GetStage() != Stage.Instance.GetUnlockedStage())
        {
            bossSpawned = true;
            if (progressBar != null)
                progressBar.fillAmount = 1f;
        }
        
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
        // Only spawn boss on newest stage (current stage == unlocked stage)
        if (Stage.Instance != null && Stage.Instance.GetStage() != Stage.Instance.GetUnlockedStage())
        {
            // Not newest stage, boss already defeated - fill bar and skip
            if (progressBar != null)
                progressBar.fillAmount = 1f;
            bossSpawned = true;
            return;
        }

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
        // Check level manager
        if (LevelManager.Instance != null && !LevelManager.Instance.ShouldBossSpawn())
        {
            Debug.Log("BossSpawner: Boss not spawning - not current level");
            return;
        }

        if (bossEnemy == null || pointListA == null || pointListB == null) return;
        if (pointListA.Length == 0 || pointListB.Length == 0) return;
        
        // Show boss first
        bossEnemy.SetActive(true);
        
        // Randomly choose to move A->B or B->A
        isMovingFromAToB = Random.value > 0.5f;
        
        Transform startPoint, endPoint;
        
        if (isMovingFromAToB)
        {
            // Pick random from list A as start, random from list B as end
            startPoint = pointListA[Random.Range(0, pointListA.Length)];
            endPoint = pointListB[Random.Range(0, pointListB.Length)];
        }
        else
        {
            // Pick random from list B as start, random from list A as end
            startPoint = pointListB[Random.Range(0, pointListB.Length)];
            endPoint = pointListA[Random.Range(0, pointListA.Length)];
        }
        
        bossEnemy.transform.position = startPoint.position;
        bossTargetPos = endPoint.position;
        
        // Calculate move direction
        moveDirection = (bossTargetPos - bossEnemy.transform.position).normalized;
        
        // Assign currency reference to boss
        Boss bossScript = bossEnemy.GetComponent<Boss>();
        if (bossScript != null)
        {
            bossScript.currencyReference = currencyReference;
            bossScript.SetTarget(bossTargetPos);
        }
        
        Debug.Log($"Boss spawned at {startPoint.position}, moving toward {bossTargetPos}");
    }

    private void MoveBossToTarget()
    {
        if (bossEnemy == null || !bossEnemy.activeInHierarchy) return;
        
        // Move toward target
        bossEnemy.transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Check if reached target
        float distanceToTarget = Vector3.Distance(bossEnemy.transform.position, bossTargetPos);
        if (distanceToTarget < 0.5f)
        {
            // Boss reached the target, disable it
            DestroyBoss();
        }
        
        // Debug to verify movement
        Debug.DrawRay(bossEnemy.transform.position, moveDirection * 2f, Color.red);
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

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (side)
        {
            case 0: // top
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y + camHeight / 2 + spawnOffsetY);
                break;
            case 1: // bottom
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y - camHeight / 2 - spawnOffsetY);
                break;
            case 2: // left
                spawnPos = new Vector2(camPos.x - camWidth / 2 - spawnOffsetX,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
            case 3: // right
                spawnPos = new Vector2(camPos.x + camWidth / 2 + spawnOffsetX,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
        }

        return spawnPos;
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpawnGizmos || mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        Vector2 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float offsetX = spawnOffsetX;
        float offsetY = spawnOffsetY;
        
        // Draw camera border in white
        Gizmos.color = Color.white;
        Vector3 topLeft = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 topRight = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 bottomLeft = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomRight = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw spawn zones with custom color
        Gizmos.color = gizmoColor;
        
        // Top spawn zone (uses Y offset)
        Vector3 topSpawnTL = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2 + offsetY, 0);
        Vector3 topSpawnTR = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2 + offsetY, 0);
        Vector3 topSpawnBL = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 topSpawnBR = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Gizmos.DrawLine(topSpawnTL, topSpawnTR);
        Gizmos.DrawLine(topSpawnTR, topSpawnBR);
        Gizmos.DrawLine(topSpawnBR, topSpawnBL);
        Gizmos.DrawLine(topSpawnBL, topSpawnTL);
        
        // Bottom spawn zone (uses Y offset)
        Vector3 bottomSpawnTL = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomSpawnTR = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomSpawnBL = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2 - offsetY, 0);
        Vector3 bottomSpawnBR = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2 - offsetY, 0);
        Gizmos.DrawLine(bottomSpawnTL, bottomSpawnTR);
        Gizmos.DrawLine(bottomSpawnTR, bottomSpawnBR);
        Gizmos.DrawLine(bottomSpawnBR, bottomSpawnBL);
        Gizmos.DrawLine(bottomSpawnBL, bottomSpawnTL);
        
        // Left spawn zone (uses X offset)
        Vector3 leftSpawnTL = new Vector3(camPos.x - camWidth / 2 - offsetX, camPos.y + camHeight / 2, 0);
        Vector3 leftSpawnTR = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 leftSpawnBL = new Vector3(camPos.x - camWidth / 2 - offsetX, camPos.y - camHeight / 2, 0);
        Vector3 leftSpawnBR = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Gizmos.DrawLine(leftSpawnTL, leftSpawnTR);
        Gizmos.DrawLine(leftSpawnTR, leftSpawnBR);
        Gizmos.DrawLine(leftSpawnBR, leftSpawnBL);
        Gizmos.DrawLine(leftSpawnBL, leftSpawnTL);
        
        // Right spawn zone (uses X offset)
        Vector3 rightSpawnTL = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 rightSpawnTR = new Vector3(camPos.x + camWidth / 2 + offsetX, camPos.y + camHeight / 2, 0);
        Vector3 rightSpawnBL = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 rightSpawnBR = new Vector3(camPos.x + camWidth / 2 + offsetX, camPos.y - camHeight / 2, 0);
        Gizmos.DrawLine(rightSpawnTL, rightSpawnTR);
        Gizmos.DrawLine(rightSpawnTR, rightSpawnBR);
        Gizmos.DrawLine(rightSpawnBR, rightSpawnBL);
        Gizmos.DrawLine(rightSpawnBL, rightSpawnTL);
    }
}
