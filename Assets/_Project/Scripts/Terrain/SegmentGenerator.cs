using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SegmentGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int minGenerations = 3;
    public int maxGenerations = 0;
    
    [Header("Segment Prefabs")]
    public GameObject straightSegmentPrefab;
    
    [Header("Generation Probability")]
    [Range(0f, 1f)]
    public float turnProbability = 0.3f;

    [Header("Manual Turn Offsets (Center to Center)")]
    [Tooltip("Exact coordinate difference between the center of the old piece and the center of the new piece.")]
    // Here we put EXACTLY the requested values: 11 on X, 19 on Z
    public Vector3 rightTurnOffset = new Vector3(11f, 0f, 19f);

    [Tooltip("Exact coordinate difference between the center of the old piece and the center of the new piece.")]
    // For left, X is negative
    public Vector3 leftTurnOffset = new Vector3(-11f, 0f, 19f);

    private static int generationCount = 0;
    private static float currentRotationY = 0f;
    
    private void Awake()
    {
        TerrainSegment terrainSegment = GetComponent<TerrainSegment>();
        if (terrainSegment == null)
        {
            terrainSegment = gameObject.AddComponent<TerrainSegment>();
            terrainSegment.segmentType = SegmentType.Straight;
        }
    }
    
    private static bool hasResetCount = false;
    
    private void Start()
    {
        if (!hasResetCount && (transform.parent == null || !transform.parent.name.Contains("Clone")))
        {
            generationCount = 0;
            hasResetCount = true;
        }
        
        if (straightSegmentPrefab == null) TryFindPrefabs();
    }
    
    private void TryFindPrefabs()
    {
        #if UNITY_EDITOR
        if (straightSegmentPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("StraightSegment t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                straightSegmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        #else
        if (straightSegmentPrefab == null)
        {
            straightSegmentPrefab = Resources.Load<GameObject>("StraightSegment");
            if (straightSegmentPrefab == null)
            {
                GameObject found = GameObject.Find("StraightSegment");
                if (found != null && found != gameObject) straightSegmentPrefab = found;
            }
        }
        #endif
    }
    
    public void GenerateNextSegment()
    {
        if (maxGenerations > 0 && generationCount >= maxGenerations) return;
        
        generationCount++;
        
        // Required data
        TerrainSegment currentSegment = GetComponent<TerrainSegment>();
        Quaternion currentRotation = transform.rotation; // Current segment rotation
        Vector3 currentPosition = transform.position;    // Current segment position (center)
        
        // For ExitPoint calculation (necessary ONLY when going straight)
        Vector3 exitPosition = currentSegment != null ? currentSegment.GetExitPosition() : transform.position + transform.forward * 30f;
        Quaternion exitRotation = currentSegment != null ? currentSegment.GetExitRotation() : transform.rotation;

        bool shouldTurn = false;
        float turnAngle = 0f;
        
        // Random Turn Logic
        if (generationCount >= minGenerations)
        {
            float randomValue = Random.Range(0f, 1f);
            if (randomValue < turnProbability)
            {
                shouldTurn = true;
                turnAngle = Random.Range(0f, 1f) < 0.5f ? 90f : -90f;
                currentRotationY += turnAngle;
            }
        }
        
        GameObject segmentToSpawn = straightSegmentPrefab;
        if (segmentToSpawn == null) return;
        
        // Calculate Final Rotation
        Quaternion finalRotation = exitRotation * Quaternion.Euler(0, turnAngle, 0);
        Vector3 spawnPosition;

        if (shouldTurn)
        {
            // TURN CASE: Calculate relative to PIVOT (Center), not to ExitPoint.
            // This guarantees that the coordinate difference is exactly (11, 19) or (-11, 19)
            
            Vector3 localOffset = (turnAngle < 0) ? rightTurnOffset : leftTurnOffset;
            
            // Rotate the offset based on the orientation of the current segment
            // If the current segment is rotated, the offset rotates with it
            Vector3 worldOffset = currentRotation * localOffset;
            
            // Add the offset to the CENTRAL position of the old segment
            spawnPosition = currentPosition + worldOffset;
            
            Debug.Log($"Turn {(turnAngle < 0 ? "RIGHT" : "LEFT")}. Old: {currentPosition}, New: {spawnPosition}. Difference: {spawnPosition - currentPosition}");
        }
        else
        {
            // STRAIGHT CASE: the StartPoint -> ExitPoint logic 
            Transform startPoint = segmentToSpawn.transform.Find("StartPoint");
            Vector3 startPointLocalPos = startPoint != null ? startPoint.localPosition : new Vector3(0, 0, -15f);
            Vector3 startPointWorldOffset = finalRotation * startPointLocalPos;
            
            spawnPosition = exitPosition - startPointWorldOffset;
        }
        // ------------------------------
        
        // Instantiate the segment
        GameObject newSegment = Instantiate(segmentToSpawn, spawnPosition, finalRotation);
        
        // Alignment correction ONLY for straight movement (for turns we have fixed coordinates)
        if (!shouldTurn)
        {
            Transform newStartPoint = newSegment.transform.Find("StartPoint");
            if (newStartPoint != null)
            {
                Vector3 correction = exitPosition - newStartPoint.position;
                if (correction.magnitude > 0.001f)
                {
                    newSegment.transform.position += correction;
                }
            }
        }
        
        // Setup the new generator
        SegmentGenerator newGenerator = newSegment.GetComponent<SegmentGenerator>();
        if (newGenerator == null) newGenerator = newSegment.AddComponent<SegmentGenerator>();
        
        newGenerator.minGenerations = this.minGenerations;
        newGenerator.maxGenerations = this.maxGenerations;
        newGenerator.straightSegmentPrefab = this.straightSegmentPrefab;
        newGenerator.turnProbability = this.turnProbability;
        newGenerator.rightTurnOffset = this.rightTurnOffset;
        newGenerator.leftTurnOffset = this.leftTurnOffset;
    }
    
    // Reset Button
    [ContextMenu("Reset Offsets to 11/19")]
    public void ResetOffsets()
    {
        rightTurnOffset = new Vector3(11f, 0f, 19f);
        leftTurnOffset = new Vector3(-11f, 0f, 19f);
        Debug.Log("Offsets reset to X=11, Z=19!");
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }
    
    public static void ResetGenerationCount()
    {
        generationCount = 0;
        hasResetCount = false;
        currentRotationY = 0f;
    }
    
    public static int GetGenerationCount() => generationCount;
}