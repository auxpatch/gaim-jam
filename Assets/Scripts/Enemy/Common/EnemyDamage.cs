using UnityEngine;

public class EnemyDamage : MonoBehaviour
{

    [SerializeField] private float cooldown = 3f;
    private float timer = 0f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (timer > 0) return;
        PlayerHealth health = collider.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            timer = cooldown;
            health.TakeDamage();
        }
    }

    private void Update()
    {
        HandleTimers();
    }

    private void HandleTimers()
    {
        timer -= Time.deltaTime;
    }

}
