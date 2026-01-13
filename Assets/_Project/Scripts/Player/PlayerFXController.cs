using System.Collections;
using UnityEngine;

public class PlayerFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelTransform;     // mesh/model child (pentru lean)
    [SerializeField] private Transform groundFxPoint;      // punct la sol
    [SerializeField] private Transform trailAnchor;        // punct pentru trail
    [SerializeField] private AudioSource sfxSource;

    [Header("VFX Prefabs")]
    [SerializeField] private ParticleSystem jumpDustPrefab;
    [SerializeField] private ParticleSystem turnDustPrefab;
    [SerializeField] private ParticleSystem hitStarsPrefab;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpBoingClip;
    [SerializeField] private AudioClip turnSwishClip;

    [Header("Turn Lean")]
    [SerializeField] private float leanAngle = 15f;
    [SerializeField] private float leanInTime = 0.08f;
    [SerializeField] private float leanOutTime = 0.12f;

    [Header("Audio Variation")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Trail")]
    [SerializeField] private float trailDuration = 0.18f;
    [SerializeField] private float trailStartWidth = 0.12f;
    [SerializeField] private float trailEndWidth = 0.0f;

    private TrailRenderer jumpTrail;
    private Coroutine leanRoutine;

    void Awake()
    {
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();

        if (trailAnchor != null)
        {
            jumpTrail = trailAnchor.GetComponent<TrailRenderer>();
            if (jumpTrail == null) jumpTrail = trailAnchor.gameObject.AddComponent<TrailRenderer>();

            jumpTrail.time = 0f;
            jumpTrail.startWidth = trailStartWidth;
            jumpTrail.endWidth = trailEndWidth;
            jumpTrail.emitting = false;
            jumpTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            jumpTrail.receiveShadows = false;
        }
    }

    //JUMP
    public void PlayJumpFX()
    {
        PlayOneShot(jumpBoingClip);
        SpawnVFX(jumpDustPrefab, GetGroundPoint(), Quaternion.identity);

        if (jumpTrail != null)
            StartCoroutine(EmitTrailBurst());
    }

    private IEnumerator EmitTrailBurst()
    {
        jumpTrail.Clear();
        jumpTrail.time = trailDuration;
        jumpTrail.emitting = true;

        yield return new WaitForSeconds(trailDuration);

        jumpTrail.emitting = false;
        jumpTrail.time = 0f;
    }

    //TURN
    //dir: -1 stanga, +1 dreapta
    public void PlayTurnFX(int dir)
    {
        PlayOneShot(turnSwishClip);
        SpawnVFX(turnDustPrefab, GetGroundPoint(), Quaternion.identity);

        if (modelTransform != null)
        {
            if (leanRoutine != null) StopCoroutine(leanRoutine);
            leanRoutine = StartCoroutine(LeanRoutine(dir));
        }
    }

    private IEnumerator LeanRoutine(int dir)
    {
        float targetZ = -dir * leanAngle;
        Quaternion startRot = modelTransform.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetZ);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, leanInTime);
            modelTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, leanOutTime);
            modelTransform.localRotation = Quaternion.Slerp(targetRot, startRot, t);
            yield return null;
        }

        modelTransform.localRotation = startRot;
        leanRoutine = null;
    }

    //IMPACT (stars only)
    public void PlayHitFX(Vector3 point, Vector3 normal)
    {
        SpawnVFX(hitStarsPrefab, point, Quaternion.LookRotation(normal));
    }

    //helpers
    private Vector3 GetGroundPoint()
    {
        if (groundFxPoint != null) return groundFxPoint.position;
        return transform.position;
    }

    private void SpawnVFX(ParticleSystem prefab, Vector3 pos, Quaternion rot)
    {
        if (!prefab) return;
        var ps = Instantiate(prefab, pos, rot);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (!clip || !sfxSource) return;
        sfxSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        sfxSource.PlayOneShot(clip);
    }
    public void StartJumpTrail()
    {
        if (jumpTrail == null) return;
        jumpTrail.Clear();
        jumpTrail.time = 0.35f;   //cat ramane trail-ul "in urma"
        jumpTrail.emitting = true;
    }

    public void StopJumpTrail()
    {
        if (jumpTrail == null) return;
        jumpTrail.emitting = false;
    }

}
