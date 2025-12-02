using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public bool disableAfterHit = true;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object that collided is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with an obstacle!");

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeHit();
            }
            else
            {
                // Trigger camera shake (visual feedback)
                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(); // uses default duration & magnitude

                // Trigger game over logic
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }

            if (disableAfterHit)
            {
                // Disable further collisions from this obstacle.
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // Optionally destroy after a short delay 
                Destroy(gameObject, 0.1f);
            }
        }
    }
}