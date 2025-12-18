using UnityEngine;

public class YellowSlime : Enemy
{
    [Header("Wandering Settings")]
    public float circleRadius = 1f;        // Radius of the circular motion
    public float circleSpeed = 2f;         // How fast it circles (rotations per second)
    
    private float circleAngle;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        // Start at random angle in circle
        circleAngle = Random.Range(0f, 360f);
    }

    private void Update()
    {
        MoveWithCircle();
        CheckOffscreen();
    }

    private void MoveWithCircle()
    {
        // Update circle angle over time
        circleAngle += circleSpeed * 360f * Time.deltaTime;
        
        // Calculate circular offset using sin and cos
        float radians = circleAngle * Mathf.Deg2Rad;
        Vector2 circleOffset = new Vector2(
            Mathf.Cos(radians) * circleRadius,
            Mathf.Sin(radians) * circleRadius
        );
        
        // Get base direction toward target
        Vector2 baseDirection = (targetPosition - (Vector2)transform.position).normalized;
        
        // Move forward + apply circular offset
        Vector2 movement = baseDirection * moveSpeed + circleOffset * circleSpeed;
        transform.position += (Vector3)(movement * Time.deltaTime);
    }
}
