using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    [Header("Ring Attack Information")]
    [SerializeField] private float ringCount = 10f;
    [SerializeField] private float ringSpeed = 5f;
    [SerializeField] private float spawnRadius = 0.5f;

    #region Bullet Behavior
    public EnemyBullet SpawnBullet(Vector2 position, Vector2 direction, float speed)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        EnemyBullet bulletCode = bullet.GetComponent<EnemyBullet>();
        bulletCode.Launch(direction, speed);
        return bulletCode;
    }

    private void RingOfBullets()
    {
        float angleOffsetDeg = 0f;
        Vector2 position = transform.position;
        Vector2 center = transform.position;
        float step = 360f / ringCount;
        for (int i = 0; i < ringCount; i++)
        {
            float a = (angleOffsetDeg + i * step) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Vector2 spawnPos = center + dir * spawnRadius;
            SpawnBullet(spawnPos, dir, ringSpeed);
        }
    }
    #endregion

    #region Timers
    [Header("Timers")]
    [SerializeField] private float attackCooldown;
    private float attackTimer = 0f;
    private void HandleTimer()
    {
        attackTimer -= Time.deltaTime;
    }
    #endregion
}
