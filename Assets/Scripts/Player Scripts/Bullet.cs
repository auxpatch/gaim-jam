using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;
    public Rigidbody2D rb;
    private Vector2 linearVelocity;

    void Start()
    {
        linearVelocity = transform.up * speed;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = linearVelocity;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Destroy(gameObject);
    }
}
