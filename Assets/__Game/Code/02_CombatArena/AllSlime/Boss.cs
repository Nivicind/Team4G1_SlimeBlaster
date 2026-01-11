using UnityEngine;

public class Boss : Enemy
{
    public bool isDefeated = false;
    private bool isDying = false;  // 🚩 Flag to stop movement during death
    Vector2 bossDirection;
    
    // 🩷 Pink Slime Animation reference
    private PinkSlimeAnimation pinkSlimeAnim;

    protected override void OnEnable()
    {
        base.OnEnable();  // This initializes slimeAnim, playerCombatArena, and other base components
        isDefeated = false;
        isDying = false;  // ♻️ Reset dying flag on enable
        targetPosition = new Vector2(0, 0);
        
        // 🩷 Get PinkSlimeAnimation component
        pinkSlimeAnim = GetComponent<PinkSlimeAnimation>();
        
        // 🩷 Reset pink slime animation state
        if (pinkSlimeAnim != null)
            pinkSlimeAnim.ResetDeathState();
    }

    protected override void OnDisable()
    {
        // Don't call base.OnDisable() to prevent returning to pool
    }

    // Set target position (uses base Enemy's targetPosition)
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        bossDirection = ((Vector2)target - (Vector2)transform.position).normalized;
    }

    protected override void Update()
    {
        // 🚩 Don't move if dying
        if (isDying) return;
        
        // Always move in the locked direction
        transform.position += (Vector3)(bossDirection * moveSpeed * Time.deltaTime);
        CheckOffscreen();
    }

    protected override void Die()
    {
        // 🚩 Stop movement
        isDying = true;
        
        // Spawn currency
        SpawnCurrency();

        // 🩷 Play pink slime death animation if available
        if (pinkSlimeAnim != null)
        {
            pinkSlimeAnim.PlayPinkSlimeDeathAnimation(() =>
            {
                // ✅ After death animation + wait time, trigger ShowWin
                isDefeated = true;
                
                if (playerCombatArena != null)
                    playerCombatArena.TriggerShowWin();
                
                // Hide boss after animation completes
                gameObject.SetActive(false);
                
                Debug.Log("Boss defeated with pink slime death animation!");
            });
        }
        else
        {
            // Fallback: original behavior if no PinkSlimeAnimation
            isDefeated = true;
            gameObject.SetActive(false);
            Debug.Log("Boss defeated!");
        }
    }
}
