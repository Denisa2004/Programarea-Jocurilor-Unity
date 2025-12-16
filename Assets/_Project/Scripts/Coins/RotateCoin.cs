using UnityEngine;

public class RotateCoin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    void Update()
    {
        // rotate coins around the y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
