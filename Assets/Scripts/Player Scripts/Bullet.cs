using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;
    [SerializeField] private float lifetime = 5f;
    public Rigidbody2D rb;
    private Vector2 linearVelocity;

    void Start()
    {
        linearVelocity = transform.up * speed;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = linearVelocity;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if(enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
