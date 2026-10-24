using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] private float mergeCount = 30f;

    [Header("Movement")]
    [SerializeField] private float moveSmoothTime = 0.6f;   // higher = lazier glide

    [SerializeField] private float edgePadding = 1f;        // keep the boss off the screen edges
    [SerializeField] private float minHopDistance = 2f;     // don't pick a spot right next to the current one

    enum MoveMode { Idle, Hop, Sweep, Orbit }

    private MoveMode moveMode = MoveMode.Idle;
    private float modeTime;

    [Header("Sweep / Orbit")]
    [SerializeField] private float sweepRange = 3f;
    [SerializeField] private float sweepSpeed = 1.2f;
    [SerializeField] private float orbitRadius = 1.5f;

    [Header("Phase 1 timing")]
    [SerializeField] private Vector2 phase1Pause = new Vector2(2f, 3f);   // calm gap between attacks

    [Header("Phase 2 timing")]
    [SerializeField] private Vector2 phase2Pause = new Vector2(1.5f, 2.5f); //Shorter gap between attacks

    [Header("Phase 3 timing")]
    [SerializeField] private Vector2 phase3Pause = new Vector2(1.2f, 2f); //Shorter gap between attacks

    [Header("Phase 4 timing")]
    [SerializeField] private Vector2 phase4Pause = new Vector2(1.0f, 1.6f);
    [SerializeField] private Vector2 specialPause = new Vector2(3f, 5f);
    [SerializeField] private Vector2 tiredPause = new Vector2(5f, 10f);

    private Vector2 moveTarget;
    private Vector2 moveVelocity;
    private PlayerController player;
    private EnemyHealth enemyHealth;

    private int phase = 1;
    bool transitioning;
    bool enraged;
    Coroutine phaseRoutine;

    private BottomWarning bw;
    private TopWarning tw;
    [SerializeField] private float warningDuration = 1f;

    private void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>();
        enemyHealth = GetComponent<EnemyHealth>();
        phaseRoutine = StartCoroutine(RunPhase(1));
        moveTarget = transform.position;
        bw = GameObject.FindAnyObjectByType<BottomWarning>();
        tw = GameObject.FindAnyObjectByType<TopWarning>();
        bw.gameObject.SetActive(false);
        tw.gameObject.SetActive(false);
        //StartCoroutine(EnrageTimer());
    }

    private void Update()
    {
        modeTime += Time.deltaTime;
        Camera cam = Camera.main;
        Vector2 c = cam.transform.position;
        float topY = c.y + cam.orthographicSize * 0.6f;

        switch (moveMode)
        {
            case MoveMode.Sweep:   // side to side across the top
                moveTarget = new Vector2(c.x + Mathf.Sin(modeTime * sweepSpeed) * sweepRange, topY);
                break;
            case MoveMode.Orbit:   // circle around a point near the top
                moveTarget = new Vector2(c.x + Mathf.Cos(modeTime * sweepSpeed) * orbitRadius * 2f,
                                         topY + Mathf.Sin(modeTime * sweepSpeed) * orbitRadius * 0.5f);
                break;
                // Hop and Idle: the phase coroutine sets moveTarget itself
        }

        transform.position = Vector2.SmoothDamp(transform.position, moveTarget, ref moveVelocity, moveSmoothTime);
    }

    private IEnumerator Warn(bool bottom, bool top, float duration = -1f)
    {
        if (duration < 0f) duration = warningDuration;

        if (bottom && bw != null) bw.gameObject.SetActive(true);   // or bw.Show()
        if (top && tw != null) tw.gameObject.SetActive(true);      // or tw.Show()

        yield return new WaitForSeconds(duration);

        HideWarnings();
    }

    private void HideWarnings()
    {
        if (bw != null) bw.gameObject.SetActive(false);            // or bw.Hide()
        if (tw != null) tw.gameObject.SetActive(false);            // or tw.Hide()
    }

    #region Phases and Behavior
    int PhaseFromHealth()
    {
        float frac = enemyHealth.currHealth / enemyHealth.maxHealth;
        return Mathf.Clamp(1 + Mathf.FloorToInt((1f - frac) / 0.2f), 1, 5);
    }

    public void CheckPhase()
    {
        if (transitioning) return;                 // re-checked when the transition ends
        if (PhaseFromHealth() > phase) StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        transitioning = true;
        enemyHealth.isInvuln = true;
        if (phaseRoutine != null)
        {
            StopCoroutine(phaseRoutine);
            phaseRoutine = null;
        }

        GoToTopCenter();

        // Skip the merge when entering phase 5 if you want; here every boundary gets one
        yield return StartCoroutine(MergeAttack());   // waits until the cubes finish

        phase = PhaseFromHealth();                 // may have jumped more than one phase
        transitioning = false;
        enemyHealth.isInvuln = false;
        phaseRoutine = StartCoroutine(RunPhase(phase));
        CheckPhase();                              // covers damage taken during the merge
    }

    IEnumerator RunPhase(int p)
    {
        switch (p)
        {
            case 1: yield return Phase1(); break;
            case 2: yield return Phase2(); break;
            case 3: yield return Phase3(); break;
            case 4: yield return Phase4(); break;
            case 5: yield return Phase5(); break;
            default: yield return Phase1(); break; // placeholder until the others exist
        }
        yield break;
    }

    IEnumerator Phase1()
    {
        SetMoveMode(MoveMode.Sweep, 0.15f);   // lower smooth time = tighter tracking of the sine path
        bool useRing = true;
        while (true)
        {
            if (useRing) RingOfBullets();
            else yield return ArcBelow();
            SetMoveMode(MoveMode.Sweep, 0.15f);
            float random = Random.Range(0, 101);
            if (random >= 50)
                useRing = !useRing;
            yield return new WaitForSeconds(Cooldown(phase1Pause));
        }
    }

    IEnumerator Phase2()
    {
        bool useRing = true;
        SetMoveMode(MoveMode.Hop, 0.6f);
        while (true)
        {
            moveTarget = RandomTopHalfPoint();
            float timeout = 3f;
            while (((Vector2)transform.position - moveTarget).sqrMagnitude >= 0.25f && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            float rand = Random.Range(0, 101);
            if (rand <= 35)
            {
                yield return Warn(bottom: true, top: false);
                RisingVolley();
                yield return new WaitForSeconds(Cooldown(phase2Pause));
            }

            if (useRing) RingOfBullets();
            else yield return ArcBelow();
            SetMoveMode(MoveMode.Hop, 0.6f);
            float random = Random.Range(0, 101);
            if (random >= 50)
                useRing = !useRing;

            yield return new WaitForSeconds(Cooldown(phase2Pause));
        }
    }

    IEnumerator Phase3()
    {
        bool useRing = true;
        SetMoveMode(MoveMode.Orbit, 0.6f);
        while (true)
        {
            float rand = Random.Range(0, 101);

            if (rand <= 35)
            {
                rand = Random.Range(0, 101);
                bool rising = rand >= 50;
                yield return Warn(bottom: rising, top: !rising);
                RisingVolley(rising);
                yield return new WaitForSeconds(Cooldown(phase3Pause));
            }

            if (useRing) RingOfBullets();
            else yield return ArcBelow();
            SetMoveMode(MoveMode.Orbit, 0.6f);

            float random = Random.Range(0, 101);
            if (random >= 35)
                useRing = true;
            else
                useRing = false;

            yield return new WaitForSeconds(Cooldown(phase3Pause));
        }

    }

    IEnumerator Phase4()
    {
        bool useRing = true;
        SetMoveMode(MoveMode.Orbit, 0.6f);
        while (true)
        {
            float rand = Random.Range(0, 101);

            if (rand <= 15)
            {
                GoToCenter();
                yield return Warn(bottom: true, top: true);
                StartCoroutine(ConvergeAttack());
                yield return new WaitForSeconds(Cooldown(specialPause));
            }

            if (rand <= 50)
            {
                rand = Random.Range(0, 101);
                bool rising = rand >= 50;
                yield return Warn(bottom: rising, top: !rising);
                RisingVolley(rising);
                yield return new WaitForSeconds(Cooldown(phase4Pause));
            }

            if (useRing) RingOfBullets();
            else yield return ArcBelow();
            SetMoveMode(MoveMode.Orbit, 0.6f);

            float random = Random.Range(0, 101);
            if (random >= 35)
                useRing = true;
            else
                useRing = false;

            yield return new WaitForSeconds(Cooldown(phase4Pause));
        }
    }

    IEnumerator Phase5()
    {
        GoToCenter();
        while (true)
        {
            yield return Warn(bottom: true, top: true);
            StartCoroutine(ConvergeAttack());
            float rand = Random.Range(0, 101);
            if (rand >= 50)
                yield return new WaitForSeconds(Cooldown(specialPause));
            else
                yield return new WaitForSeconds(Cooldown(tiredPause));
        }
    }

    float Cooldown(Vector2 range)
    {
        float t = Random.Range(range.x, range.y);
        return t;
    }

    Vector2 RandomTopHalfPoint()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector2 c = cam.transform.position;

        Vector2 p = transform.position;
        for (int i = 0; i < 10; i++)
        {
            p = new Vector2(
                Random.Range(c.x - halfW + edgePadding, c.x + halfW - edgePadding),
                Random.Range(c.y + edgePadding, c.y + halfH - edgePadding));   // c.y to the top = top half
            if (((Vector2)transform.position - p).sqrMagnitude >= minHopDistance * minHopDistance) break;
        }
        return p;
    }
    void SetMoveMode(MoveMode mode, float smoothTime = 0.6f)
    {
        moveMode = mode;
        modeTime = 0f;
        moveSmoothTime = smoothTime;
    }

    void GoToTopCenter(float smoothTime = 0.4f)
    {
        Camera cam = Camera.main;
        Vector2 c = cam.transform.position;

        SetMoveMode(MoveMode.Idle, smoothTime);
        moveTarget = new Vector2(c.x, c.y + cam.orthographicSize - edgePadding);
    }

    void GoToCenter(float smoothTime = 0.4f)
    {
        Camera cam = Camera.main;
        Vector2 c = cam.transform.position;

        SetMoveMode(MoveMode.Idle, smoothTime);
        moveTarget = new Vector2(c.x, c.y);
    }

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

    private IEnumerator ArcBelow()
    {
        SetMoveMode(MoveMode.Idle);
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
        yield return new WaitForSeconds(arcChargeTimer);
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
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator MergeAttack()
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
        yield return new WaitUntil(() => arrived >= 2);
    }

    // Optional payoff: a ring where they collide
    private void OnMergeImpact(Vector2 point)
    {
        
        for (int i = 0; i < mergeCount; i++)
        {
            float a = i * (360f / mergeCount) * Mathf.Deg2Rad;
            SpawnBullet(point, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), 4f);
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
