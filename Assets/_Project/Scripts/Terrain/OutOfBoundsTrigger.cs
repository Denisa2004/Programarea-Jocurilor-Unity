using UnityEngine;
using UnityEngine.SceneManagement;

// script that resets the player when falling off the road
public class OutOfBoundsTrigger : MonoBehaviour
{
    [HideInInspector]
    public InfiniteRoadGenerator generator;
    
    [Header("Settings")]
    [Tooltip("Delay before restart/reset in seconds")]
    public float resetDelay = 0.5f;
    
    [Tooltip("Reset method: true = reload scene, false = respawn at last position")]
    public bool reloadScene = true;
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Player fell out of bounds!");
            
            var playerController = other.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                // disable movement
                other.GetComponent<Rigidbody>()?.Sleep();
            }
            
            if (reloadScene)
            {
                Invoke(nameof(ReloadCurrentScene), resetDelay);
            }
            else
            {
                Invoke(nameof(RespawnPlayer), resetDelay);
            }
        }
    }
    
    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void RespawnPlayer()
    {
        if (generator != null && generator.player != null)
        {
            // first segment as respawn point
            var segments = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var segment in segments)
            {
                if (segment.name.StartsWith("Road_Straight_") || segment.name.StartsWith("Road_Corner_"))
                {
                    generator.player.position = segment.position + Vector3.up * 3f;
                    generator.player.rotation = segment.rotation;
                    
                    var rb = generator.player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    
                    break;
                }
            }
        }
        
        hasTriggered = false;
    }
}
