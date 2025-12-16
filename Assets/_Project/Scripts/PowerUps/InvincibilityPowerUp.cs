using System.Collections;
using UnityEngine;

public class InvincibilityPowerUp : MonoBehaviour
{
    [Header("Power Up Settings")]
    public float duration = 5f;
    public float speedMultiplier = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivatePowerUp(other));
        }
    }

    private IEnumerator ActivatePowerUp(Collider player)
    {
        // Disable collider to prevent multiple triggers
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Disable renderer to hide the object (make it look collected)
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        // Apply effects
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerController controller = player.GetComponent<PlayerController>();

        if (health != null)
        {
            health.SetInvulnerable(true);
        }

        if (controller != null)
        {
            controller.SetSpeedMultiplier(speedMultiplier);
        }

        if (controller != null)
        {
            controller.SetSpeedMultiplier(speedMultiplier);
        }

        // PHANTOM MODE: Ignore collisions with all ObstacleControllers
        Collider playerCollider = player.GetComponent<Collider>();
        ObstacleController[] obstacles = FindObjectsByType<ObstacleController>(FindObjectsSortMode.None);
        if (playerCollider != null)
        {
            foreach (var obs in obstacles)
            {
                Collider obsVal = obs.GetComponent<Collider>();
                if (obsVal != null)
                    Physics.IgnoreCollision(playerCollider, obsVal, true);
            }
        }

        // Wait for the duration of the power-up
        yield return new WaitForSeconds(duration);

        // Remove effects
        if (health != null)
        {
            health.SetInvulnerable(false);
        }

        if (controller != null)
        {
            controller.SetSpeedMultiplier(1f);
        }

        // Restore collisions
        if (playerCollider != null)
        {
            foreach (var obs in obstacles)
            {
                if (obs != null) // Check if obstacle still exists
                {
                    Collider obsVal = obs.GetComponent<Collider>();
                    if (obsVal != null)
                       Physics.IgnoreCollision(playerCollider, obsVal, false);
                }
            }
        }

        // Destroy the power-up object
        Destroy(gameObject);
    }
}
