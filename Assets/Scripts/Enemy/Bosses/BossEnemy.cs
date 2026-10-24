using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    [Header("Ring Attack Information")]
    [SerializeField] private float ringCount = 10f;
    [SerializeField] private float ringSpeed = 5f;
    [SerializeField] private float spawnRadius = 0.5f;

    [Header("Arc Attack Information")]
    [SerializeField] private int arcCount = 4;
    [SerializeField] private float arcSpanDeg = 120f;
    [SerializeField] private float arcRadius = 1.2f;
    [SerializeField] private float arcSpeed = 4f;
    [SerializeField] private float arcChargeTimer = 0.8f;

    private PlayerController player;

    private void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>();
    }

    #region Phase 1 General Floating Behavior

    #endregion

    #region Bullet Behavior
    public EnemyBullet SpawnBullet(Vector2 position, Vector2 direction, float speed, bool willDelay = false, float delay = 0f)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        EnemyBullet bulletCode = bullet.GetComponent<EnemyBullet>();
        if (willDelay) LauncDelay(delay);
        bulletCode.Launch(direction, speed);
        return bulletCode;
    }

    private IEnumerator LauncDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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

    private void ArcBelow()
    {
        Vector2 center = transform.position;
        float centerAngle = 270f;

        float step = arcCount > 1 ? arcSpanDeg / (arcCount - 1) : 0f;
        float startAngle = centerAngle - arcSpanDeg * 0.5f;

        for (int i = 0; i < arcCount; i++)
        {
            float a = (arcCount > 1 ? startAngle + i * step : centerAngle) * Mathf.Deg2Rad;
            Vector2 radial = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            Vector2 spawnPos = center + radial * arcRadius;
            Vector2 fireDir = (new Vector2(player.transform.position.x, player.transform.position.y) - spawnPos);

            SpawnBullet(spawnPos, fireDir, arcSpeed);
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
