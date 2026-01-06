using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    public GameObject liquidMask;
    
    [Header("Liquid Mask Settings")]
    public float minY = 1.9f; // Y position at 0% health
    public float maxY = 10.5f; // Y position at 100% health

    private Enemy enemy;
    private int maxHealth;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (enemy != null && enemy.enemyData != null)
        {
            maxHealth = enemy.enemyData.hp * Stage.Instance.GetStage();
        }
    }

    private void Update()
    {
        // 💧 Update liquidMask y position based on health percentage
        if (liquidMask != null && enemy != null && maxHealth > 0)
        {
            float healthPercent = (float)enemy.currentHealth / maxHealth;
            float targetY = Mathf.Lerp(minY, maxY, healthPercent);
            Vector3 position = liquidMask.transform.localPosition;
            position.y = targetY;
            liquidMask.transform.localPosition = position;
        }
    }
}
