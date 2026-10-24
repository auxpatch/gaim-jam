using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    [SerializeField] private float iFrameDuration = 2.0f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition;

    void Start()
    {
        currentLives = maxLives;
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;
        currentLives--;

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(RespawnSequence());
        }

    }

    private IEnumerator RespawnSequence()
    {
        isInvulnerable = true;

        transform.position = startPosition;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        float elapsed = 0f;
        while (elapsed < iFrameDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += Time.deltaTime;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    private void GameOver()
    {
        gameObject.SetActive(false);
    }

    public int GetCurrentLives() => currentLives;

}
