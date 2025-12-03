using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    public Image healthBarImage;

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
            maxHealth = enemy.enemyData.hp * Level.Instance.GetLevel();
        }
    }

    private void Update()
    {
        if (enemy != null && healthBarImage != null && maxHealth > 0)
        {
            float fillAmount = (float)enemy.currentHealth / maxHealth;
            healthBarImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }
}
