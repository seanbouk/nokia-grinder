using UnityEngine;

/// <summary>
/// Rotates an object continuously around its Y axis at a configurable speed.
/// </summary>
public class ConstantRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation in degrees per second")]
    public float rotationSpeed = 90f;  // Default to 90 degrees per second (1/4 turn)

    void Update()
    {
        // Rotate around the world Y axis
        transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
