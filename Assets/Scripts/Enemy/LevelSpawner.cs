using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;      // relative chance of being picked
        public float unlockTime = 0f;            // seconds into the level before it can appear
        public Vector2Int groupSize = new Vector2Int(1, 1); // inclusive min/max spawned together
    }

    [Header("Enemies")]
    [SerializeField] private List<EnemyEntry> enemies = new List<EnemyEntry>();

    [Header("Timing")]
    [SerializeField] private float levelDuration = 120f;
    [SerializeField] private float startInterval = 2.5f;   // seconds between spawns at the start
    [SerializeField] private float endInterval = 1.0f;     // seconds between spawns at the end
    [SerializeField] private float intervalJitter = 0.3f;  // +/- random fraction of the interval
    [SerializeField] private float groupStagger = 0.4f;    // delay between members of a group

    [Header("Control")]
    [SerializeField] private int maxAlive = 8;
    [SerializeField] private float quietStart = 2f;        // breathing room before the first spawn

    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Vector2 bossSpawnOffset = new Vector2(0f, 1.5f); // above the camera's top edge
    [SerializeField] private bool waitForClear = true;
    [SerializeField] private int clearThreshold = 1;       // boss comes once this few enemies remain
    [SerializeField] private float clearTimeout = 8f;      // don't wait forever

    private readonly List<GameObject> alive = new List<GameObject>();
    private float elapsed;

    void Start()
    {
        StartCoroutine(RunLevel());
    }

    IEnumerator RunLevel()
    {
        yield return new WaitForSeconds(quietStart);

        // Main spawn phase
        while (elapsed < levelDuration)
        {
            float progress = Mathf.Clamp01(elapsed / levelDuration);
            float interval = Mathf.Lerp(startInterval, endInterval, progress);
            interval *= 1f + Random.Range(-intervalJitter, intervalJitter);

            PruneDead();
            if (alive.Count < maxAlive)
            {
                EnemyEntry entry = PickEntry();
                if (entry != null)
                    yield return SpawnGroup(entry);
            }

            float wait = Mathf.Max(0.1f, interval);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }

        // Let the screen clear out before the boss
        if (waitForClear)
        {
            float timeout = clearTimeout;
            while (CountAlive() > clearThreshold && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        SpawnBoss();
    }

    IEnumerator SpawnGroup(EnemyEntry entry)
    {
        int count = Random.Range(entry.groupSize.x, entry.groupSize.y + 1);
        for (int i = 0; i < count; i++)
        {
            PruneDead();
            if (alive.Count >= maxAlive) yield break;

            // the enemy positions itself in its own Start, so no position needed here
            GameObject e = Instantiate(entry.prefab);
            alive.Add(e);

            if (i < count - 1) yield return new WaitForSeconds(groupStagger);
        }
    }

    // Weighted random pick among entries that have unlocked
    EnemyEntry PickEntry()
    {
        float total = 0f;
        foreach (var e in enemies)
            if (e.prefab != null && elapsed >= e.unlockTime) total += e.weight;

        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (var e in enemies)
        {
            if (e.prefab == null || elapsed < e.unlockTime) continue;
            roll -= e.weight;
            if (roll <= 0f) return e;
        }
        return null;
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("LevelSpawner: no boss prefab assigned.");
            return;
        }

        Camera cam = Camera.main;
        Vector2 c = cam.transform.position;
        Vector2 pos = new Vector2(c.x, c.y + cam.orthographicSize) + bossSpawnOffset;
        Instantiate(bossPrefab, pos, Quaternion.identity);
    }

    void PruneDead() { alive.RemoveAll(e => e == null); }

    int CountAlive() { PruneDead(); return alive.Count; }
}