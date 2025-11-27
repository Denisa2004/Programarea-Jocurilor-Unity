using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object that collided is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with an obstacle!");

            // Trigger camera shake (visual feedback)
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(); // uses default duration & magnitude
            }

            // Trigger game over logic
            GameManager.Instance.GameOver();
        }
    }
}
