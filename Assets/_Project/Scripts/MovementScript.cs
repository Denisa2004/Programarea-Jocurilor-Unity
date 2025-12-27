using UnityEngine;

public class MovementScript : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 velocity = new Vector3(0f, 0f, 2f);

    [Header("Boundaries")]
    public Transform laneTransform;

    [Tooltip("Limita stanga/dreapta in coordonate locale ale lui laneTransform.")]
    public float minLocalX = -2f;
    public float maxLocalX =  2f;

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
        Vector3 newPosition = transform.position + velocity * Time.deltaTime;

        if (laneTransform != null)
        {
            Vector3 localPos = laneTransform.InverseTransformPoint(newPosition);

            localPos.x = Mathf.Clamp(localPos.x, minLocalX, maxLocalX);

            newPosition = laneTransform.TransformPoint(localPos);
        }

        transform.position = newPosition;
    }
}