using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Boss : Enemy
{
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public bool hasTarget = false;
    public bool isDefeated = false;
    private Vector2 bossDirection;
    
    protected override void OnEnable()
    {
        InitializeEnemy();
    }
    
    protected override void OnDisable()
    {
        // Don't call base.OnDisable() to prevent returning to pool
    }
    
    // Boss inherits all functionality from Enemy
    // Override Die() to hide instead of returning to pool
    
    protected override void Die()
    {
        // Spawn currency (inherited from Enemy)
        SpawnCurrency();
        
        // Mark as defeated
        isDefeated = true;
        
        // Hide boss
        gameObject.SetActive(false);
        
        Debug.Log("Boss defeated!");
    }
    
    // Set target and calculate direction
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        bossDirection = (targetPosition - transform.position).normalized;
        hasTarget = true;
    }
    
    // Boss movement instead of Enemy movement
    private void Update()
    {
        if (hasTarget)
        {
            // Move in straight line toward target
            transform.position += (Vector3)bossDirection * moveSpeed * Time.deltaTime;
        }
    }
}
