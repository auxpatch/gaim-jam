using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float attackCooldown;
    private float attackTimer = 0f;

    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private float generalSpeed = 5f;

    [Header("Ring Attack Information")]
    [SerializeField] private float ringCount = 10f;
    [SerializeField] private float ringSpeed = 5f;
    [SerializeField] private float spawnRadius = 0.5f;



    private void Update()
    {
        //TestAttack();
        HandleTimer();
    }

    #region Attack Types and Patterns

    public EnemyBullet SpawnBullet(Vector2 position, Vector2 direction, float speed)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        EnemyBullet bulletCode = bullet.GetComponent<EnemyBullet>();
        bulletCode.Launch(direction, speed);
        return bulletCode;
    }

    private void BulletStraightDown()
    {
        Vector2 center = transform.position;
        Vector2 dir = new Vector2(0, -1);
        Vector2 spawnPos = center + dir * spawnRadius;
        EnemyBullet spawnedBullet = SpawnBullet(spawnPos, new Vector2(0, -1), generalSpeed);
    }

    private void RingOfBullets()
    {
        float angleOffsetDeg = 0f;
        Vector2 position = transform.position;
        Vector2 center = transform.position;
        float step = 360f / ringCount;
        for(int i = 0; i < ringCount; i++)
        {
            float a = (angleOffsetDeg + i * step) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Vector2 spawnPos = center + dir * spawnRadius;
            SpawnBullet(spawnPos, dir, ringSpeed);
        }
    }

    #endregion


    #region Movement



    #endregion


    //Used for testing, will manually update and call which function is currently being tested
    private void TestAttack()
    {
        if(attackTimer <= 0f)
        {
            attackTimer = attackCooldown;

            //BulletStraightDown();
            RingOfBullets();
        }
    }



    private void HandleTimer()
    {
        attackTimer -= Time.deltaTime;
    }
}
