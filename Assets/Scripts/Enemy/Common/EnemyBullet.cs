using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    public float Damage => damage;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
        // optional: rotate the sprite to face its travel direction
        transform.up = direction;
    }
}