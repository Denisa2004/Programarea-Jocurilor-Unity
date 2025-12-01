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

    public event Action<float> OnHealthChanged;

    private Rigidbody rb;

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
    private bool isRestoring = false;

    // The most recent 'safe' recorded state (used as a fallback if rewind samples are invalid)
    private State lastSafeState;
    private bool hasLastSafeState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rb = GetComponent<Rigidbody>();
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
            velocity = rb != null ? rb.velocity : Vector3.zero,
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
            velocity = rb != null ? rb.velocity : Vector3.zero,
            time = Time.time
        };

        history.Add(s);

        // Update lastSafeState heuristics:
        // Consider the sample 'safe' if it is above the fall threshold and not rapidly falling
        bool notRapidlyFalling = rb == null || rb.velocity.y > -5f;
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
        if (isInvulnerable) return;

        // Apply health change
        health = Mathf.Clamp01(health - fraction);
        OnHealthChanged?.Invoke(health);
        if (healthSlider != null) healthSlider.value = health;

        // Camera feedback
        if (CameraShake.Instance != null) CameraShake.Instance.Shake();

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

        // Prevent immediate re-triggers
        RestoreToSecondsAgo(rewindSeconds);

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

        if (disableMovementOnHit && movementComponent != null)
            movementComponent.enabled = true;
        isInvulnerable = false;
        isRestoring = false;
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
        if (s.position.y < fallYThreshold && hasLastSafeState)
        {
            ApplyState(lastSafeState);
        }
        else
        {
            ApplyState(s);
        }
    }

    private void ApplyState(State s)
    {
        if (rb != null)
        {
            rb.position = s.position;
            rb.rotation = s.rotation;
            rb.velocity = s.velocity;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = s.position;
            transform.rotation = s.rotation;
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
            velocity = rb != null ? rb.velocity : Vector3.zero,
            time = Time.time
        };
        history.Add(init);
        lastSafeState = init;
        hasLastSafeState = true;
    }

    public bool IsInvulnerable() => isInvulnerable;
}