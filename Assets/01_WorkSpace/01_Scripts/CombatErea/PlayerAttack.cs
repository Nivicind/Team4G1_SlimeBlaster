using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAttackAndMove2D : MonoBehaviour
{
    [Header("Stats")]
    public PlayerStats playerStats;

    [Header("Attack Settings")]
    public LayerMask enemyLayer;
    public float flashDuration = 0.1f;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;     // Speed for following mouse/finger

    public SpriteRenderer rend;
    private Camera mainCamera;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        StartCoroutine(AttackRoutine());
    }

    private void OnDisable()
    {
        if (rend != null)
        {
            Color color = rend.color;
            color.a = 0f;
            rend.color = color;
        }
    }

    private void Update()
    {
        HandleMovement();
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
            Attack();
        }
    }

    private void Attack()
    {
        // Detect all 2D colliders inside the box
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f, enemyLayer);

        int damage = playerStats.GetStatValue(EnumStat.damage);

        foreach (var hit in hits)
        {
            // Apply damage if enemy has Enemy script
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        StartCoroutine(FlashAlpha());
    }

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
}
