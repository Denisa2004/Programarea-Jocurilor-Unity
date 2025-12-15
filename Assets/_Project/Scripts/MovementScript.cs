using UnityEngine;

public class MovementScript : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 velocity = new Vector3(0f, 0f, 2f); 

    public void SetForwardSpeed(float newForwardSpeed)
    {
        velocity = new Vector3(velocity.x, velocity.y, newForwardSpeed);
    }

    public float GetForwardSpeed()
    {
        return velocity.z;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}