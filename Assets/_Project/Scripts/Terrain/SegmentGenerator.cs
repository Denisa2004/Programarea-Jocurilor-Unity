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
    [Tooltip("Diferenta exacta de coordonate intre centrul piesei vechi si centrul piesei noi.")]
    // Aici punem EXACT valorile cerute: 11 pe X, 19 pe Z
    public Vector3 rightTurnOffset = new Vector3(11f, 0f, 19f); 

    [Tooltip("Diferenta exacta de coordonate intre centrul piesei vechi si centrul piesei noi.")]
    // Pentru stanga, X este negativ
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
        
        // Date necesare
        TerrainSegment currentSegment = GetComponent<TerrainSegment>();
        Quaternion currentRotation = transform.rotation; // Rotatia segmentului curent
        Vector3 currentPosition = transform.position;    // Pozitia (centrul) segmentului curent
        
        // Pentru calculul ExitPoint (necesar DOAR la mers drept)
        Vector3 exitPosition = currentSegment != null ? currentSegment.GetExitPosition() : transform.position + transform.forward * 30f;
        Quaternion exitRotation = currentSegment != null ? currentSegment.GetExitRotation() : transform.rotation;

        bool shouldTurn = false;
        float turnAngle = 0f;
        
        // Logica Random Turn
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
        
        // Calcul Rotatie Finala
        Quaternion finalRotation = exitRotation * Quaternion.Euler(0, turnAngle, 0);
        Vector3 spawnPosition;

        // --- LOGICA MODIFICATA AICI ---
        if (shouldTurn)
        {
            // CAZ VIRAJ: Calculam relativ la PIVOT (Centru), nu la ExitPoint.
            // Asta garanteaza ca diferenta de coordonate este exact (11, 19) sau (-11, 19)
            
            Vector3 localOffset = (turnAngle < 0) ? rightTurnOffset : leftTurnOffset;
            
            // Rotim offset-ul in functie de orientarea segmentului curent
            // Daca segmentul curent e rotit, si offset-ul se roteste cu el
            Vector3 worldOffset = currentRotation * localOffset;
            
            // Adaugam offset-ul la pozitia CENTRALA a segmentului vechi
            spawnPosition = currentPosition + worldOffset;
            
            Debug.Log($"Viraj {(turnAngle < 0 ? "DREAPTA" : "STANGA")}. Vechi: {currentPosition}, Nou: {spawnPosition}. Diferenta: {spawnPosition - currentPosition}");
        }
        else
        {
            // CAZ DREPT: Ramanem la logica StartPoint -> ExitPoint (care mergea bine)
            Transform startPoint = segmentToSpawn.transform.Find("StartPoint");
            Vector3 startPointLocalPos = startPoint != null ? startPoint.localPosition : new Vector3(0, 0, -15f);
            Vector3 startPointWorldOffset = finalRotation * startPointLocalPos;
            
            spawnPosition = exitPosition - startPointWorldOffset;
        }
        // ------------------------------
        
        // Instantiem segmentul
        GameObject newSegment = Instantiate(segmentToSpawn, spawnPosition, finalRotation);
        
        // Corectie aliniere DOAR pentru mers drept (la viraj avem coordonatele fixe)
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
        
        // Setup noul generator
        SegmentGenerator newGenerator = newSegment.GetComponent<SegmentGenerator>();
        if (newGenerator == null) newGenerator = newSegment.AddComponent<SegmentGenerator>();
        
        newGenerator.minGenerations = this.minGenerations;
        newGenerator.maxGenerations = this.maxGenerations;
        newGenerator.straightSegmentPrefab = this.straightSegmentPrefab;
        newGenerator.turnProbability = this.turnProbability;
        newGenerator.rightTurnOffset = this.rightTurnOffset;
        newGenerator.leftTurnOffset = this.leftTurnOffset;
    }
    
    // Buton de Reset
    [ContextMenu("Reset Offsets to 11/19")]
    public void ResetOffsets()
    {
        rightTurnOffset = new Vector3(11f, 0f, 19f);
        leftTurnOffset = new Vector3(-11f, 0f, 19f);
        Debug.Log("Offsets resetate la X=11, Z=19!");
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