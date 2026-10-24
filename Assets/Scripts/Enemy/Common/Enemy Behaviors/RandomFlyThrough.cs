using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RandomFlyThrough : MonoBehaviour
{
    enum State { Moving, Hovering, Exiting }
    enum Edge { Left, Right, Top, Bottom }

    [Header("Screen (match your camera)")]
    [SerializeField] private Vector2 halfExtents = new Vector2(4f, 4.5f);
    [SerializeField] private float offscreenMargin = 1.5f;

    [Header("Hover zone")]
    [SerializeField] private Vector2 hoverMin = new Vector2(-3f, 1f);
    [SerializeField] private Vector2 hoverMax = new Vector2(3f, 3.5f);
    [SerializeField] private Vector2Int hoverCount = new Vector2Int(1, 3); // inclusive min/max stops
    [SerializeField] private float minSpotSpacing = 1.5f; // keeps stops from landing on each other

    [Header("Random ranges (x = min, y = max)")]
    [SerializeField] private Vector2 moveSpeed = new Vector2(2.5f, 4f);
    [SerializeField] private Vector2 exitSpeed = new Vector2(3f, 5f);
    [SerializeField] private Vector2 hoverTime = new Vector2(1f, 2.5f); // per stop
    [SerializeField] private Vector2 wobbleAmplitude = new Vector2(0.2f, 0.6f);
    [SerializeField] private Vector2 wobbleFrequency = new Vector2(1f, 3f);
    [SerializeField] private float wobbleFadeSpeed = 3f; // how quickly wobble fades in/out

    [SerializeField] private float maxLifetime = 40f; // failsafe

    private Rigidbody2D rb;
    private State state;
    private Vector2 basePos;
    private List<Vector2> hoverPoints = new List<Vector2>();
    private int spotIndex;
    private Vector2 exitPoint;
    private float moveSpd, exitSpd, hoverTimer;
    private float amp, freq, phase, t, wobbleWeight;

    public bool IsHovering => state == State.Hovering; // handy for gating attacks

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Start()
    {
        Edge entryEdge = (Edge)Random.Range(0, 3); // left, right, or top
        basePos = EdgePoint(entryEdge);
        exitPoint = EdgePoint(Opposite(entryEdge));

        int stops = Random.Range(hoverCount.x, hoverCount.y + 1);
        for (int i = 0; i < stops; i++)
            hoverPoints.Add(PickHoverPoint(i == 0 ? basePos : hoverPoints[i - 1]));

        moveSpd = Random.Range(moveSpeed.x, moveSpeed.y);
        exitSpd = Random.Range(exitSpeed.x, exitSpeed.y);
        amp = Random.Range(wobbleAmplitude.x, wobbleAmplitude.y);
        freq = Random.Range(wobbleFrequency.x, wobbleFrequency.y);
        phase = Random.Range(0f, Mathf.PI * 2f);

        state = State.Moving;
        rb.position = basePos;
        transform.position = basePos;
        Destroy(gameObject, maxLifetime);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        t += dt;

        switch (state)
        {
            case State.Moving:
                basePos = Vector2.MoveTowards(basePos, hoverPoints[spotIndex], moveSpd * dt);
                if (basePos == hoverPoints[spotIndex])
                {
                    hoverTimer = Random.Range(hoverTime.x, hoverTime.y);
                    state = State.Hovering;
                }
                break;

            case State.Hovering:
                hoverTimer -= dt;
                if (hoverTimer <= 0f)
                {
                    spotIndex++;
                    state = spotIndex >= hoverPoints.Count ? State.Exiting : State.Moving;
                }
                break;

            case State.Exiting:
                basePos = Vector2.MoveTowards(basePos, exitPoint, exitSpd * dt);
                if (basePos == exitPoint) { Destroy(gameObject); return; }
                break;
        }

        // Wobble only while hovering; fade it in and out so there are no pops
        float target = state == State.Hovering ? 1f : 0f;
        wobbleWeight = Mathf.MoveTowards(wobbleWeight, target, wobbleFadeSpeed * dt);

        Vector2 wobble = new Vector2(
            Mathf.Sin(t * freq + phase),
            Mathf.Cos(t * freq * 0.7f + phase)) * (amp * wobbleWeight);

        rb.MovePosition(basePos + wobble);
    }

    // Tries a few times to find a spot far enough from the previous one
    Vector2 PickHoverPoint(Vector2 previous)
    {
        Vector2 p = Vector2.zero;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            p = new Vector2(
                Random.Range(hoverMin.x, hoverMax.x),
                Random.Range(hoverMin.y, hoverMax.y));
            if ((p - previous).sqrMagnitude >= minSpotSpacing * minSpotSpacing) break;
        }
        return p;
    }

    Edge Opposite(Edge e)
    {
        switch (e)
        {
            case Edge.Left: return Edge.Right;
            case Edge.Right: return Edge.Left;
            case Edge.Top: return Edge.Bottom;
            default: return Edge.Top;
        }
    }

    Vector2 EdgePoint(Edge e)
    {
        float mx = halfExtents.x + offscreenMargin;
        float my = halfExtents.y + offscreenMargin;
        switch (e)
        {
            case Edge.Left: return new Vector2(-mx, Random.Range(0f, halfExtents.y));
            case Edge.Right: return new Vector2(mx, Random.Range(0f, halfExtents.y));
            case Edge.Top: return new Vector2(Random.Range(-halfExtents.x, halfExtents.x), my);
            default: return new Vector2(Random.Range(-halfExtents.x, halfExtents.x), -my);
        }
    }
}