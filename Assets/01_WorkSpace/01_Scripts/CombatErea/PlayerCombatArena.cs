using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerCombatArena : MonoBehaviour
{
    [Header("Stats")]
    public PlayerStats playerStats;

    [Header("Attack Settings")]
    public LayerMask enemyLayer;
    public float flashDuration = 0.1f;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;     // Speed for following mouse/finger

    [Header("Combat Arena Temp Stats")]
    public int currentHp;
    public int currentExp;

    [Header("UI")]
    public Image healthBarImage;
    public Image expBarImage;   

    public SpriteRenderer rend;
    private Camera mainCamera;

    private void OnEnable() 
    {
        mainCamera = Camera.main;
        
        // Initialize current stats from PlayerStats
        if (playerStats != null)
        {
            currentHp = playerStats.GetStatValue(EnumStat.hp);
            currentExp = playerStats.GetStatValue(EnumStat.exp);
        }
        
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
        UpdateHealthBar();
        UpdateExpBar();
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarImage != null && playerStats != null)
        {
            int maxHp = playerStats.GetStatValue(EnumStat.hp);
            if (maxHp > 0)
            {
                float fillAmount = (float)currentHp / maxHp;
                healthBarImage.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }
    private void UpdateExpBar()
    {
        if (expBarImage != null)
        {
            // Assuming max exp is 100 for now (you can change this logic)
            float fillAmount = (float)currentExp / 100f;
            expBarImage.fillAmount = Mathf.Clamp01(fillAmount);
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
