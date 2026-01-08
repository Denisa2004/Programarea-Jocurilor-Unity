using UnityEngine;

// A segment (e.g. StraightSegment) has NewSectionTrigger which has a Box Collider located at the middle of the segment

// The purpose of this Script is that when the player's Collider interacts with the platform's collider, we generate a new segment

public class SectionTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        // Debug: check what object entered the trigger
        Debug.Log($"SectionTrigger: Object entered trigger - {other.gameObject.name}, Tag: {other.gameObject.tag}");
        
        // Check if the player entered the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("SectionTrigger: Player detected, attempting to generate next segment...");
            
            // Get SegmentGenerator from the current segment (the trigger's parent)
            Transform segmentParent = transform.parent;
            Debug.Log($"SectionTrigger: Parent is {(segmentParent != null ? segmentParent.name : "NULL")}");
            
            if (segmentParent != null)
            {
                Debug.Log($"SectionTrigger: Checking for SegmentGenerator on {segmentParent.name}...");
                SegmentGenerator generator = segmentParent.GetComponent<SegmentGenerator>();
                
                // If it doesn't exist, we try to add it automatically
                if (generator == null)
                {
                    Debug.Log($"SectionTrigger: SegmentGenerator not found, adding it to {segmentParent.name}");
                    generator = segmentParent.gameObject.AddComponent<SegmentGenerator>();
                    Debug.Log($"SectionTrigger: SegmentGenerator added: {(generator != null ? "SUCCESS" : "FAILED")}");
                    
                    // Ensure we also have TerrainSegment
                    TerrainSegment terrainSegment = segmentParent.GetComponent<TerrainSegment>();
                    if (terrainSegment == null)
                    {
                        Debug.Log($"SectionTrigger: Adding TerrainSegment to {segmentParent.name}");
                        terrainSegment = segmentParent.gameObject.AddComponent<TerrainSegment>();
                        terrainSegment.segmentType = SegmentType.Straight;
                    }
                }
                else
                {
                    Debug.Log($"SectionTrigger: SegmentGenerator found on {segmentParent.name}");
                }
                
                // Re-check after adding
                generator = segmentParent.GetComponent<SegmentGenerator>();
                if (generator != null)
                {
                    // Check if prefabs are assigned
                    if (generator.straightSegmentPrefab == null)
                    {
                        Debug.LogWarning("SectionTrigger: straightSegmentPrefab is not assigned! Please assign prefabs in Inspector.");
                        return;
                    }
                    
                    // Generate the next segment using the logic from SegmentGenerator
                    generator.GenerateNextSegment();
                    Debug.Log("SectionTrigger: Segment generation called successfully!");
                    
                    // Rotate the player to align with the segment direction
                    RotatePlayerToSegmentDirection(other.gameObject, segmentParent);
                }
                else
                {
                    Debug.LogError($"SectionTrigger: Failed to add SegmentGenerator to {segmentParent.name}!");
                }
            }
            else
            {
                Debug.LogWarning("SectionTrigger: No parent found! Trigger must be a child of the segment.");
            }
        }
    }
    
    private void RotatePlayerToSegmentDirection(GameObject player, Transform segment)
    {
        // Get the segment direction (forward direction)
        Vector3 segmentForward = segment.forward;
        
        // Get the PlayerController component
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Smoothly rotate the player to align with the segment direction
            Quaternion targetRotation = Quaternion.LookRotation(segmentForward);
            
            // Use coroutine for smooth rotation
            StartCoroutine(SmoothRotatePlayer(player.transform, targetRotation));
            
            Debug.Log($"SectionTrigger: Rotating player smoothly to match segment direction: {segmentForward}");
        }
    }
    
    private System.Collections.IEnumerator SmoothRotatePlayer(Transform playerTransform, Quaternion targetRotation)
    {
        Quaternion startRotation = playerTransform.rotation;
        float duration = 0.2f; // Rotation duration in seconds (reduced for less movement)
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        
        // Ensure the final rotation is exact
        playerTransform.rotation = targetRotation;
    }
}