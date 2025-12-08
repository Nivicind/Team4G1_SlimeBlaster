using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Data")]
    public SOEnemyData enemyData;

    [Header("Movement")]
    public float moveSpeed = 2f;
    
    [HideInInspector] public ObjectPool pool;
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public StoreCurrencyReference currencyReference;

    public int currentHealth { get; private set; }
    private Camera mainCamera;
    private bool justSpawned = true;
    private float spawnIgnoreTime = 3f;
    private Vector2 moveDirection;

    protected virtual void OnEnable()
    {
        mainCamera = Camera.main;
        InitializeEnemy();
        justSpawned = true;

        // Calculate direction toward center of target area, then move toward opposite side
        if (spawner != null)
        {
            Vector2 targetAreaCenter = spawner.targetArea.bounds.center;
            Vector2 directionToCenter = (targetAreaCenter - (Vector2)transform.position).normalized;
            
            // Pick a point on the opposite side of the target area
            Bounds bounds = spawner.targetArea.bounds;
            Vector2 oppositePoint = targetAreaCenter + directionToCenter * (bounds.extents.magnitude);
            
            moveDirection = (oppositePoint - (Vector2)transform.position).normalized;
        }
        else
        {
            // Fallback: random direction
            moveDirection = Random.insideUnitCircle.normalized;
        }

        Invoke(nameof(DisableJustSpawned), spawnIgnoreTime);
    }

    protected virtual void OnDisable()
    {
        ReturnToPool();
    }

    private void Update()
    {
        MoveStraight();
        CheckOffscreen();
    }

    protected void InitializeEnemy()
    {
        if (enemyData != null)
        {
            int level = Level.Instance.GetLevel();
            currentHealth = enemyData.hp * level;
        }
    }

    private void MoveStraight()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void CheckOffscreen()
    {
        if (justSpawned) return;

        if (!IsVisibleToCamera())
        {
            ReturnToPool();
        }
    }

    private void DisableJustSpawned() => justSpawned = false;

    private bool IsVisibleToCamera()
    {
        if (mainCamera == null) return true;
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        float buffer = 0.1f;
        return (viewPos.x >= -buffer && viewPos.x <= 1 + buffer &&
                viewPos.y >= -buffer && viewPos.y <= 1 + buffer);
    }

    private void ReturnToPool()
    {
        pool?.ReturnToPool(gameObject);
        spawner?.RemoveEnemyFromActiveList(this);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    public float GetEnemyMultiplierBaseReflection()
    {
        // Return multiplier scaled by level
        int level = Level.Instance.GetLevel();
        return enemyData.baseReflectionMultiplier * level;
    }

    protected virtual void Die()
    {
        SpawnCurrency();
        ReturnToPool();
    }

    protected void SpawnCurrency()
    {
        if (enemyData == null || currencyReference == null) return;

        // Get the correct currency pool from StoreCurrencyReference
        ObjectPool selectedPool = currencyReference.GetCurrencyPool(enemyData.currencyType);
        
        if (selectedPool == null) return;

        int level = Level.Instance.GetLevel();
        int amount = enemyData.baseCurrencyAmount * level;
        
        for (int i = 0; i < amount; i++)
        {
            // Spawn currency at enemy position with slight random offset
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            GameObject currencyObj = selectedPool.Get(spawnPos, Quaternion.identity);
            
            // Set the pool reference so it can return to pool
            CurrencyControl currencyControl = currencyObj.GetComponent<CurrencyControl>();
            if (currencyControl != null)
            {
                currencyControl.pool = selectedPool;
            }
        }
    }
}
