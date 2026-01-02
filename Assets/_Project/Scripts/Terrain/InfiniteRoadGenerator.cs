using UnityEngine;
using System.Collections.Generic;

public class InfiniteRoadGenerator : MonoBehaviour
{
    [Header("Road Segment Prefabs")]
    public GameObject straightPrefab;
    
    public GameObject cornerPrefab;
    
    [Header("Player Reference")]
    public Transform player;
    
    
    [Header("Generation Settings")]
    [Range(5, 10)]
    public int segmentsAhead = 7;
    
    [Range(0f, 1f)]
    public float turnChance = 0.25f;
    
    public int minStraightBetweenTurns = 3;
    
    
    [Header("Cleanup Settings")]
    public float deleteDelay = 1f;
    
    public float deleteDistanceBehind = 50f;
    
    
    [Header("Segment Dimensions")]
    public float segmentLength = 32.06f;
    
    [Header("Prefab Rotation")]
    public float prefabRotationYOffset = 90f;
    
    
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;
    
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.7f;
    
    [Range(0, 5)]
    public int minObstaclesPerSegment = 1;
    
    [Range(1, 10)]
    public int maxObstaclesPerSegment = 3;
    
    
    [Header("Coins")]
    public GameObject coinPrefab;
    
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.5f;
    
    [Range(1, 10)]
    public int minCoinsPerSegment = 3;
    
    [Range(1, 20)]
    public int maxCoinsPerSegment = 8;
    
    
    [Header("PowerUps")]
    public GameObject[] powerUpPrefabs;
    
    public float powerUpSpawnInterval = 20f;
    
    private float powerUpTimer = 0f;
    
    [Header("Right Turn Offsets")]
    public Vector3 rightCornerOffset = new Vector3(0f, 0f, 0f);
    
    public Vector3 afterRightStraightOffset = new Vector3(10.01f, 0f, 10.12f);
    
    
    [Header("Left Turn Offsets")]
    public Vector3 leftCornerOffset = new Vector3(-0.81f, 0f, 10.46f);
    
    public Vector3 afterLeftStraightOffset = new Vector3(-9.3f, 0f, 0f);
    
    
    private List<GameObject> activeSegments = new List<GameObject>();
    private Vector3 nextSpawnPosition;
    private Quaternion nextSpawnRotation;
    private int straightSegmentCount = 0;
    private int totalSegmentsGenerated = 0;
    private GameObject outOfBoundsPlane;
    
    [Header("Out of Bounds Settings")]
    public float outOfBoundsYOffset = -10f;
    
    public float outOfBoundsSize = 500f;
    
    [Header("Road Barriers")]
    public float barrierHeight = 3f;
    
    [Range(0.1f, 0.5f)]
    public float roadHalfWidth = 0.25f;
    
    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        nextSpawnPosition = transform.position;
        nextSpawnRotation = transform.rotation;
        
        CreateOutOfBoundsPlane();
        
        GenerateInitialSegments();
    }
    
    private void Update()
    {
        if (player == null) return;
        
        MaintainSegmentCount();
        
        CleanupOldSegments();
        
        UpdateOutOfBoundsPlane();
        
        UpdatePowerUpTimer();
    }
    
    private void CreateOutOfBoundsPlane()
    {
        outOfBoundsPlane = new GameObject("OutOfBoundsPlane");
        outOfBoundsPlane.transform.SetParent(transform);
        outOfBoundsPlane.transform.localRotation = Quaternion.identity;
        if (player != null)
        {
            outOfBoundsPlane.transform.position = new Vector3(
                player.position.x,
                transform.position.y + outOfBoundsYOffset,
                player.position.z
            );
        }
        else
        {
            outOfBoundsPlane.transform.localPosition = new Vector3(0, outOfBoundsYOffset, 0);
        }
        BoxCollider triggerCollider = outOfBoundsPlane.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(outOfBoundsSize, 1f, outOfBoundsSize);
        triggerCollider.center = Vector3.zero;
        OutOfBoundsTrigger trigger = outOfBoundsPlane.AddComponent<OutOfBoundsTrigger>();
        trigger.generator = this;
        
    }
    
    private void UpdateOutOfBoundsPlane()
    {
        if (outOfBoundsPlane != null && player != null)
        {
            outOfBoundsPlane.transform.position = new Vector3(
                player.position.x,
                transform.position.y + outOfBoundsYOffset,
                player.position.z
            );
        }
    }
    
    private void GenerateInitialSegments()
    {
        for (int i = 0; i < segmentsAhead; i++)
        {
            if (totalSegmentsGenerated < 3)
            {
                SpawnStraightSegment();
            }
            else
            {
                SpawnNextSegment();
            }
        }
        
    }
    
    private void MaintainSegmentCount()
    {
        while (activeSegments.Count < segmentsAhead)
        {
            SpawnNextSegment();
        }
    }
    
    private void CleanupOldSegments()
    {
        if (player == null || activeSegments.Count == 0) return;
        
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (GameObject segment in activeSegments)
        {
            if (segment == null)
            {
                toRemove.Add(segment);
                continue;
            }
            Vector3 toSegment = segment.transform.position - player.position;
            float distanceBehind = -Vector3.Dot(toSegment, player.forward);
            
            if (distanceBehind > deleteDistanceBehind)
            {
                toRemove.Add(segment);
            }
        }
        foreach (GameObject segment in toRemove)
        {
            activeSegments.Remove(segment);
            if (segment != null)
            {
                StartCoroutine(DestroyAfterDelay(segment, deleteDelay));
            }
        }
    }
    
    private System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    
    private void SpawnNextSegment()
    {
        bool shouldTurn = false;
        if (straightSegmentCount >= minStraightBetweenTurns)
        {
            if (Random.value < turnChance)
            {
                shouldTurn = true;
            }
        }
        
        if (shouldTurn)
        {
            bool turnRight = Random.value < 0.5f;
            SpawnTurn(turnRight);
        }
        else
        {
            SpawnStraightSegment();
        }
    }
    
    private void SpawnStraightSegment()
    {
        if (straightPrefab == null)
        {
            return;
        }
        Quaternion prefabRotation = nextSpawnRotation * Quaternion.Euler(0, prefabRotationYOffset, 0);
        GameObject newSegment = Instantiate(straightPrefab, nextSpawnPosition, prefabRotation);
        newSegment.name = $"Road_Straight_{totalSegmentsGenerated}";
        newSegment.transform.localScale = new Vector3(30f, 1f, 30f);
        SetupSegment(newSegment);
        
        activeSegments.Add(newSegment);
        straightSegmentCount++;
        totalSegmentsGenerated++;
        nextSpawnPosition += nextSpawnRotation * new Vector3(0, 0, segmentLength);
        
    }
    
    private void SpawnTurn(bool turnRight)
    {
        if (cornerPrefab == null)
        {
            SpawnStraightSegment();
            return;
        }
        Vector3 cornerPosition;
        Quaternion cornerDirectionRotation;
        Quaternion cornerPrefabRotation;
        Vector3 currentEndPos = nextSpawnPosition;
        
        if (turnRight)
        {
            cornerPosition = currentEndPos + nextSpawnRotation * rightCornerOffset;
            cornerDirectionRotation = nextSpawnRotation;
            cornerPrefabRotation = cornerDirectionRotation * Quaternion.Euler(0, prefabRotationYOffset, 0);
            
        }
        else
        {
            cornerPosition = currentEndPos + nextSpawnRotation * leftCornerOffset;
            cornerDirectionRotation = nextSpawnRotation * Quaternion.Euler(0, 90, 0);
            cornerPrefabRotation = cornerDirectionRotation * Quaternion.Euler(0, prefabRotationYOffset, 0);
            
        }
        GameObject corner = Instantiate(cornerPrefab, cornerPosition, cornerPrefabRotation);
        corner.name = $"Road_Corner_{(turnRight ? "R" : "L")}_{totalSegmentsGenerated}";
        corner.transform.localScale = new Vector3(30f, 1f, 30f);
        
        SetupSegment(corner);
        activeSegments.Add(corner);
        totalSegmentsGenerated++;
        Quaternion oldRotation = nextSpawnRotation;
        
        if (turnRight)
        {
            nextSpawnRotation = oldRotation * Quaternion.Euler(0, 90, 0);
            nextSpawnPosition = currentEndPos + oldRotation * afterRightStraightOffset;
        }
        else
        {
            nextSpawnRotation = oldRotation * Quaternion.Euler(0, -90, 0);
            nextSpawnPosition = cornerPosition + nextSpawnRotation * afterLeftStraightOffset;
        }
        straightSegmentCount = 0;
        
    }
    
    private void SetupSegment(GameObject segment)
    {
        MeshFilter meshFilter = segment.GetComponent<MeshFilter>();
        MeshCollider meshCollider = segment.GetComponent<MeshCollider>();
        
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            if (meshCollider == null)
            {
                meshCollider = segment.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = false;
        }
        else
        {
            BoxCollider boxCollider = segment.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = segment.AddComponent<BoxCollider>();
            }
            boxCollider.size = new Vector3(1f, 0.1f, 1f);
            boxCollider.center = new Vector3(0f, 0f, 0f);
            boxCollider.isTrigger = false;
        }
        CreateRoadBarriers(segment);
        SpawnObstacles(segment);
        SpawnCoins(segment);
        CreatePassTrigger(segment);
    }
    
    private void SpawnObstacles(GameObject segment)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        if (Random.value > obstacleSpawnChance) return;
        bool isCorner = segment.name.Contains("Corner");
        if (isCorner) return;
        GameObject obstaclesContainer = new GameObject("Obstacles");
        obstaclesContainer.transform.SetParent(segment.transform);
        obstaclesContainer.transform.localPosition = Vector3.zero;
        obstaclesContainer.transform.localRotation = Quaternion.identity;
        Vector3 parentScale = segment.transform.localScale; // (30, 1, 30)
        obstaclesContainer.transform.localScale = new Vector3(
            1f / parentScale.x,  // 1/30
            1f / parentScale.y,  // 1/1
            1f / parentScale.z   // 1/30
        );
        int obstacleCount = Random.Range(minObstaclesPerSegment, maxObstaclesPerSegment + 1);
        float roadMinX = -0.95f * parentScale.x;  // -28.5
        float roadMaxX = -0.05f * parentScale.x;  // -1.5
        
        float roadMinZ = 0.02f * parentScale.z;   // 0.6
        float roadMaxZ = 0.28f * parentScale.z;   // 8.4
        
        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            Vector3 originalScale = prefab.transform.localScale;
            Quaternion originalRotation = prefab.transform.localRotation;
            float posX = Random.Range(roadMinX, roadMaxX);
            float posZ = Random.Range(roadMinZ, roadMaxZ);
            Vector3 localPos = new Vector3(posX, 0f, posZ);
            GameObject obstacle = Instantiate(prefab, obstaclesContainer.transform);
            obstacle.transform.localPosition = localPos;
            obstacle.transform.localRotation = originalRotation;
            obstacle.transform.localScale = originalScale; // scale original din prefab
        }
    }

    
    private void CreateRoadBarriers(GameObject segment)
    {
        Transform existingBarrier = segment.transform.Find("LeftBarrier");
        if (existingBarrier != null) return;
        bool isCorner = segment.name.Contains("Corner");
        Vector3 leftLocalPos;
        Vector3 rightLocalPos;
        Quaternion leftBarrierRotation;
        Quaternion rightBarrierRotation;
        
        if (isCorner)
        {
            leftLocalPos = new Vector3(-0.55f, 2.5f, 0f);
            rightLocalPos = new Vector3(-0.33f, 2.5f, 0f);
            leftBarrierRotation = Quaternion.Euler(0, -90, 0);  // -90 pentru left
            rightBarrierRotation = Quaternion.identity;          // 0 pentru right
        }
        else
        {
            
            float segmentYRotation = segment.transform.localRotation.eulerAngles.y;
            
            if (Mathf.Abs(segmentYRotation - 180f) < 10f)
            {
                leftLocalPos = new Vector3(-0.55f, 2.5f, 0f);
                rightLocalPos = new Vector3(-0.55f, 2.5f, 0.31f);  // +0.3
                leftBarrierRotation = Quaternion.Euler(0, 90, 0);
                rightBarrierRotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                leftLocalPos = new Vector3(-0.55f, 2.5f, 0f);
                rightLocalPos = new Vector3(-0.55f, 2.5f, 0.31f);
                leftBarrierRotation = Quaternion.Euler(0, 90, 0);
                rightBarrierRotation = Quaternion.Euler(0, 90, 0);
            }
        }
        Vector3 parentScale = segment.transform.localScale; // (30, 1, 30)
        Vector3 colliderSize = new Vector3(0.01f, 5f, 32.3f);
        Vector3 colliderCenter = Vector3.zero;
        GameObject leftBarrier = new GameObject("LeftBarrier");
        leftBarrier.transform.SetParent(segment.transform);
        leftBarrier.transform.localPosition = leftLocalPos;
        leftBarrier.transform.localRotation = leftBarrierRotation;
        leftBarrier.transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
        
        BoxCollider leftCollider = leftBarrier.AddComponent<BoxCollider>();
        leftCollider.isTrigger = false;
        leftCollider.size = colliderSize;
        leftCollider.center = colliderCenter;
        GameObject rightBarrier = new GameObject("RightBarrier");
        rightBarrier.transform.SetParent(segment.transform);
        rightBarrier.transform.localPosition = rightLocalPos;
        rightBarrier.transform.localRotation = rightBarrierRotation;
        rightBarrier.transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
        
        BoxCollider rightCollider = rightBarrier.AddComponent<BoxCollider>();
        rightCollider.isTrigger = false;
        rightCollider.size = colliderSize;
        rightCollider.center = colliderCenter;
    }
    
    private void CreatePassTrigger(GameObject segment)
    {
        Transform existingTrigger = segment.transform.Find("PassTrigger");
        if (existingTrigger != null) return;
        
        GameObject triggerObj = new GameObject("PassTrigger");
        triggerObj.transform.SetParent(segment.transform);
        triggerObj.transform.localPosition = new Vector3(0, 1f, 0.4f);
        triggerObj.transform.localRotation = Quaternion.identity;
        triggerObj.transform.localScale = Vector3.one;
        
        BoxCollider triggerCollider = triggerObj.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(0.9f, 3f, 0.15f);
        triggerCollider.center = Vector3.zero;
        
        SegmentPassTrigger passTrigger = triggerObj.AddComponent<SegmentPassTrigger>();
        passTrigger.parentSegment = segment;
        passTrigger.generator = this;
    }
    
    public void OnSegmentPassed(GameObject segment)
    {
        if (activeSegments.Count < segmentsAhead + 2)
        {
            SpawnNextSegment();
        }
    }
    
    private void SpawnCoins(GameObject segment)
    {
        if (coinPrefab == null) return;
        if (Random.value > coinSpawnChance) return;
        bool isCorner = segment.name.Contains("Corner");
        if (isCorner) return;
        GameObject coinsContainer = new GameObject("Coins");
        coinsContainer.transform.SetParent(segment.transform);
        coinsContainer.transform.localPosition = Vector3.zero;
        coinsContainer.transform.localRotation = Quaternion.identity;
        Vector3 parentScale = segment.transform.localScale;
        coinsContainer.transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );
        int coinCount = Random.Range(minCoinsPerSegment, maxCoinsPerSegment + 1);
        float roadMinX = -0.95f * parentScale.x;
        float roadMaxX = -0.05f * parentScale.x;
        float roadMinZ = 0.05f * parentScale.z;
        float roadMaxZ = 0.25f * parentScale.z;
        float roadRandomZ = Random.Range(roadMinZ, roadMaxZ);
        float spacing = (roadMaxX - roadMinX) / (coinCount + 1);
        
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 localPos = new Vector3(
                roadMinX + spacing * (i + 1),
                0.5f,
                roadRandomZ
            );
            
            GameObject coin = Instantiate(coinPrefab, coinsContainer.transform);
            coin.transform.localPosition = localPos;
            coin.transform.localRotation = coinPrefab.transform.localRotation;
            coin.transform.localScale = coinPrefab.transform.localScale;
        }
        
    }
    
    private void UpdatePowerUpTimer()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return;
        }
        powerUpTimer += Time.deltaTime;
        if (Mathf.FloorToInt(powerUpTimer) % 5 == 0 && powerUpTimer - Time.deltaTime < Mathf.Floor(powerUpTimer))
        {
        }
        if (powerUpTimer >= powerUpSpawnInterval)
        {
            SpawnPowerUp();
            powerUpTimer = 0f; // Reset timer
        }
    }
    
    private void SpawnPowerUp()
    {
        List<GameObject> recentStraightSegments = new List<GameObject>();
        for (int i = activeSegments.Count - 1; i >= 0 && recentStraightSegments.Count < 3; i--)
        {
            GameObject seg = activeSegments[i];
            if (seg != null && !seg.name.Contains("Corner"))
            {
                recentStraightSegments.Add(seg);
            }
        }
        
        
        if (recentStraightSegments.Count == 0)
        {
            return;
        }
        GameObject targetSegment = recentStraightSegments[Random.Range(0, recentStraightSegments.Count)];
        GameObject powerUpPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        GameObject powerUpContainer = new GameObject("PowerUp");
        powerUpContainer.transform.SetParent(targetSegment.transform);
        powerUpContainer.transform.localPosition = Vector3.zero;
        powerUpContainer.transform.localRotation = Quaternion.identity;
        Vector3 parentScale = targetSegment.transform.localScale;
        powerUpContainer.transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );
        float posX = Random.Range(-0.5f, -0.2f) * parentScale.x;
        float posZ = 0.15f * parentScale.z; // Centru segment
        
        Vector3 localPos = new Vector3(posX, 1f, posZ);
        
        GameObject powerUp = Instantiate(powerUpPrefab, powerUpContainer.transform);
        powerUp.transform.localPosition = localPos;
        powerUp.transform.localRotation = powerUpPrefab.transform.localRotation;
        powerUp.transform.localScale = powerUpPrefab.transform.localScale;
        
    }
}

public class SegmentPassTrigger : MonoBehaviour
{
    [HideInInspector] public GameObject parentSegment;
    [HideInInspector] public InfiniteRoadGenerator generator;
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            
            if (generator != null)
            {
                generator.OnSegmentPassed(parentSegment);
            }
            
        }
    }
}
