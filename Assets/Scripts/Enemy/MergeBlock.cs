using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MergeBlock : MonoBehaviour
{
    enum State { Tracking, Pausing, Slamming, Done }

    public event Action<MergeBlock> Arrived;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private State state;
    private Vector2 pos;

    private float timer, slamSpeed;
    private float trackDuration, pauseDuration, trackSpeed, slamMaxSpeed, slamAccel;
    private float centerX, minY, maxY;

    [SerializeField] private int contactDamageCooldownFrames = 0; // unused, PlayerHealth handles i-frames
    [SerializeField] private Color pauseColor = Color.red;
    [SerializeField] private float lingerTime = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(Transform playerTarget, Vector2 startPos, float centerX, float minY, float maxY,
                     float trackDuration, float pauseDuration, float trackSpeed,
                     float slamMaxSpeed, float slamAccel)
    {
        player = playerTarget;
        pos = startPos;
        this.centerX = centerX;
        this.minY = minY;
        this.maxY = maxY;
        this.trackDuration = trackDuration;
        this.pauseDuration = pauseDuration;
        this.trackSpeed = trackSpeed;
        this.slamMaxSpeed = slamMaxSpeed;
        this.slamAccel = slamAccel;

        transform.position = startPos;
        rb.position = startPos;
        state = State.Tracking;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        switch (state)
        {
            case State.Tracking:
                timer += dt;
                if (player != null)
                {
                    float targetY = Mathf.Clamp(player.position.y, minY, maxY);
                    pos.y = Mathf.MoveTowards(pos.y, targetY, trackSpeed * dt); // capped so it can be outrun
                }
                if (timer >= trackDuration) { state = State.Pausing; timer = 0f; if (sr) sr.color = pauseColor; }
                break;

            case State.Pausing: // frozen in place: this is the player's window to get out
                timer += dt;
                if (timer >= pauseDuration) { state = State.Slamming; slamSpeed = 0f; }
                break;

            case State.Slamming:
                slamSpeed = Mathf.MoveTowards(slamSpeed, slamMaxSpeed, slamAccel * dt);
                pos.x = Mathf.MoveTowards(pos.x, centerX, slamSpeed * dt);
                if (Mathf.Approximately(pos.x, centerX))
                {
                    state = State.Done;
                    Arrived?.Invoke(this);
                    Destroy(gameObject, lingerTime);
                }
                break;
        }

        rb.MovePosition(pos);
    }

    // Stay, not Enter, so the player still gets hit when their i-frames end while overlapping
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var health))
            health.TakeDamage();
    }
}