using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShadowEnemy : MonoBehaviour
{
    public static ShadowEnemy Instance { get; private set; }

    // Enemy sprite in world space
    public SpriteRenderer enemySprite;
    public Transform playerTransform;
    public Transform cameraTransform;

    // Distance settings
    public float maxDistance = 5f;
    public float minDistance = 1f;

    // Transparency settings
    public float maxAlpha = 1f;
    public float minAlpha = 0f;

    // Screen takeover UI
    public Image screenTakeoverImage;
    public float takeoverDuration = 1f;

    // Position offset
    public Vector3 offsetFromPlayer = new Vector3(0f, 1.5f, 0f);

    private float currentHealth = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            if (PlayerHealth.Instance != null)
            {
                playerTransform = PlayerHealth.Instance.transform;
            }
            else
            {
                PlayerHealth ph = FindObjectOfType<PlayerHealth>();
                if (ph != null)
                    playerTransform = ph.transform;
            }
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }
        }

        // Auto-find camera if not assigned
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        if (screenTakeoverImage != null)
            screenTakeoverImage.gameObject.SetActive(false);

        StartCoroutine(SubscribeToHealthDelayed());
        UpdateEnemyAppearance();
    }

    private IEnumerator SubscribeToHealthDelayed()
    {
        yield return null;
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged += OnHealthChanged;
            currentHealth = PlayerHealth.Instance.health;
            UpdateEnemyAppearance();
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnHealthChanged -= OnHealthChanged;
    }

    private void LateUpdate()
    {
        UpdateEnemyPosition();
    }

    private void OnHealthChanged(float newHealth)
    {
        currentHealth = newHealth;
        UpdateEnemyAppearance();
    }

    private void UpdateEnemyAppearance()
    {
        if (enemySprite == null) return;

        // Alpha: 0 at full health, maxAlpha at zero health
        float healthPercent = Mathf.Clamp01(currentHealth);
        float alpha = Mathf.Lerp(maxAlpha, minAlpha, healthPercent);

        Color color = enemySprite.color;
        color.a = alpha;
        enemySprite.color = color;
    }

    private void UpdateEnemyPosition()
    {
        if (enemySprite == null || playerTransform == null || cameraTransform == null) return;

        // Distance: far at full health, close at zero health
        float healthPercent = Mathf.Clamp01(currentHealth);
        float distance = Mathf.Lerp(minDistance, maxDistance, healthPercent);

        // Place enemy between camera and player (chasing from behind)
        Vector3 directionToPlayer = (playerTransform.position - cameraTransform.position).normalized;
        Vector3 enemyPosition = playerTransform.position - directionToPlayer * distance + offsetFromPlayer;
        enemySprite.transform.position = enemyPosition;

        enemySprite.transform.LookAt(playerTransform.position + offsetFromPlayer);
    }

    public void TriggerScreenTakeover(System.Action onComplete)
    {
        StartCoroutine(ScreenTakeoverSequence(onComplete));
    }

    private IEnumerator ScreenTakeoverSequence(System.Action onComplete)
    {
        if (screenTakeoverImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        screenTakeoverImage.gameObject.SetActive(true);

        Color color = screenTakeoverImage.color;
        color.a = 0f;
        screenTakeoverImage.color = color;

        float elapsed = 0f;
        float fadeInDuration = takeoverDuration * 0.6f;

        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;
            color.a = Mathf.Lerp(0f, 1f, t);
            screenTakeoverImage.color = color;
            yield return null;
        }

        // Hold before showing game over
        yield return new WaitForSecondsRealtime(takeoverDuration * 0.4f);
        onComplete?.Invoke();
    }

    public void ResetEnemy()
    {
        currentHealth = 1f;
        UpdateEnemyAppearance();
        if (screenTakeoverImage != null)
            screenTakeoverImage.gameObject.SetActive(false);
    }
}
