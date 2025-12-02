using UnityEngine;

// Un segment i.e. StraightSegment are NewSectionTrigger care are un Box Collider care se afla la mijlocul segmentului

// Scopul acestui Script e ca atunci cand Collider-ul jucatorului interactioneaza cu collider-ul platformei, generam segment nou

public class SectionTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        // Debug: verificam ce obiect a intrat in trigger
        Debug.Log($"SectionTrigger: Object entered trigger - {other.gameObject.name}, Tag: {other.gameObject.tag}");
        
        // Verificam daca jucatorul a intrat in trigger
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("SectionTrigger: Player detected, attempting to generate next segment...");
            
            // Obtinem SegmentGenerator de la segmentul curent (parintele trigger-ului)
            Transform segmentParent = transform.parent;
            Debug.Log($"SectionTrigger: Parent is {(segmentParent != null ? segmentParent.name : "NULL")}");
            
            if (segmentParent != null)
            {
                Debug.Log($"SectionTrigger: Checking for SegmentGenerator on {segmentParent.name}...");
                SegmentGenerator generator = segmentParent.GetComponent<SegmentGenerator>();
                
                // Daca nu exista, incercam sa o adaugam automat
                if (generator == null)
                {
                    Debug.Log($"SectionTrigger: SegmentGenerator not found, adding it to {segmentParent.name}");
                    generator = segmentParent.gameObject.AddComponent<SegmentGenerator>();
                    Debug.Log($"SectionTrigger: SegmentGenerator added: {(generator != null ? "SUCCESS" : "FAILED")}");
                    
                    // Asiguram ca avem si TerrainSegment
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
                
                // Re-verificam dupa adaugare
                generator = segmentParent.GetComponent<SegmentGenerator>();
                if (generator != null)
                {
                    // Verificam daca prefab-urile sunt asignate
                    if (generator.straightSegmentPrefab == null)
                    {
                        Debug.LogWarning("SectionTrigger: straightSegmentPrefab is not assigned! Please assign prefabs in Inspector.");
                        return;
                    }
                    
                    // Generam urmatorul segment folosind logica din SegmentGenerator
                    generator.GenerateNextSegment();
                    Debug.Log("SectionTrigger: Segment generation called successfully!");
                    
                    // Rotim jucatorul pentru a se alinia cu directia segmentului
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
        // Obtinem directia segmentului (forward direction)
        Vector3 segmentForward = segment.forward;
        
        // Obtinem componenta PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Rotim jucatorul smooth pentru a se alinia cu directia segmentului
            Quaternion targetRotation = Quaternion.LookRotation(segmentForward);
            
            // Folosim coroutine pentru rotire smooth
            StartCoroutine(SmoothRotatePlayer(player.transform, targetRotation));
            
            Debug.Log($"SectionTrigger: Rotating player smoothly to match segment direction: {segmentForward}");
        }
    }
    
    private System.Collections.IEnumerator SmoothRotatePlayer(Transform playerTransform, Quaternion targetRotation)
    {
        Quaternion startRotation = playerTransform.rotation;
        float duration = 0.2f; // Durata rotirii in secunde (redusa pentru mai putina miscare)
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        
        // Asiguram ca rotatia finala este exacta
        playerTransform.rotation = targetRotation;
    }
}