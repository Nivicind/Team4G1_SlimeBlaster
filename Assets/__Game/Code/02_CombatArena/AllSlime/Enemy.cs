using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    public SOEnemyData enemyData;

    [Header("Movement")]
    public float moveSpeed = 2f;

     private float spawnIgnoreTime = 8f;
    
    [HideInInspector] public ObjectPool pool;
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public StoreCurrencyReference currencyReference;
    [HideInInspector] public PlayerStats playerStats;
    [HideInInspector] public Vector2 targetPosition;

    public int maxHealth { get; private set; }
    public int currentHealth { get; private set; }
    protected SlimeAnimation slimeAnim;
    
    private Camera mainCamera;
    private bool justSpawned = true;
    private Vector2 direction;
    private bool directionSet = false;
    protected PlayerCombatArena playerCombatArena; // 🎮 Cached reference for healing on kill

    protected virtual void OnEnable()
    {
        mainCamera = Camera.main;
        slimeAnim = GetComponent<SlimeAnimation>();
        
        // 🎮 Auto-find PlayerCombatArena for heal on kill
        if (playerCombatArena == null)
            playerCombatArena = FindObjectOfType<PlayerCombatArena>();
        
        InitializeEnemy();
        justSpawned = true;
        directionSet = false;
        Invoke(nameof(DisableJustSpawned), spawnIgnoreTime);
    }

    protected virtual void OnDisable()
    {
        
        ReturnToPool();
    }

    protected virtual void Update()
    {
        MoveToTarget();
        CheckOffscreen();
    }

    protected void InitializeEnemy()
    {
        if (enemyData != null)
        {
            int stage = Stage.Instance.GetStage();
            float healthMult = GameConfig.Instance != null ? GameConfig.Instance.GetEnemyHealthMultiplier(stage) : 1f;
            maxHealth = Mathf.RoundToInt(enemyData.hp * healthMult);
            currentHealth = maxHealth;
        }
    }

    private void MoveToTarget()
    {
        if (!directionSet)
        {
            direction = (targetPosition - (Vector2)transform.position).normalized;
            directionSet = true;
        }
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

    protected void ReturnToPool()
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
        // Return multiplier scaled by stage config
        int stage = Stage.Instance.GetStage();
        float reflectMult = GameConfig.Instance != null ? GameConfig.Instance.GetEnemyReflectionMultiplier(stage) : 1f;
        float baseMultiplier = enemyData.baseReflectionMultiplier * reflectMult;
        
        // 😠 Check for angry multiplier (Boss with PinkSlimeAnimation)
        PinkSlimeAnimation pinkAnim = slimeAnim as PinkSlimeAnimation;
        if (pinkAnim != null && pinkAnim.IsAngry())
        {
            baseMultiplier *= pinkAnim.GetAngryReflectionMultiplier();
        }
        
        return baseMultiplier;
    }

    protected virtual void Die()
    {
        slimeAnim.PlayDeathAnimation(() => {
            // Delayed actions after animation completes
            GiveExpToPlayer();
            SpawnCurrency();
            HealPlayerOnKill();
            ReturnToPool();
        });
    }

    /// <summary>
    /// 💚 Heal player when this enemy is killed
    /// </summary>
    protected void HealPlayerOnKill()
    {
        if (playerStats == null || playerCombatArena == null) return;
        
        int healAmount = playerStats.GetStatValue(EnumStat.addHealthPerEnemyKill);
        if (healAmount > 0)
        {
            playerCombatArena.HealPlayer(healAmount);
        }
    }

    protected void GiveExpToPlayer()
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

        int stage = Stage.Instance.GetStage();
        float currencyMult = GameConfig.Instance != null ? GameConfig.Instance.GetEnemyCurrencyMultiplier(stage) : 1f;
        int baseAmount = enemyData.baseCurrencyAmount;
        
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
                case EnumCurrency.greenBits:
                    additionalAmount = spawner.playerStats.GetStatValue(EnumStat.additionalGreenBitsDropPerEnemy);
                    break;
            }
        }
        
        // Apply multiplier AFTER all bonuses: (base + bonus) * multiplier
        int totalAmount = Mathf.RoundToInt((baseAmount + additionalAmount) * currencyMult);
        
        // Get collider bounds
        Collider2D col = GetComponent<Collider2D>();
        
        for (int i = 0; i < totalAmount; i++)
        {
            Vector3 spawnPos;
            
            if (col != null)
            {
                // Spawn random position inside collider bounds
                Bounds bounds = col.bounds;
                spawnPos = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    transform.position.z
                );
            }
            else
            {
                // Fallback if no collider
                spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            }
            
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
