using UnityEngine;
using System.Collections;

public class Roast : MonoBehaviour
{
    public enum RoastState
    {
        ATTRACT,
        REPEL,
        GRIND
    }

    [Header("Grind Settings")]
    [Tooltip("Acceleration applied downward when grinding")]
    public float grindAcceleration = 20f;
    
    [Tooltip("Time in seconds before the roast is destroyed when grinding")]
    public float grindTime = 0.5f;

    [Header("Movement Settings")]
    [Tooltip("Initial movement speed towards player")]
    public float initialSpeed = 2f;
    
    [Tooltip("Speed increases over time by this amount")]
    public float acceleration = 1f;
    
    [Tooltip("Distance from camera at which the roast destroys itself")]
    public float destroyDistance = 1f;

    [Tooltip("Speed when shot as a projectile")]
    public float shootSpeed = 5f;

    private Transform mainCamera;
    private float currentSpeed;
    private RoastState state = RoastState.ATTRACT;
    private Vector3 moveDirection;
    private float initialY;
    private Rigidbody rb;

    private void Start()
    {
        // Get reference to main camera transform
        mainCamera = Camera.main.transform;
        currentSpeed = initialSpeed;
        initialY = transform.position.y;

        // If in REPEL state, set initial direction away from player
        if (state == RoastState.REPEL)
        {
            moveDirection = transform.forward;
        }

        // Get or add Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Vector3 newPosition = transform.position;
        Vector3 directionToPlayer = mainCamera.position - transform.position;
        directionToPlayer.y = 0f; // Keep movement in XZ plane
        
        // Handle movement and collection based on state
        if (state == RoastState.GRIND)
        {
            rb.isKinematic = false;
            // Continue horizontal movement
            newPosition += moveDirection * shootSpeed * Time.deltaTime;
            transform.position = newPosition;
            // Add downward acceleration
            rb.AddForce(Vector3.down * grindAcceleration, ForceMode.Acceleration);
            return;
        }
        else if (state == RoastState.ATTRACT)
        {
            float distance = directionToPlayer.magnitude;
            
            // Only check for collection in ATTRACT state
            if (distance <= destroyDistance)
            {
                GameManager.Instance?.OnRoastCollected();
                Destroy(gameObject);
                return;
            }

            // Move towards player
            newPosition += directionToPlayer.normalized * currentSpeed * Time.deltaTime;
            
            // Increase speed over time
            currentSpeed += acceleration * Time.deltaTime;
        }
        else // REPEL state
        {
            // Move in the set direction
            newPosition += moveDirection * shootSpeed * Time.deltaTime;
        }

        // Keep Y position constant
        newPosition.y = initialY;
        transform.position = newPosition;
    }

    public void SetState(RoastState newState)
    {
        state = newState;
        if (state == RoastState.REPEL)
        {
            currentSpeed = shootSpeed;
        }
        else
        {
            currentSpeed = initialSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for grinder henge collision in REPEL state
        if (state == RoastState.REPEL && other.gameObject.name == "grinder henge")
        {
            StartCoroutine(GrindSequence());
            return;
        }
        
        // Check for wall collisions in REPEL state
        if (state == RoastState.REPEL && other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            SetState(RoastState.ATTRACT);
        }
    }

    private IEnumerator GrindSequence()
    {
        state = RoastState.GRIND;
        yield return new WaitForSeconds(grindTime);
        GameManager.Instance?.OnRoastGround();
        Destroy(gameObject);
    }
}
