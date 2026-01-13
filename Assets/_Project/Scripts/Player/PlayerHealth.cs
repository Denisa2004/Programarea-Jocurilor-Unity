using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// PlayerHealth with continuous history recording, auto-rewind on hit, and auto-rewind when falling below a Y threshold.
/// Rewinds to a previous physics-consistent state and falls back to the last safe recorded state if needed.
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public float health = 1f;
    public float damagePerHit = 1f / 3f;

    public Slider healthSlider;

    public float rewindSeconds = 2f;
    public float historyDuration = 5f;
    public float recordInterval = 0.05f;

    public float fallYThreshold = -10f;
    public bool enableAutoRewindOnFall = true;

    public float invulnerabilityDuration = 1.5f;
    public bool disableMovementOnHit = true;
    public MonoBehaviour movementComponent;

    [Header("Audio")]
    public AudioClip damageSound;

    public event Action<float> OnHealthChanged;

    private Rigidbody rb;
    private AudioSource audioSource;
    private AvatarAudioRuntime audioRuntime;


    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public float time;
    }

    private readonly List<State> history = new List<State>();
    private float recordTimer = 0f;
    private bool isInvulnerable = false;
    private bool isPowerUpInvulnerable = false;
    private bool isRestoring = false;

    // The most recent 'safe' recorded state (used as a fallback if rewind samples are invalid)
    private State lastSafeState;
    private bool hasLastSafeState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning("PlayerHealth: No AudioSource found on Player. Damage sounds won't play.");

        audioRuntime = GetComponent<AvatarAudioRuntime>();

        if (rb == null)
            Debug.LogWarning("PlayerHealth: No Rigidbody found. Rewind will still move Transform but won't restore velocity.");
    }

    private void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = health;
        }

        // Trying to auto-find a common movement component if none assigned
        if (movementComponent == null)
        {
            var comps = GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == "MovingTheCapsule")
                {
                    movementComponent = c;
                    break;
                }
            }
        }

        // Initializing lastSafeState from current transform/rigidbody
        var init = new State
        {
            position = rb != null ? rb.position : transform.position,
            rotation = rb != null ? rb.rotation : transform.rotation,
            velocity = rb != null ? rb.linearVelocity : Vector3.zero,
            time = Time.time
        };
        lastSafeState = init;
        hasLastSafeState = true;
        history.Add(init);
    }

    private void FixedUpdate()
    {
        // Record physics-consistent samples in FixedUpdate
        recordTimer += Time.fixedDeltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            AddHistorySample();
            TrimHistory();
        }

        // Auto-detect falling under the threshold and attempt a rewind (if enabled)
        if (enableAutoRewindOnFall && rb != null && rb.position.y < fallYThreshold && !isRestoring)
        {
            StartCoroutine(HandleAutoFallRestore());
        }
    }

    private void AddHistorySample()
    {
        var s = new State
        {
            position = rb != null ? rb.position : transform.position,
            rotation = rb != null ? rb.rotation : transform.rotation,
            velocity = rb != null ? rb.linearVelocity : Vector3.zero,
            time = Time.time
        };

        history.Add(s);

        // Update lastSafeState heuristics:
        // Consider the sample 'safe' if it is above the fall threshold and not rapidly falling
        bool notRapidlyFalling = rb == null || rb.linearVelocity.y > -5f;
        if (s.position.y > fallYThreshold + 1f && notRapidlyFalling)
        {
            lastSafeState = s;
            hasLastSafeState = true;
        }
    }

    private void TrimHistory()
    {
        float cutoff = Time.time - historyDuration;
        if (history.Count == 0) return;

        int removeCount = 0;
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].time < cutoff) removeCount++;
            else break;
        }
        if (removeCount > 0)
            history.RemoveRange(0, removeCount);
    }

    // Public API used by ObstacleController
    public void TakeHit() => TakeDamage(damagePerHit);

    public void TakeDamage(float fraction)
    {
        if (fraction <= 0f) return;
        if (health <= 0f) return;
        if (isInvulnerable || isPowerUpInvulnerable) return;

        // Apply health change
        health = Mathf.Clamp01(health - fraction);
        OnHealthChanged?.Invoke(health);
        if (healthSlider != null) healthSlider.value = health;

        // Camera feedback
        if (CameraShake.Instance != null) CameraShake.Instance.Shake();

        //audio feedback (avatar-specific, with fallback)
        if (audioSource != null)
        {
            AudioClip clipToPlay = null;

            //1) Prefer: avatar audio
            if (audioRuntime != null &&
                audioRuntime.Current != null &&
                audioRuntime.Current.impactClip != null)
            {
                clipToPlay = audioRuntime.Current.impactClip;
            }
            //2) Fallback: inspector sound(adica cat-hurt-sound)
            else if (damageSound != null)
            {
                clipToPlay = damageSound;
            }

            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }


        // Rewind player a few seconds back
        RestoreToSecondsAgo(rewindSeconds);

        // Start invulnerability/stun and handle lethal case after invulnerability
        bool lethal = health <= 0f;
        StartCoroutine(HandleInvulnerabilityAndPossibleGameOver(lethal));
    }

    private IEnumerator HandleInvulnerabilityAndPossibleGameOver(bool lethal)
    {
        isInvulnerable = true;
        if (disableMovementOnHit && movementComponent != null)
            movementComponent.enabled = false;

        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!lethal)
        {
            if (disableMovementOnHit && movementComponent != null)
                movementComponent.enabled = true;
            isInvulnerable = false;
        }
        else
        {
            GameManager.Instance?.GameOver();
        }
    }

    private IEnumerator HandleAutoFallRestore()
    {
        isRestoring = true;

        // Apply damage for falling (same as hitting an obstacle)
        health = Mathf.Clamp01(health - damagePerHit);
        OnHealthChanged?.Invoke(health);
        if (healthSlider != null) healthSlider.value = health;

        // Check if this fall was lethal
        bool lethal = health <= 0f;

        // Camera feedback
        if (CameraShake.Instance != null) CameraShake.Instance.Shake();

        // Store original kinematic state
        bool wasKinematic = rb != null && rb.isKinematic;

        // Find the best restore position BEFORE changing physics state
        Vector3 restorePos;
        Quaternion restoreRot;
        FindBestRestoreState(out restorePos, out restoreRot);

        // Ensure restore position is valid - if not, force it above threshold
        if (restorePos.y < fallYThreshold)
        {
            restorePos.y = fallYThreshold + 2f;
            Debug.LogWarning($"PlayerHealth: No valid restore position found, forcing Y to {restorePos.y}");
        }

        // Apply the restore using transform (more reliable than rb.position when kinematic)
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = restorePos;
        transform.rotation = restoreRot;

        // Sync rigidbody position
        if (rb != null)
        {
            rb.position = restorePos;
            rb.rotation = restoreRot;
        }

        // Apply short invulnerability and optionally disable movement
        isInvulnerable = true;
        if (disableMovementOnHit && movementComponent != null)
            movementComponent.enabled = false;

        float elapsed = 0f;
        while (elapsed < invulnerabilityDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore physics state
        if (rb != null && !wasKinematic)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        // Update lastSafeState to current position after successful restore
        Vector3 currentPos = rb != null ? rb.position : transform.position;
        if (currentPos.y >= fallYThreshold)
        {
            lastSafeState = new State
            {
                position = currentPos,
                rotation = rb != null ? rb.rotation : transform.rotation,
                velocity = Vector3.zero,
                time = Time.time
            };
            hasLastSafeState = true;

            // Clear old history to prevent restoring to pre-fall states
            history.Clear();
            history.Add(lastSafeState);
        }

        if (lethal)
        {
            // Player died from fall
            GameManager.Instance?.GameOver();
        }
        else
        {
            if (disableMovementOnHit && movementComponent != null)
                movementComponent.enabled = true;
            isInvulnerable = false;
        }
        isRestoring = false;
    }

    private void FindBestRestoreState(out Vector3 position, out Quaternion rotation)
    {
        // First try: find a valid state from history (2 seconds ago)
        float targetTime = Time.time - rewindSeconds;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].time <= targetTime && history[i].position.y >= fallYThreshold)
            {
                position = history[i].position;
                rotation = history[i].rotation;
                return;
            }
        }

        // Second try: find ANY valid state from history
        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].position.y >= fallYThreshold)
            {
                position = history[i].position;
                rotation = history[i].rotation;
                return;
            }
        }

        // Third try: use lastSafeState if valid
        if (hasLastSafeState && lastSafeState.position.y >= fallYThreshold)
        {
            position = lastSafeState.position;
            rotation = lastSafeState.rotation;
            return;
        }

        // Last resort: use current position (will be corrected by caller)
        position = rb != null ? rb.position : transform.position;
        rotation = rb != null ? rb.rotation : transform.rotation;
    }

    /// Restores the player's transform to the recorded state from secondsAgo
    /// Uses history when possible, falls back to lastSafeState if necessary
    private void RestoreToSecondsAgo(float secondsAgo)
    {
        if (history.Count == 0 && hasLastSafeState)
        {
            ApplyState(lastSafeState);
            return;
        }
        if (history.Count == 0) return;

        float targetTime = Time.time - secondsAgo;

        // If target is before first sample, use first
        if (targetTime <= history[0].time)
        {
            ApplySafeOrFallback(history[0]);
            return;
        }

        // If target is after last sample, use last
        if (targetTime >= history[history.Count - 1].time)
        {
            ApplySafeOrFallback(history[history.Count - 1]);
            return;
        }

        int idx = history.FindIndex(s => s.time >= targetTime);
        if (idx <= 0)
        {
            ApplySafeOrFallback(history[0]);
            return;
        }

        State a = history[idx - 1];
        State b = history[idx];

        float t = Mathf.InverseLerp(a.time, b.time, targetTime);
        Vector3 pos = Vector3.Lerp(a.position, b.position, t);
        Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);
        Vector3 vel = Vector3.Lerp(a.velocity, b.velocity, t);

        ApplySafeOrFallback(new State { position = pos, rotation = rot, velocity = vel, time = targetTime });
    }

    // If the computed state appears to be below the fall threshold or otherwise unsafe, fall back to lastSafeState
    private void ApplySafeOrFallback(State s)
    {
        // If computed state is valid, use it
        if (s.position.y >= fallYThreshold)
        {
            ApplyState(s);
            return;
        }

        // Try lastSafeState if valid
        if (hasLastSafeState && lastSafeState.position.y >= fallYThreshold)
        {
            State safeStateWithZeroVelocity = lastSafeState;
            safeStateWithZeroVelocity.velocity = Vector3.zero;
            ApplyState(safeStateWithZeroVelocity);
            return;
        }

        // Last resort: use computed state but force Y above threshold
        Debug.LogWarning("PlayerHealth: No safe state available, forcing position above threshold.");
        State forced = s;
        forced.position.y = fallYThreshold + 2f;
        forced.velocity = Vector3.zero;
        ApplyState(forced);
    }

    private void ApplyState(State s)
    {
        // Always set transform position first (more reliable)
        transform.position = s.position;
        transform.rotation = s.rotation;

        // Then sync rigidbody if available
        if (rb != null)
        {
            rb.position = s.position;
            rb.rotation = s.rotation;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = s.velocity;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void ResetHealth()
    {
        health = 1f;
        OnHealthChanged?.Invoke(health);
        if (healthSlider != null) 
            healthSlider.value = health;
        isInvulnerable = false;
        isRestoring = false;
        if (disableMovementOnHit && movementComponent != null)
            movementComponent.enabled = true;

        // Reset history/last safe state to current transform so subsequent falls behave predictably
        history.Clear();
        var init = new State
        {
            position = rb != null ? rb.position : transform.position,
            rotation = rb != null ? rb.rotation : transform.rotation,
            velocity = rb != null ? rb.linearVelocity : Vector3.zero,
            time = Time.time
        };
        history.Add(init);
        lastSafeState = init;
        hasLastSafeState = true;
    }

    public bool IsInvulnerable() => isInvulnerable || isPowerUpInvulnerable;

    public void SetInvulnerable(bool state)
    {
        isPowerUpInvulnerable = state;
    }
}