using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShadowEnemy : MonoBehaviour
{
    public static ShadowEnemy Instance { get; private set; }

    // Enemy object in world space (can have multiple SpriteRenderers)
    public Transform enemyTransform;
    private SpriteRenderer[] enemySprites;
    public Transform playerTransform;
    public Transform cameraTransform;

    // Distance settings
    public float maxDistance = 15f;
    public float minDistance = 2f;

    // Transparency settings
    public float maxAlpha = 1f;
    public float minAlpha = 0f;

    // Screen takeover UI
    public Image screenTakeoverImage;
    public float takeoverDuration = 1f;

    // Position offset
    public Vector3 offsetFromPlayer = new Vector3(0f, -1f, -1f);

    // Visibility duration settings
    public float maxVisibleDuration = 5f;

    private float currentHealth = 1f;

    // Visibility state
    private bool isVisible = false;
    private Coroutine hideCoroutine;

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

        // Get all SpriteRenderers from the enemy transform
        if (enemyTransform != null)
            enemySprites = enemyTransform.GetComponentsInChildren<SpriteRenderer>();
        else
            Debug.LogWarning("ShadowEnemy: enemyTransform is not assigned!");

        if (screenTakeoverImage != null)
            screenTakeoverImage.gameObject.SetActive(false);

        StartCoroutine(SubscribeToHealthDelayed());

        // Start hidden
        HideEnemy();
    }

    private IEnumerator SubscribeToHealthDelayed()
    {
        yield return null;
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged += OnHealthChanged;
            currentHealth = PlayerHealth.Instance.health;
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnHealthChanged -= OnHealthChanged;
    }

    private void LateUpdate()
    {
        if (isVisible)
        {
            UpdateEnemyPosition();
        }
    }
    private void OnHealthChanged(float newHealth)
    {
        float previousHealth = currentHealth;
        currentHealth = newHealth;

        // Only show enemy if player took damage (health decreased)
        if (newHealth < previousHealth)
            ShowEnemy();

        float alpha = Mathf.Lerp(maxAlpha, minAlpha, Mathf.Clamp01(currentHealth));

        if (isVisible)
        {
            UpdateEnemyAppearance();
            UpdateEnemyPosition();
        }
    }

    private void UpdateEnemyAppearance()
    {
        if (enemySprites == null || enemySprites.Length == 0) 
            return;

        // Alpha: 0 at full health, maxAlpha at zero health
        float healthPercent = Mathf.Clamp01(currentHealth);
        float alpha = Mathf.Lerp(maxAlpha, minAlpha, healthPercent);

        foreach (SpriteRenderer sprite in enemySprites)
        {
            if (sprite == null) 
                continue;
            Color color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
    }

    private void UpdateEnemyPosition()
    {
        if (enemyTransform == null || playerTransform == null || cameraTransform == null) return;

        // Distance: far at full health, close at zero health
        float healthPercent = Mathf.Clamp01(currentHealth);
        float distance = Mathf.Lerp(minDistance, maxDistance, healthPercent);

        // Place enemy in front of player (between player and camera) - only on horizontal plane
        Vector3 directionToCamera = cameraTransform.position - playerTransform.position;
        directionToCamera.y = 0; // Keep only horizontal direction
        directionToCamera.Normalize();

        Vector3 enemyPosition = playerTransform.position + directionToCamera * distance + offsetFromPlayer;
        enemyTransform.position = enemyPosition;

        // Make 2D sprite face away from camera (show back to player)
        Vector3 lookDir = cameraTransform.position - enemyTransform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            enemyTransform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void ShowEnemy()
    {
        // Cancel any pending hide
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        isVisible = true;

        if (enemyTransform != null)
            enemyTransform.gameObject.SetActive(true);

        UpdateEnemyAppearance();
        UpdateEnemyPosition();

        // Start timer to hide after maxVisibleDuration
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void HideEnemy()
    {
        isVisible = false;

        if (enemyTransform != null)
            enemyTransform.gameObject.SetActive(false);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(maxVisibleDuration);
        HideEnemy();
        Debug.Log("ShadowEnemy: Hidden after timeout");
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

        // Hide the world sprite
        if (enemyTransform != null)
            enemyTransform.gameObject.SetActive(false);

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
        HideEnemy();
        if (screenTakeoverImage != null)
            screenTakeoverImage.gameObject.SetActive(false);
    }
}
