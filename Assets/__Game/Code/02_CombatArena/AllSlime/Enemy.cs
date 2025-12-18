using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    public SOEnemyData enemyData;

    [Header("Movement")]
    public float moveSpeed = 2f;
    
    [HideInInspector] public ObjectPool pool;
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public StoreCurrencyReference currencyReference;
    [HideInInspector] public PlayerStats playerStats;
    [HideInInspector] public Vector2 targetPosition;

    public int currentHealth { get; private set; }
    private Camera mainCamera;
    private SlimeAnimation slimeAnim;
    private bool justSpawned = true;
    private float spawnIgnoreTime = 3f;

    protected virtual void OnEnable()
    {
        mainCamera = Camera.main;
        slimeAnim = GetComponent<SlimeAnimation>();
        InitializeEnemy();
        justSpawned = true;
        Invoke(nameof(DisableJustSpawned), spawnIgnoreTime);
    }

    protected virtual void OnDisable()
    {
        ReturnToPool();
    }

    private void Update()
    {
        MoveToTarget();
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

    private void MoveToTarget()
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    protected void CheckOffscreen()
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
        
        // Play hurt animation
        if (slimeAnim != null)
            slimeAnim.Hurt();
        
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
        GiveExpToPlayer();
        SpawnCurrency();
        ReturnToPool();
    }

    private void GiveExpToPlayer()
    {
        if (enemyData == null || playerStats == null) return;

        int expReward = enemyData.exp;
        playerStats.AddStat(EnumStat.exp, expReward);
        Debug.Log($"Player gained {expReward} exp from enemy");
    }

    protected void SpawnCurrency()
    {
        if (enemyData == null || currencyReference == null) return;

        // Get the correct currency pool from StoreCurrencyReference
        ObjectPool selectedPool = currencyReference.GetCurrencyPool(enemyData.currencyType);
        
        if (selectedPool == null) return;

        int level = Level.Instance.GetLevel();
        int baseAmount = enemyData.baseCurrencyAmount * level;
        
        // Add additional currency drops based on the currency type
        int additionalAmount = 0;
        if (spawner != null && spawner.playerStats != null)
        {
            switch (enemyData.currencyType)
            {
                case EnumCurrency.blueBits:
                    additionalAmount = spawner.playerStats.GetStatValue(EnumStat.additionalBlueBitsDropPerEnemy);
                    break;
                case EnumCurrency.pinkBits:
                    additionalAmount = spawner.playerStats.GetStatValue(EnumStat.additionalPinkBitsDropPerEnemy);
                    break;
                case EnumCurrency.yellowBits:
                    additionalAmount = spawner.playerStats.GetStatValue(EnumStat.additionalYellowBitsDropPerEnemy);
                    break;
            }
        }
        
        int totalAmount = baseAmount + additionalAmount;
        
        for (int i = 0; i < totalAmount; i++)
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
