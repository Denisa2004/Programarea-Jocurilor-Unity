using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPosition;
    private float shakeDuration = 0f;

    [SerializeField] private float shakeAmount = 0.6f;   // Intensity of the shake
    [SerializeField] private float shakeTime = 0.4f;     // Duration of the shake

    private void Awake()
    {
        Instance = this;
        originalPosition = transform.localPosition;
    }

    // Use LateUpdate so the shake overrides any camera movement script
    private void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.unscaledDeltaTime;

        }
        else
        {
            // Restore camera to its original position
            transform.localPosition = originalPosition;
        }
    }

    public void Shake()
    {
        shakeDuration = shakeTime; // Start shake with fixed duration
        
    }
}
