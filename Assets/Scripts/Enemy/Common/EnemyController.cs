using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float attackCooldown;
    private float attackTimer = 0f;

    [SerializeField] private EnemyBullet bulletPrefab;

    [SerializeField] private float generalSpeed = 5f;

    [Header("Ring Attack Information")]
    [SerializeField] private float ringCount = 10f;
    [SerializeField] private float ringSpeed = 5f;
    [SerializeField] private float spawnRadius = 0.5f;

    private PlayerController pc;

    private void Start()
    {
        pc = GameObject.FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        ChooseRandomAttack();
        HandleTimer();
    }

    #region Attack Types and Patterns

    public EnemyBullet SpawnBullet(Vector2 position, Vector2 direction, float speed)
    {
        EnemyBullet bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        EnemyBullet bulletCode = bullet.GetComponent<EnemyBullet>();
        bulletCode.Launch(direction, speed);
        return bulletCode;
    }

    public EnemyBullet SpawnBulletAimed(Vector2 position, Transform target, float speed, float delay, bool willDelay = false, float duration = 5f)
    {
        EnemyBullet bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        TimedObjectDestroyer destroyer = bullet.GetComponent<TimedObjectDestroyer>();
        if (destroyer != null && destroyer.lifeTime != duration) destroyer.lifeTime = duration;
        if (willDelay)
            StartCoroutine(AimAndLaunch(bullet, target, speed, delay));
        else
        {
            Vector2 dir = (Vector2)target.position - (Vector2)bullet.transform.position;
            bullet.Launch(dir, speed);
        }
        return bullet;
    }

    private IEnumerator AimAndLaunch(EnemyBullet bullet, Transform target, float speed, float delay)
    {
        TimedObjectDestroyer destroyer = bullet.GetComponent<TimedObjectDestroyer>();
        if (destroyer != null) destroyer.lifeTime += delay; //delay doesn't affect the maximum time
        yield return new WaitForSeconds(delay);
        if (bullet == null) yield break;

        Vector2 dir = (Vector2)target.position - (Vector2)bullet.transform.position;
        bullet.Launch(dir, speed);
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
        for (int i = 0; i < ringCount; i++)
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
    private void ChooseRandomAttack()
    {
        if(attackTimer <= 0f)
        {
            attackTimer = attackCooldown;

            float rand = Random.Range(0, 101);
            if(rand <= 45)
            {
                BulletStraightDown();
            }
            else if(rand > 45 && rand <= 90)
            {
                SpawnBulletAimed(transform.position, pc.transform, 7f, 0f);
            }
            else
            {
                RingOfBullets();
            }
        }
    }



    private void HandleTimer()
    {
        attackTimer -= Time.deltaTime;
    }
}
