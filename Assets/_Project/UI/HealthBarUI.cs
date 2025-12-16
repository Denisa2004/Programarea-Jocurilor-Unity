using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    public bool enforceZeroToOne = true;
    public float lerpSpeed = 4f;

    private Slider slider;
    private float targetValue = 1f; // start full by default
    private bool subscribed = false;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider != null)
        {
            if (enforceZeroToOne)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
            slider.value = targetValue;
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (subscribed && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged -= OnHealthChanged;
            subscribed = false;
        }
    }

    private void Start()
    {
        // In case PlayerHealth wasn't ready at OnEnable/Awake, try again in Start
        TrySubscribe();
    }

    private void Update()
    {
        if (slider == null) return;
        if (lerpSpeed <= 0f)
        {
            slider.value = targetValue;
        }
        else
        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, lerpSpeed * Time.unscaledDeltaTime);
        }
    }

    private void TrySubscribe()
    {
        if (subscribed) return;

        if (PlayerHealth.Instance != null)
        {
            // subscribe to the singleton if available
            PlayerHealth.Instance.OnHealthChanged += OnHealthChanged;
            subscribed = true;
            SetImmediate(PlayerHealth.Instance.health);
            return;
        }

        // fallback: find any PlayerHealth in the scene and subscribe
        var ph = FindObjectOfType<PlayerHealth>();
        if (ph != null)
        {
            ph.OnHealthChanged += OnHealthChanged;
            subscribed = true;
            SetImmediate(ph.health);
            return;
        }

        // If no PlayerHealth exists yet, keep the bar full (targetValue already 1)
        SetImmediate(targetValue);
    }

    private void OnHealthChanged(float value)
    {
        // use value directly so bar empties as health decreases
        targetValue = Mathf.Clamp01(value);
    }

    private void SetImmediate(float value)
    {
        targetValue = Mathf.Clamp01(value);
        if (slider != null)
            slider.value = targetValue;
    }
}