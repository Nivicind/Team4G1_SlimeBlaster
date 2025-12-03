using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatArena : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerCombatUI playerUI;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Currency Collection")]
    [SerializeField] private LayerMask currencyLayer;
    [SerializeField] private float currencyPickupRadius = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;     // Speed for following mouse/finger

    [SerializeField] private SpriteRenderer rend;

    // Boss resolved from container
    private Boss bossEnemy;

    // Combat Arena Temp Stats
    private int currentHp;
    private int currentExp;
    private Camera mainCamera;
    private bool isDead = false;

    // Track collected currency during this run
    private Dictionary<EnumCurrency, int> collectedCurrency = new Dictionary<EnumCurrency, int>();

    private void OnEnable() 
    {
        mainCamera = Camera.main;
        
        // Find boss (including inactive objects)
        if (bossEnemy == null)
        {
            bossEnemy = FindObjectOfType<Boss>(true);
            bossEnemy.isDefeated = false;
        }
        
        // Reset player position and state
        transform.position = Vector3.zero;
        isDead = false;
        
        // Reset collected currency
        collectedCurrency.Clear();
        
        // Initialize current stats from PlayerStats
        if (playerStats != null)
        {
            currentHp = playerStats.GetStatValue(EnumStat.hp);
            currentExp = playerStats.GetStatValue(EnumStat.exp);
        }
        
        StartCoroutine(AttackRoutine());
        StartCoroutine(HpLossRoutine());
    }

    private void OnDisable()
    {
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // Stop all coroutines
        StopAllCoroutines();
        
        if (rend != null)
        {
            Color color = rend.color;
            color.a = 0f;
            rend.color = color;
        }
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
        // Check if boss is defeated
        if (bossEnemy != null && bossEnemy.isDefeated)
        {
            ShowWin();
            bossEnemy = null; // Prevent checking multiple times
        }
    }

    private void ShowWin()
    {
        isDead = true;
        
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // Unlock 1 new level when boss is defeated
        if (Level.Instance != null)
        {
            Level.Instance.UnlockLevels(1);
        }
        
        // Show win UI
        if (playerUI != null)
            playerUI.ShowWin();
        
        Debug.Log("Player won!");
    }

    private void CheckCurrencyPickup()
    {
        // Detect all currency within pickup radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currencyPickupRadius, currencyLayer);

        foreach (var hit in hits)
        {
            CurrencyControl currency = hit.GetComponent<CurrencyControl>();
            if (currency != null && !currency.IsFlying())
            {
                // Track collected currency for this run
                EnumCurrency currencyType = currency.currencyType;
                if (!collectedCurrency.ContainsKey(currencyType))
                    collectedCurrency[currencyType] = 0;
                collectedCurrency[currencyType] += currency.currencyAmount;
                
                // Start flying towards player (currency will be added after 1 second)
                currency.StartFlyingToPlayer(transform, playerStats);
            }
        }
    }
    
    private void HandleMovement()
    {
        Vector3 targetPos = transform.position;

#if UNITY_EDITOR || UNITY_STANDALONE
        // PC: follow mouse
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            targetPos = mainCamera.ScreenToWorldPoint(mousePos);
            targetPos.z = 0f; // Keep player on Z=0 plane
        }
#elif UNITY_IOS || UNITY_ANDROID
        // Mobile: follow first touch
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = Input.GetTouch(0).position;
            touchPos.z = Mathf.Abs(mainCamera.transform.position.z);
            targetPos = mainCamera.ScreenToWorldPoint(touchPos);
            targetPos.z = 0f;
        }
#endif

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            float attackSpeed = Mathf.Max(playerStats.GetStatValue(EnumStat.attackSpeed), 0.01f);
            yield return new WaitForSeconds(1f / attackSpeed);
            
            if (!isDead)
                Attack();
        }
    }

    private IEnumerator HpLossRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
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
        // Detect all 2D colliders inside the box
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f, enemyLayer);

        int damage = playerStats.GetStatValue(EnumStat.damage);
        int baseReflection = playerStats.GetStatValue(EnumStat.baseReflection);
        float totalMultiplier = 0;

        foreach (var hit in hits)
        {
            // Apply damage if enemy has Enemy script
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                
                // Get multiplier from each enemy
                totalMultiplier += enemy.GetEnemyMultiplierBaseReflection();
            }
        }

        // Calculate reflection damage: baseReflection * totalMultiplier
        float reflectionDamage = baseReflection * totalMultiplier;
        if (reflectionDamage > 0)
        {
            TakeDamage((int)reflectionDamage);
        }

        StartCoroutine(FlashAlpha());
    }
    public void TakeDamage(int damage)
    {
        // Apply armor reduction
        int armor = playerStats.GetStatValue(EnumStat.armor);
        int finalDamage = Mathf.Max(1, damage - armor); // Minimum 1 damage
        
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp); // Prevent negative HP
        
        if (currentHp <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        isDead = true;
        
        // Show lose UI
        if (playerUI != null)
            playerUI.ShowLose();
        
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        Debug.Log("Player died!");
    }

    // Getter methods for UI
    public int GetCurrentHp() => currentHp;
    public int GetCurrentExp() => currentExp;
    public Dictionary<EnumCurrency, int> GetCollectedCurrency() => collectedCurrency;

    private IEnumerator FlashAlpha()
    {
        if (rend == null) yield break;

        Color original = rend.color;

        // Set alpha to 0.4 (keep original color)
        Color flashColor = new Color(original.r, original.g, original.b, 0.4f);
        rend.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        // Reset to original alpha
        rend.color = original;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw attack range (box)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw currency pickup range (circle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currencyPickupRadius);
    }
#endif
}
