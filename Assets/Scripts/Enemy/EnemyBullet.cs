using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    public float Damage => damage;

    private PlayerHealth playerHealth;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
        // optional: rotate the sprite to face its travel direction
        transform.up = direction;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerHealth>())
        {
            playerHealth.TakeDamage(); 
        }
    }
    
}