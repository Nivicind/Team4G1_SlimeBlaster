using UnityEngine;

public class Boss : Enemy
{
    public bool isDefeated = false;
    Vector2 bossDirection;


    protected override void OnEnable()
    {
        base.OnEnable();  // This initializes slimeAnim and other base components
        isDefeated = false;
        targetPosition = new Vector2(0, 0);
    }

    protected override void OnDisable()
    {
        // Don't call base.OnDisable() to prevent returning to pool
    }

    // Set target position (uses base Enemy's targetPosition)
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    protected override void Update()
    {
        // Only move if not at target position
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (direction != Vector2.zero) bossDirection = direction;
        transform.position += (Vector3)(bossDirection * moveSpeed * Time.deltaTime);
        CheckOffscreen();
    }

    protected override void Die()
    {
        // Spawn currency
        SpawnCurrency();

        // Mark as defeated
        isDefeated = true;

        // Hide boss
        gameObject.SetActive(false);

        Debug.Log("Boss defeated!");
    }
}
