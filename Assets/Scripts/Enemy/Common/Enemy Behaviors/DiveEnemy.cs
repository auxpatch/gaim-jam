using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DiveEnemy : MonoBehaviour
{
    [Header("Screen (match your camera)")]
    [SerializeField] private Vector2 halfExtents = new Vector2(4f, 4.5f);
    [SerializeField] private float offscreenMargin = 3f;

    [Header("Entry")]
    [SerializeField] private Vector2 startHeightRange = new Vector2(2.5f, 5.5f);

    [Header("Movement")]
    [SerializeField] private Vector2 speedRange = new Vector2(4f, 6f);
    [Header("Pull-up (after passing the player)")]
    [Tooltip("How fast the enemy bends upward once past the player's spot.")]
    [SerializeField] private float pullUpDegPerSec = 60f;
    [Tooltip("Final upward angle above horizontal. Keep small for a subtle curve.")]
    [Range(0f, 45f)]
    [SerializeField] private float maxClimbAngleDeg = 20f;

    [SerializeField] private float maxLifetime = 15f; // failsafe

    private Rigidbody2D rb;
    private PlayerController player;

    private Vector2 pos, dir, target;
    private float speed, sideSign;
    private bool passedTarget, hasEntered;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("DiveEnemy: no TEMP_player found in scene.");
            Destroy(gameObject);
            return;
        }

        sideSign = Random.value < 0.5f ? 1f : -1f; // +1 = enters top-left, travels right
        pos = new Vector2(-sideSign * (halfExtents.x + offscreenMargin),
                          Random.Range(startHeightRange.x, startHeightRange.y));

        target = player.transform.position; // locked at spawn
        dir = (target - pos).normalized;    // the line that hits the player if they stand still
        speed = Random.Range(speedRange.x, speedRange.y);

        rb.position = pos;
        transform.position = pos;
        Destroy(gameObject, maxLifetime);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Once past the player's spot, optionally bend toward horizontal
        if (!passedTarget && Vector2.Dot(target - pos, dir) <= 0f)
            passedTarget = true;

        if (passedTarget && pullUpDegPerSec > 0f)
        {
            // Final heading: sideways, tilted up by maxClimbAngleDeg
            float climb = maxClimbAngleDeg * Mathf.Deg2Rad;
            Vector2 finalDir = new Vector2(sideSign * Mathf.Cos(climb), Mathf.Sin(climb));

            float maxRad = pullUpDegPerSec * Mathf.Deg2Rad * dt;
            dir = Vector3.RotateTowards(dir, finalDir, maxRad, 0f);
            dir.Normalize();
        }

        pos += dir * speed * dt;
        rb.MovePosition(pos);

        // Despawn only after it has been on screen and then leaves bounds + margin
        bool inside = Mathf.Abs(pos.x) <= halfExtents.x && Mathf.Abs(pos.y) <= halfExtents.y;
        if (inside) hasEntered = true;

        bool outside = Mathf.Abs(pos.x) > halfExtents.x + offscreenMargin ||
                       Mathf.Abs(pos.y) > halfExtents.y + offscreenMargin;
        if (hasEntered && outside) Destroy(gameObject);
    }
}