using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatArena : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerCombatUI playerUI;
    [SerializeField] private PlayerEffect playerEffect;

    [Header("Attack Settings")]
    [SerializeField] private Vector2 baseAttackRange = new Vector2(15f, 15f);
    [SerializeField] private LayerMask enemyLayer;
    
    // 📐 Pre-calculated attack size multipliers (+10% per level for X and Y)
    // Formula: 1 + level × 0.1
    // Level 0 = 100%, Level 14 = 240%
    private static readonly float[] attackSizeMultipliers = new float[]
    {
        1.0f,  // Level 0:  100%
        1.1f,  // Level 1:  110%
        1.2f,  // Level 2:  120%
        1.3f,  // Level 3:  130%
        1.4f,  // Level 4:  140%
        1.5f,  // Level 5:  150%
        1.6f,  // Level 6:  160%
        1.7f,  // Level 7:  170%
        1.8f,  // Level 8:  180%
        1.9f,  // Level 9:  190%
        2.0f,  // Level 10: 200%
        2.1f,  // Level 11: 210%
        2.2f,  // Level 12: 220%
        2.3f,  // Level 13: 230%
        2.4f   // Level 14: 240%
    };

    [Header("Currency Collection")]
    [SerializeField] private LayerMask currencyLayer;
    [SerializeField] private float currencyPickupRadius = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;     // Speed for following mouse/finger
    [SerializeField] private float mouseOffset = 1f;    // Player stays this much higher than mouse/touch
    
    [Header("Movement Bounds")]
    [SerializeField] private bool limitMovement = true;  // Enable/disable movement limits
    [SerializeField] private Vector2 boundsCenter = Vector2.zero;  // Center of the movement area
    [SerializeField] private Vector2 boundsSize = new Vector2(10f, 18f);  // Width and Height of allowed area
    [SerializeField] private bool showBoundsGizmo = true;  // Show bounds in Scene view
    [SerializeField] private Color boundsGizmoColor = Color.green;  // Gizmo color

    [Header("Debug")]

    [SerializeField] private GameObject Sprite;
    [SerializeField] private SpriteRenderer rend;
    
    [SerializeField, TextArea(20, 20)] private string DebugField = "";
    // 👑 Boss resolved from container
    private Boss bossEnemy;

    // 🎮 Combat Arena Temp Stats
    private int currentHp;
    private int currentExp;
    private Camera mainCamera;
    private bool isDead = false;

    // 💰 Track collected currency during this run
    private Dictionary<EnumCurrency, int> collectedCurrency = new Dictionary<EnumCurrency, int>();

    private void OnEnable() 
    {
        mainCamera = Camera.main;
        
        // 🔍 Find boss (including inactive objects)
        if (bossEnemy == null)
        {
            bossEnemy = FindObjectOfType<Boss>(true);
            bossEnemy.isDefeated = false;
        }
        
        // 🔄 Reset player position and state
        transform.position = Vector3.zero;
        isDead = false;
        
        // 💸 Reset collected currency
        collectedCurrency.Clear();
        
        // 📋 Initialize current stats from PlayerStats
        if (playerStats != null)
        {
            int baseHpValue = playerStats.GetStatValue(EnumStat.baseHp);
            int hpValue = playerStats.GetStatValue(EnumStat.hp);
            currentHp = baseHpValue + hpValue;
            currentExp = playerStats.GetStatValue(EnumStat.exp);
            
            // 📐 Apply attack size count (0-14) - swaps sprites instead of scaling
            int attackSizeCount = playerStats.GetStatValue(EnumStat.attackSizeCount);
            Debug.Log($"🎮 PlayerCombatArena: attackSizeCount = {attackSizeCount}, playerEffect = {playerEffect != null}");
            if (playerEffect != null)
            {
                playerEffect.SetAttackSizeLevel(attackSizeCount);
            }
            else
            {
                Debug.LogError("❌ playerEffect is NULL! Assign it in Inspector!");
            }
        }
        
        StartCoroutine(AttackRoutine());
        StartCoroutine(HpLossRoutine());
    }

    private void OnDisable()
    {
        // 🚫 Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // ⏹️ Stop all coroutines
        StopAllCoroutines();
    }
    private void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            CheckCurrencyPickup();
            CheckBossDefeat();
        }
    }

    private void CheckBossDefeat()
    {
        // 🏆 Check if boss is defeated
        if (bossEnemy != null && bossEnemy.isDefeated)
        {
            ShowWin();
            bossEnemy = null; // ✅ Prevent checking multiple times
        }
    }

    private void ShowWin()
    {
        isDead = true;
        
        // 🚫 Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // 🔓 Unlock 1 new stage when boss is defeated
        if (Stage.Instance != null)
        {
            Stage.Instance.UnlockStages(1);
        }
        
        // 💎 Give player 1 pink bit as reward
        if (playerStats != null)
        {
            playerStats.AddCurrency(EnumCurrency.pinkBits, 1);
            
            // Track in collected currency for UI display
            if (!collectedCurrency.ContainsKey(EnumCurrency.pinkBits))
                collectedCurrency[EnumCurrency.pinkBits] = 0;
            collectedCurrency[EnumCurrency.pinkBits] += 1;
        }
        
        // 💰 Save all currencies when game ends
        if (SaveSystem.Instance != null && playerStats != null)
            SaveSystem.Instance.SaveAllCurrenciesFromPlayerStats(playerStats);
        
        // 🎉 Show win UI
        if (playerUI != null)
            playerUI.ShowWin();
        
        Debug.Log("Player won!");
    }

    private void CheckCurrencyPickup()
    {
        // 🧲 Calculate actual pickup radius with bonus from stats
        int radiusIncreasePercent = playerStats.GetStatValue(EnumStat.currencyPickupRadiusIncreasePercent);
        float actualPickupRadius = currencyPickupRadius * (1f + radiusIncreasePercent / 100f);
        
        // 💰 Detect all currency within pickup radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, actualPickupRadius, currencyLayer);

        foreach (var hit in hits)
        {
            CurrencyControl currency = hit.GetComponent<CurrencyControl>();
            if (currency != null && !currency.IsFlying())
            {
                // 📊 Track collected currency for this run
                EnumCurrency currencyType = currency.currencyType;
                if (!collectedCurrency.ContainsKey(currencyType))
                    collectedCurrency[currencyType] = 0;
                collectedCurrency[currencyType] += currency.currencyAmount;
                
                // 🚀 Start flying towards player (currency will be added after 1 second)
                currency.StartFlyingToPlayer(transform, playerStats);
            }
        }
    }
    
    private void HandleMovement()
    {
        Vector3 targetPos = transform.position;

        // 🎮 Use PlayerInputHandler singleton for input
        if (PlayerInputHandler.Instance.IsInputActive())
        {
            targetPos = PlayerInputHandler.Instance.GetInputWorldPosition();
            targetPos.y += mouseOffset; // 📐 Player stays higher than input
        }

        // 🛡️ Clamp target position within movement bounds
        if (limitMovement)
        {
            float minX = boundsCenter.x - boundsSize.x / 2f;
            float maxX = boundsCenter.x + boundsSize.x / 2f;
            float minY = boundsCenter.y - boundsSize.y / 2f;
            float maxY = boundsCenter.y + boundsSize.y / 2f;
            
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        // 🎯 Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// ⚔️ Attack Routine - Continuously attacks based on secondPerAttack stat
    /// 
    /// 📊 How secondPerAttack works:
    /// - secondPerAttack = 2  → Attack every 2.0 seconds
    /// - secondPerAttack = 1  → Attack every 1.0 second
    /// - secondPerAttack = 3  → Attack every 3.0 seconds
    /// 
    /// 📊 How additionalAttackSpeedIncreasePercent works:
    /// - 0%   → No change (2 sec stays 2 sec)
    /// - 50%  → 50% faster (2 × 0.5 = 1 sec)
    /// - 75%  → 75% faster (2 × 0.25 = 0.5 sec)
    /// 
    /// 🔢 Formula: waitTime = secondPerAttack × (1 - bonus% / 100)
    /// Higher bonus% = faster attacks!
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            // 🎮 Get base secondPerAttack from PlayerStats
            float baseSeconds = playerStats.GetStatValue(EnumStat.secondPerAttack);
            
            // ⚔️ Apply additionalAttackSpeedIncreasePercent to REDUCE wait time
            // Formula: effectiveTime = baseSeconds × (1 - bonus% / 100)
            // Example: 50% bonus → 2 × 0.5 = 1 sec, 75% bonus → 2 × 0.25 = 0.5 sec
            float bonusPercent = playerStats.GetStatValue(EnumStat.additionalAttackSpeedIncreasePercent);
            float multiplier = Mathf.Max(0.01f, 1f - bonusPercent / 100f);  // Clamp to min 0.01 (99% max reduction)
            float effectiveSeconds = baseSeconds * multiplier;
            
            // ⏱️ Wait time between attacks
            yield return new WaitForSeconds(Mathf.Max(0.05f, effectiveSeconds));  // Min 0.05 sec
            
            if (!isDead)
                Attack();
        }
    }

    private IEnumerator HpLossRoutine()
    {
        while (true)
        {
            float interval = GameConfig.Instance != null ? GameConfig.Instance.hpLossInterval : 1f;
            yield return new WaitForSeconds(interval);
            
            if (!isDead && playerStats != null)
            {
                int hpLoss = playerStats.GetStatValue(EnumStat.hpLossPerSecond);
                if (hpLoss > 0)
                {
                    TakeDamage(hpLoss);
                }
            }
        }
    }

    private void Attack()
    {
        // 📷 Camera shake on attack (uses Inspector values from CameraShakeManager)
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.Shake(); // Uses defaultDuration & defaultIntensity from Inspector
        }
        else
        {
            Debug.LogWarning("⚠️ CameraShakeManager.Instance is NULL! Add CameraShakeManager to scene!");
        }
        
        // 📐 Get attack size count (0-14) and calculate actual attack range
        // Uses √(1 + count × 0.1) to increase AREA by 10% per count
        int attackSizeCount = Mathf.Clamp(playerStats.GetStatValue(EnumStat.attackSizeCount), 0, 14);
        float sizeMultiplier = attackSizeMultipliers[attackSizeCount];
        Vector2 actualAttackRange = baseAttackRange * sizeMultiplier;
        
        // 🎯 Detect all 2D colliders inside the box
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, actualAttackRange, 0f, enemyLayer);

        int baseDamageValue = playerStats.GetStatValue(EnumStat.baseDamage);
        int damageValue = playerStats.GetStatValue(EnumStat.damage);
        int totalBaseDamage = baseDamageValue + damageValue;
        
        int baseReflection = playerStats.GetStatValue(EnumStat.baseReflection);
        int additionalDamagePerEnemyPercent = playerStats.GetStatValue(EnumStat.additionalDamagePerEnemyInAreaPercent);
        float totalMultiplier = 0;

        // 🔢 Count valid enemies hit for damage calculation
        int enemyCount = 0;
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemyCount++;
            }
        }

        // 🧮 Calculate final damage with additionalDamagePerEnemyInAreaPercent
        // 📝 Formula: (baseDamage + damage) * (1 + (additionalDamagePerEnemyPercent * enemyCount / 100))
        float damageMultiplier = 1f + (additionalDamagePerEnemyPercent * enemyCount / 100f);
        int finalDamage = Mathf.RoundToInt(totalBaseDamage * damageMultiplier);

        // 🎲 Critical Hit Calculation
        // critRatePercent = chance to crit (5 = 5% chance)
        // critDamagePercent = bonus damage on crit (150 = +150% = 250% total damage)
        int critRatePercent = playerStats.GetStatValue(EnumStat.critRatePercent);
        int critDamagePercent = playerStats.GetStatValue(EnumStat.critDamagePercent);
        bool isCriticalHit = Random.Range(0, 100) < critRatePercent;
        
        if (isCriticalHit)
        {
            // 💥 Apply crit bonus: finalDamage * (100 + critDamagePercent) / 100
            // Example: 150% crit damage = 100 + 150 = 250% = 2.5x damage
            float critMultiplier = (100f + critDamagePercent) / 100f;
            finalDamage = Mathf.RoundToInt(finalDamage * critMultiplier);
            Debug.Log($"💥 CRITICAL HIT! Damage boosted to {finalDamage} ({critMultiplier:F2}x)");
        }

        bool hitBoss = false;
        int damageDealtToSingleEnemy = 0;
        int totalDamageDealtToAllEnemies = 0;
        
        foreach (var hit in hits)
        {
            // ⚔️ Apply damage if enemy has Enemy script
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 👑 Check if this is a boss and apply boss damage bonus
                if (enemy is Boss)
                {
                    hitBoss = true;
                    int bossDamage = playerStats.GetStatValue(EnumStat.bossDamage);
                    int bossFinalDamage = finalDamage + bossDamage;
                    enemy.TakeDamage(bossFinalDamage);
                    damageDealtToSingleEnemy = bossFinalDamage; // 💾 Store damage dealt to boss
                    totalDamageDealtToAllEnemies += bossFinalDamage; // 📊 Accumulate total damage
                }
                else
                {
                    enemy.TakeDamage(finalDamage);
                    if (damageDealtToSingleEnemy == 0) // 💾 Store first normal enemy damage
                    {
                        damageDealtToSingleEnemy = finalDamage;
                    }
                    totalDamageDealtToAllEnemies += finalDamage; // 📊 Accumulate total damage
                }
                
                // 🔄 Get multiplier from each enemy
                totalMultiplier += enemy.GetEnemyMultiplierBaseReflection();
            }
        }

        // 🔮 Calculate reflection damage: baseReflection * totalMultiplier
        float reflectionDamage = baseReflection * totalMultiplier;
        int damageTakenFromBoss = 0;
        int damageTakenFromEnemy = 0;
        int damageTakenFromSingleEnemy = 0;
        
        // 🎯 Calculate reflection damage separately for boss and normal enemies
        float bossReflectionDamage = 0;
        float normalEnemyReflectionDamage = 0;
        int bossCount = 0;
        int normalEnemyCount = 0;
        
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                float enemyReflection = baseReflection * enemy.GetEnemyMultiplierBaseReflection();
                
                if (enemy is Boss)
                {
                    bossReflectionDamage += enemyReflection;
                    bossCount++;
                }
                else
                {
                    normalEnemyReflectionDamage += enemyReflection;
                    normalEnemyCount++;
                }
            }
        }
        
        // 💥 Take damage from boss reflection if hit boss
        if (bossReflectionDamage > 0)
        {
            damageTakenFromBoss = TakeDamageFromBoss((int)bossReflectionDamage);
        }
        
        // 💥 Take damage from normal enemy reflection if hit enemies
        if (normalEnemyReflectionDamage > 0)
        {
            damageTakenFromEnemy = TakeDamage((int)normalEnemyReflectionDamage);
        }
        
        // 🧮 Calculate average damage from single enemy
        int totalDamageTaken = damageTakenFromBoss + damageTakenFromEnemy;
        damageTakenFromSingleEnemy = enemyCount > 0 ? Mathf.RoundToInt((float)totalDamageTaken / enemyCount) : 0;
        // 💚 Heal HP based on enemies hit: addHealthPerEnemyHit * enemyCount
        int healPerHit = playerStats.GetStatValue(EnumStat.addHealthPerEnemyHit);
        if (healPerHit > 0 && enemyCount > 0)
        {
            int totalHeal = healPerHit * enemyCount;
            currentHp += totalHeal;
            
            // 📊 Cap HP at max HP
            int baseHpValue = playerStats.GetStatValue(EnumStat.baseHp);
            int hpValue = playerStats.GetStatValue(EnumStat.hp);
            int maxHp = baseHpValue + hpValue;
            currentHp = Mathf.Min(currentHp, maxHp);
        }
        // 📊 Update debug info
        UpdateDebugField(enemyCount, damageDealtToSingleEnemy, totalDamageDealtToAllEnemies, damageTakenFromBoss, damageTakenFromEnemy, damageTakenFromSingleEnemy);

        // 🔊 Play laser attack sound
        GlobalSoundManager.PlaySound(SoundType.laserAttack);

        playerEffect.PlayerAttackEffect();
    }
    public int TakeDamage(int damage)
    {
        // 🛡️ Apply armor reduction (baseArmor + armor)
        int baseArmorValue = playerStats.GetStatValue(EnumStat.baseArmor);
        int armorValue = playerStats.GetStatValue(EnumStat.armor);
        int totalArmor = baseArmorValue + armorValue;
        int finalDamage = Mathf.Max(1, damage - totalArmor); // ⚠️ Minimum 1 damage
        
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp); // Prevent negative HP
        
        if (currentHp <= 0)
        {
            Die();
        }
        
        return finalDamage; // Return actual damage taken
    }
    
    public int TakeDamageFromBoss(int damage)
    {
        // 🛡️ Apply baseArmor + armor + bossArmor reduction when taking reflection damage from boss
        int baseArmorValue = playerStats.GetStatValue(EnumStat.baseArmor);
        int armorValue = playerStats.GetStatValue(EnumStat.armor);
        int bossArmor = playerStats.GetStatValue(EnumStat.bossArmor);
        int totalArmor = baseArmorValue + armorValue + bossArmor;
        int finalDamage = Mathf.Max(1, damage - totalArmor); // ⚠️ Minimum 1 damage
        
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp); // Prevent negative HP
        
        if (currentHp <= 0)
        {
            Die();
        }
        
        return finalDamage; // ↩️ Return actual damage taken
    }
    
    // 📊 Cached debug values to preserve non-zero stats
    private int lastEnemyCount = 0;
    private int lastDamagePerEnemy = 0;
    private int lastTotalDamageToAllEnemies = 0;
    private int lastDamageTakenFromBoss = 0;
    private int lastDamageTakenFromEnemy = 0;
    private int lastDamageTakenFromSingleEnemy = 0;
    
    /// <summary>
    /// 📊 Updates the DebugField with detailed combat statistics
    /// Shows: enemies hit, damage dealt, and damage taken breakdown
    /// </summary>
    /// <param name="enemyCount">👥 Total number of enemies hit in the attack</param>
    /// <param name="damagePerEnemy">⚔️ Damage dealt to a single enemy</param>
    /// <param name="totalDamageToAllEnemies">💥 Total damage dealt to all enemies</param>
    /// <param name="damageTakenFromBoss">🦴 Total damage taken from boss reflection</param>
    /// <param name="damageTakenFromEnemy">👹 Total damage taken from normal enemy reflection</param>
    /// <param name="damageTakenFromSingleEnemy">💔 Average damage taken from one enemy</param>
    private void UpdateDebugField(int enemyCount, int damagePerEnemy, int totalDamageToAllEnemies, int damageTakenFromBoss, int damageTakenFromEnemy, int damageTakenFromSingleEnemy)
    {
        // 🔄 Keep previous values if current values are 0
        if (enemyCount != 0) lastEnemyCount = enemyCount;
        if (damagePerEnemy != 0) lastDamagePerEnemy = damagePerEnemy;
        if (totalDamageToAllEnemies != 0) lastTotalDamageToAllEnemies = totalDamageToAllEnemies;
        if (damageTakenFromBoss != 0) lastDamageTakenFromBoss = damageTakenFromBoss;
        if (damageTakenFromEnemy != 0) lastDamageTakenFromEnemy = damageTakenFromEnemy;
        if (damageTakenFromSingleEnemy != 0) lastDamageTakenFromSingleEnemy = damageTakenFromSingleEnemy;
        
        // 🎯 Build debug string with detailed combat statistics
        DebugField = $"👥 Enemies Hit: {lastEnemyCount}\n" +
                     $"⚔️ Damage to 1 Enemy: {lastDamagePerEnemy}\n" +
                     $"💥 Total Damage to All: {lastTotalDamageToAllEnemies}\n" +
                     $"🦴 Damage taken from Boss: {lastDamageTakenFromBoss}\n" +
                     $"👹 Damage taken from all Enemies: {lastDamageTakenFromEnemy}\n" +
                     $"💔 Damage taken from 1 Enemy: {lastDamageTakenFromSingleEnemy}";
    }
    
    private void Die()
    {
        isDead = true;
                // 🏆 Check if boss is also defeated - if so, player wins!
        if (bossEnemy != null && bossEnemy.isDefeated)
        {
            ShowWin();
            return;
        }
                // � Save all currencies when game ends
        if (SaveSystem.Instance != null && playerStats != null)
            SaveSystem.Instance.SaveAllCurrenciesFromPlayerStats(playerStats);
        
        // �💀 Show lose UI
        if (playerUI != null)
            playerUI.ShowLose();
        
        // 🚫 Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        Debug.Log("Player died!");
    }

    // 📥 Getter methods for UI
    public int GetCurrentHp() => currentHp;
    public int GetCurrentExp() => currentExp;
    public Dictionary<EnumCurrency, int> GetCollectedCurrency() => collectedCurrency;

    /// <summary>
    /// 💚 Heal player by amount (capped at max HP)
    /// Called when enemy dies to apply addHealthPerEnemyKill
    /// </summary>
    public void HealPlayer(int amount)
    {
        if (isDead || amount <= 0) return;
        
        currentHp += amount;
        
        // 📊 Cap HP at max HP
        int baseHpValue = playerStats.GetStatValue(EnumStat.baseHp);
        int hpValue = playerStats.GetStatValue(EnumStat.hp);
        int maxHp = baseHpValue + hpValue;
        currentHp = Mathf.Min(currentHp, maxHp);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // �️ Draw movement bounds (green)
        if (showBoundsGizmo)
        {
            Gizmos.color = boundsGizmoColor;
            Vector3 center = new Vector3(boundsCenter.x, boundsCenter.y, 0);
            Vector3 size = new Vector3(boundsSize.x, boundsSize.y, 0);
            Gizmos.DrawWireCube(center, size);
            
            Color fillColor = boundsGizmoColor;
            fillColor.a = 0.1f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(center, size);
        }
        
        // �📦 Draw attack range (box)
        Gizmos.color = Color.red;
        Vector2 rangeToShow = baseAttackRange;
        if (Application.isPlaying && playerStats != null)
        {
            // Uses √(1 + count × 0.1) to increase AREA by 10% per count
            int attackSizeCount = Mathf.Clamp(playerStats.GetStatValue(EnumStat.attackSizeCount), 0, 14);
            float sizeMultiplier = attackSizeMultipliers[attackSizeCount];
            rangeToShow = baseAttackRange * sizeMultiplier;
        }
        Gizmos.DrawWireCube(transform.position, rangeToShow);
        
        // ⭕ Draw currency pickup range (circle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currencyPickupRadius);
    }
#endif
}
