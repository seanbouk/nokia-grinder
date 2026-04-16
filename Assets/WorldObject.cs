using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Tooltip("Extra rotation if the plane faces up instead of forward.")]
    public float extraXRotation = -90f;

    [Header("Animation Settings")]
    [Tooltip("Textures that will cycle through.")]
    public Texture2D[] fireTextures;
    [Tooltip("Duration for each frame in seconds.")]
    public float frameDuration = 0.1f;

    [Header("Twist Angle Settings")]
    [Tooltip("Minimum twist angle for the texture rotation (in degrees).")]
    public float minAngle = 0f;
    [Tooltip("Maximum twist angle for the texture rotation (in degrees).")]
    public float maxAngle = 360f;

    private float timer;
    private int currentFrame;
    private Material fireballMaterial;
    // Current random twist angle for the texture rotation (in degrees)
    private float twistAngle = 0f;

    void Start()
    {
        // Get the material instance from the Renderer.
        fireballMaterial = GetComponent<Renderer>().material;
        if (fireTextures == null || fireTextures.Length == 0)
        {
            Debug.LogWarning("No fireTextures assigned in Fireball script on " + gameObject.name);
        }
        // Initialize with a random twist within the specified range.
        twistAngle = Random.Range(minAngle, maxAngle);
        fireballMaterial.SetFloat("_Rotation", twistAngle);
    }

    void Update()
    {
        // ----------------------------
        // 1. Billboard Rotation Logic:
        // ----------------------------
        if (Camera.main != null)
        {
            Vector3 direction = Camera.main.transform.position - transform.position;
            // Optionally, restrict rotation to horizontal (remove vertical tilting)
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                // Set rotation so that the plane's +Z faces the camera.
                transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
                // Adjust for the plane's default orientation.
                transform.Rotate(extraXRotation, 0f, 0f);
            }
        }

        // ----------------------------
        // 2. Texture Animation & Random UV Twist:
        // ----------------------------
        if (fireTextures != null && fireTextures.Length > 0)
        {
            timer += Time.deltaTime;
            if (timer >= frameDuration)
            {
                timer -= frameDuration;
                // Cycle to the next texture.
                currentFrame = (currentFrame + 1) % fireTextures.Length;
                fireballMaterial.mainTexture = fireTextures[currentFrame];

                // Choose a new random twist angle within the specified range.
                twistAngle = Random.Range(minAngle, maxAngle);
                fireballMaterial.SetFloat("_Rotation", twistAngle);
            }
        }
    }
}
