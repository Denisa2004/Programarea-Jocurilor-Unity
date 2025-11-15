using UnityEngine;

public class TerrainSegment : MonoBehaviour
{
    public SegmentType segmentType;

    public Vector3 GetExitPosition()
    {
        Transform exitPoint = transform.Find("ExitPoint");
        if (exitPoint != null)
            return exitPoint.position;
        return transform.position + transform.forward * 15f;
    }

    public Vector3 GetStartPosition()
    {
        Transform startPoint = transform.Find("StartPoint");
        if (startPoint != null)
            return startPoint.position;
        //la inceputul segmentului
        return transform.position - transform.forward * 15f;
    }

    public Quaternion GetExitRotation()
    {
        Transform exitPoint = transform.Find("ExitPoint");
        if (exitPoint != null)
            return exitPoint.rotation;
        return transform.rotation;
    }
}

