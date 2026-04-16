using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which the fireball moves away from the player (in units/second).")]
    public float speed = 5f;
    [Tooltip("Maximum allowed distance from the player before the fireball destroys itself.")]
    public float maxDistance = 20f;

    [Header("Effect Settings")]
    [Tooltip("Prefab to spawn when a chicken is destroyed")]
    public GameObject roastPrefab;

    // Movement direction (in X,Z plane).
    private Vector3 moveDirection;
    // Y coordinate remains constant.
    private float initialY;

    void Awake()
    {
        // Ensure the fireball has a Rigidbody so that trigger events fire.
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        // Save the initial Y so that the fireball remains at this height.
        initialY = transform.position.y;

        // Use the object's forward direction for movement
        moveDirection = transform.forward;
    }

    void Update()
    {
        // Move the fireball in the X,Z plane.
        Vector3 displacement = moveDirection * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + displacement;
        newPosition.y = initialY;  // Ensure Y remains constant.
        transform.position = newPosition;

        // Check the distance from the player; if too far, destroy the fireball.
        if (Camera.main != null)
        {
            Vector3 playerPos = Camera.main.transform.position;
            // Ignore vertical differences.
            playerPos.y = transform.position.y;
            float distanceFromPlayer = Vector3.Distance(transform.position, playerPos);
            if (distanceFromPlayer > maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    // Called when the fireball (with a trigger collider) enters another collider
    private void OnTriggerEnter(Collider other)
    {
        // Check for wall collision
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (SoundEngine.Instance != null)
            {
                SoundEngine.Instance.PlaySound(SoundType.WallHit);
            }
            Destroy(gameObject);
            return;
        }

        // Check for chicken collision
        Chicken chicken = other.GetComponent<Chicken>();
        if (chicken != null)
        {
            // Notify GameManager and spawn roast
            if (roastPrefab != null)
            {
                Vector3 roastPosition = chicken.transform.position;
                Instantiate(roastPrefab, roastPosition, Quaternion.identity);
            }
            
            GameManager.Instance?.OnChickenDestroyed();
            Destroy(gameObject);
            Destroy(chicken.gameObject);
        }
    }
}
