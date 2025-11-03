using UnityEngine;

public class MovePingPong : MonoBehaviour
{
    public Vector3 moveAxis = Vector3.right;
    public float distance = 3f;
    public float speed = 1.5f;

    private Vector3 startPos;

    void Start() { startPos = transform.position; }

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * speed, distance * 2f) - distance;
        transform.position = startPos + moveAxis.normalized * offset;
    }
}
