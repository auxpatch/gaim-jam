using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField] private EnemyBullet bulletPrefab;

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

    [Header("Rising Volley (from below the screen)")]
    [SerializeField] private int volleyCount = 14;
    [SerializeField] private float spawnMargin = 1f;          // how far below the camera bottom to spawn
    [SerializeField] private Vector2 rowJitterY = new Vector2(0f, 1.5f); // extra depth, staggers arrival times
    [SerializeField] private float angleJitterDeg = 4f;       // random tilt from straight up
    [SerializeField] private Vector2 volleySpeed = new Vector2(3f, 5f);
    [SerializeField] private int gapSlots = 3;                // adjacent slots left empty

    [Header("Converge Attack")]
    [SerializeField] private int convergeCount = 60;
    [SerializeField] private float convergeSpawnMargin = 1f;   // how far outside the camera edge to spawn
    [SerializeField] private Vector2 convergeSpeed = new Vector2(2f, 3f);
    [SerializeField] private float convergeDuration = 2f;      // spawn window in seconds, 0 = all at once
    [SerializeField] private float centerScatter = 0f;         // 0 = exactly the center, higher = loose cloud

    [Header("Merge Attack")]
    [SerializeField] private MergeBlock mergeBlockPrefab;
    [SerializeField] private float mergeEdgeInset = 0.8f;
    [SerializeField] private float mergeTrackDuration = 3f;
    [SerializeField] private float mergePauseDuration = 1f;
    [SerializeField] private float mergeTrackSpeed = 3f;   // keep below player speed
    [SerializeField] private float mergeSlamSpeed = 14f;
    [SerializeField] private float mergeSlamAccel = 30f;

    private PlayerController player;

    private void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>();
    }

    #region Phase 1 General Floating Behavior

    #endregion

    #region Bullet Behavior
    public EnemyBullet SpawnBullet(Vector2 position, Vector2 direction, float speed, bool willDelay = false, float delay = 0f, float duration = 5f)
    {
        EnemyBullet bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        TimedObjectDestroyer destroyer = bullet.GetComponent<TimedObjectDestroyer>();
        if (destroyer != null && destroyer.lifeTime != duration) destroyer.lifeTime = duration;
        if (willDelay) LaunchAfterDelay(bullet, direction, speed, delay);
        else bullet.Launch(direction, speed);
        return bullet;
    }

    private IEnumerator LaunchAfterDelay(EnemyBullet bullet, Vector2 direction, float speed, float delay)
    {
        TimedObjectDestroyer destroyer = bullet.GetComponent<TimedObjectDestroyer>();
        if (destroyer != null) destroyer.lifeTime += delay; //delay doesn't affect the maximum time
        yield return new WaitForSeconds(delay);
        if (bullet != null) // it may have been destroyed while waiting
            bullet.Launch(direction, speed);

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

    private Vector2 RandomPerimeterPoint(Vector2 c, float halfW, float halfH)
    {
        float w = halfW * 2f, h = halfH * 2f;
        float t = Random.value * (2f * w + 2f * h);

        if (t < w) return new Vector2(c.x - halfW + t, c.y + halfH);   // top
        t -= w;
        if (t < w) return new Vector2(c.x - halfW + t, c.y - halfH);   // bottom
        t -= w;
        if (t < h) return new Vector2(c.x - halfW, c.y - halfH + t);   // left
        t -= h;
        return new Vector2(c.x + halfW, c.y - halfH + t);              // right
    }
    #endregion

    #region Attacks
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
            SpawnBulletAimed(spawnPos, player.transform, arcSpeed, arcChargeTimer, true);
        }
    }

    //Passing false will make it RainingVolley
    private void RisingVolley(bool isRising = true)
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector2 camPos = cam.transform.position;

        float minX = camPos.x - halfW;
        float slotWidth = (halfW * 2f) / volleyCount;
        float baseY = camPos.y - halfH - spawnMargin;
        if (!isRising)
            baseY = camPos.y + halfH + spawnMargin;

        int gapStart = Random.Range(0, volleyCount - gapSlots + 1);

        for (int i = 0; i < volleyCount; i++)
        {
            if (i >= gapStart && i < gapStart + gapSlots) continue;

            float x = minX + (i + Random.value) * slotWidth;
            float y = baseY - Random.Range(rowJitterY.x, rowJitterY.y);

            float angle = (90f + Random.Range(-angleJitterDeg, angleJitterDeg)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (!isRising) dir = -dir;

            SpawnBullet(new Vector2(x, y), dir, Random.Range(volleySpeed.x, volleySpeed.y));
        }
    }

    private IEnumerator ConvergeAttack()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize + convergeSpawnMargin;
        float halfW = cam.orthographicSize * cam.aspect + convergeSpawnMargin;
        Vector2 center = cam.transform.position;

        float elapsed = 0f;
        int spawned = 0;

        while (spawned < convergeCount)
        {
            elapsed += Time.deltaTime;
            int shouldHaveSpawned = convergeDuration > 0f
                ? Mathf.Min(convergeCount, Mathf.CeilToInt(convergeCount * elapsed / convergeDuration))
                : convergeCount;

            while (spawned < shouldHaveSpawned)
            {
                Vector2 spawnPos = RandomPerimeterPoint(center, halfW, halfH);
                Vector2 aimPoint = center + Random.insideUnitCircle * centerScatter;
                Vector2 dir = aimPoint - spawnPos;

                SpawnBullet(spawnPos, dir, Random.Range(convergeSpeed.x, convergeSpeed.y));
                spawned++;
            }
            yield return null;
        }
    }

    private void MergeAttack()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector2 c = cam.transform.position;

        float leftX = c.x - halfW + mergeEdgeInset;
        float rightX = c.x + halfW - mergeEdgeInset;
        float minY = c.y - halfH + 0.5f;
        float maxY = c.y + halfH - 0.5f;

        int arrived = 0;
        System.Action<MergeBlock> onArrived = _ =>
        {
            arrived++;
            if (arrived == 2) OnMergeImpact(new Vector2(c.x, _.transform.position.y));
        };

        foreach (float x in new[] { leftX, rightX })
        {
            MergeBlock b = Instantiate(mergeBlockPrefab);
            b.Init(player.transform, new Vector2(x, c.y), c.x, minY, maxY,
                   mergeTrackDuration, mergePauseDuration, mergeTrackSpeed,
                   mergeSlamSpeed, mergeSlamAccel);
            b.Arrived += onArrived;
        }
    }

    // Optional payoff: a ring where they collide
    private void OnMergeImpact(Vector2 point)
    {
        int n = 16;
        for (int i = 0; i < n; i++)
        {
            float a = i * (360f / n) * Mathf.Deg2Rad;
            SpawnBullet(point, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), 4f);
        }
    }

    #endregion

    #region testing
    private float cooldown = 5f;
    private float timer = 0f;
    private void Update()
    {
        TestAttack();
        HandleTimer();
    }

    private void TestAttack()
    {
        if(attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            
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
