using UnityEngine;

public class GreenSlime : Enemy
{
    [Header("Split Settings")]
    [SerializeField] private int smallSlimesToSpawn = 2;

    private ObjectPool greenSlimeSmallPool;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Find the pool by tag
        if (greenSlimeSmallPool == null)
        {
            GameObject poolObject = GameObject.FindGameObjectWithTag("GreenSlimeSmall");
            if (poolObject != null)
            {
                greenSlimeSmallPool = poolObject.GetComponent<ObjectPool>();
            }
            else
            {
                Debug.LogWarning("GreenSlimeSmall pool not found! Make sure there's a GameObject with tag 'GreenSlimeSmall'");
            }
        }
    }

    protected override void Die()
    {
        slimeAnim.PlayDeathAnimation(() =>
        {
            // Spawn small slimes before dying
            SpawnSmallSlimes();
            // Continue with normal death behavior
            GiveExpToPlayer();
            SpawnCurrency();
            ReturnToPool();
        });

    }

    private void SpawnSmallSlimes()
    {
        if (greenSlimeSmallPool == null)
        {
            Debug.LogWarning("Cannot spawn small slimes - pool is null");
            return;
        }

        // Get collider bounds
        Collider2D col = GetComponent<Collider2D>();

        for (int i = 0; i < smallSlimesToSpawn; i++)
        {
            Vector3 spawnPos;

            if (col != null)
            {
                // Spawn at random position inside collider bounds
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

            GameObject smallSlime = greenSlimeSmallPool.Get(spawnPos, Quaternion.identity);

            if (smallSlime != null)
            {
                GreenSlimeSmall smallSlimeScript = smallSlime.GetComponent<GreenSlimeSmall>();
                if (smallSlimeScript != null)
                {
                    // Set up the small slime's references
                    smallSlimeScript.pool = greenSlimeSmallPool;
                    smallSlimeScript.spawner = spawner;
                    smallSlimeScript.currencyReference = currencyReference;
                    smallSlimeScript.playerStats = playerStats;
                    smallSlimeScript.targetPosition = targetPosition;

                    // Add to spawner's active list
                    spawner?.AddEnemyToActiveList(smallSlimeScript);
                }
            }
        }
    }
}
