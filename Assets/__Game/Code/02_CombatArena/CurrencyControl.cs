using UnityEngine;

public class CurrencyControl : MonoBehaviour
{
    [Header("Currency Settings")]
    public EnumCurrency currencyType;
    public int currencyAmount = 1;

    [Header("Flight Settings")]
    public float maxSpeed = 15f;
    public float acceleration = 5f;
    public float flyTime = 1f;
    public float pickupDistance = 0.25f;

    [HideInInspector] public ObjectPool pool;
    [HideInInspector] public PlayerStats playerStats;

    private bool isFlying = false;
    private Transform targetPlayer;
    private float currentSpeed = 0f;
    private float flyTimer = 0f;
    private bool currencyAdded = false;
    
    private void OnDisable()
    {
        StopFlying();
        pool?.ReturnToPool(gameObject);
    }

    private void Update()
    {
        if (isFlying && targetPlayer != null)
        {
            flyTimer += Time.deltaTime;

            // After 1 second, add currency and return to pool
            if (flyTimer >= flyTime)
            {
                if (!currencyAdded && playerStats != null)
                {
                    playerStats.AddCurrency(currencyType, currencyAmount);
                    currencyAdded = true;
                }
                ReturnToPool();
                return;
            }

            // Accelerate smoothly
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            // Move towards player
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;

            // Check if reached player (disappear immediately)
            if (Vector3.Distance(transform.position, targetPlayer.position) < pickupDistance)
            {
                if (!currencyAdded && playerStats != null)
                {
                    playerStats.AddCurrency(currencyType, currencyAmount);
                    currencyAdded = true;
                }
                ReturnToPool();
            }
        }
    }

    public void StartFlyingToPlayer(Transform player, PlayerStats stats)
    {
        isFlying = true;
        targetPlayer = player;
        playerStats = stats;
        currentSpeed = 0f;
        flyTimer = 0f;
        currencyAdded = false;
    }

    public bool IsFlying()
    {
        return isFlying;
    }

    public void StopFlying()
    {
        isFlying = false;
        targetPlayer = null;
        playerStats = null;
        currentSpeed = 0f;
        flyTimer = 0f;
        currencyAdded = false;
    }

    private void ReturnToPool()
    {
        StopFlying();
        pool?.ReturnToPool(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Always draw the pickup radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
        
        // Draw line to player only when flying
        if (isFlying && targetPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
        }
    }

}
