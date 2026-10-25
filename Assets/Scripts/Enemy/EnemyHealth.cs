using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private bool IsBoss = false;
    private BossEnemy bossRef;
    public float maxHealth = 20f;
    public float currHealth = 20f;
    [SerializeField] private int pointValue = 100;

    public bool isInvuln = false;

    private void Start()
    {
        currHealth = maxHealth;
        if (IsBoss) bossRef = GetComponent<BossEnemy>();
    }

    public void TakeDamage(float damage)
    {
        if (isInvuln) return;
        currHealth -= damage;

        if(currHealth <= 0)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointValue);
            }
            if (IsBoss)
            {
                SceneManager.LoadScene("Win_Scene");
            }
            Destroy(gameObject);

        }

        if (IsBoss)
        {
            bossRef.CheckPhase();
        }

    }
}
