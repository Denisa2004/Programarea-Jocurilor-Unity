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
            // Run coroutine on player object
            MonoBehaviour playerMono = other.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                playerMono.StartCoroutine(ActivatePowerUp(other));
            }
            
            // Hide and destroy powerup
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
            
            Destroy(gameObject, 0.1f);
        }
    }

    private IEnumerator ActivatePowerUp(Collider player)
    {
        Debug.Log("InvincibilityPowerUp: Activating effects");
        
        // Apply effects
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerController controller = player.GetComponent<PlayerController>();
        
        // Save original speed
        float originalSpeedMultiplier = 1f;
        if (controller != null)
        {
            originalSpeedMultiplier = controller.GetSpeedMultiplier();
        }

        if (health != null)
        {
            health.SetInvulnerable(true);
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
            // Restore original speed
            controller.SetSpeedMultiplier(originalSpeedMultiplier);
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

        Debug.Log("InvincibilityPowerUp: Effects expired, restored to normal");
    }
}
