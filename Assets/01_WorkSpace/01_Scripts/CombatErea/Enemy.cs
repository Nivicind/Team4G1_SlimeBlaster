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

    public int currentHealth { get; private set; }
    private Camera mainCamera;
    private bool justSpawned = true;
    private float spawnIgnoreTime = 3f;
    private Vector2 moveDirection;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        InitializeEnemy();
        justSpawned = true;

        // Pick a random target point inside target area and calculate movement direction
        if (spawner != null)
        {
            Vector2 targetPoint = spawner.GetRandomPointInsideTargetArea();
            moveDirection = (targetPoint - (Vector2)transform.position).normalized;
        }
        else
        {
            // Fallback: random direction
            moveDirection = Random.insideUnitCircle.normalized;
        }

        Invoke(nameof(DisableJustSpawned), spawnIgnoreTime);
    }

    private void Update()
    {
        MoveStraight();
        CheckOffscreen();
    }

    private void InitializeEnemy()
    {
        if (enemyData != null)
            currentHealth = enemyData.hp;
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

    private void Die()
    {
        ReturnToPool();
    }
}
