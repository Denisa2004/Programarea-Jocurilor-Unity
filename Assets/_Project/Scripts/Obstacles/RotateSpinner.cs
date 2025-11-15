using UnityEngine;

public class RotateSpinner : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 90f;         

    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime, Space.World);
    }
}
