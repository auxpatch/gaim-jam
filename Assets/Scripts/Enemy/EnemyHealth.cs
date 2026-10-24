using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 20f;
    private float currHealth = 20f;

    public void TakeDamage(float damage)
    {
        currHealth -= damage;
        if(currHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
