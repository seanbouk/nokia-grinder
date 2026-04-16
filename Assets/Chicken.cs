using UnityEngine;

public class Chicken : MonoBehaviour
{
    //--- State Definitions ---
    private enum ChickenState { Waiting, Attacking, Resting }
    private ChickenState currentState = ChickenState.Waiting;
    private bool hasAttackedOnce = false;

    //--- Bouncing Settings (Vertical) ---
    [Header("Vertical Bouncing Settings")]
    [Tooltip("Controls how quickly the object bounces vertically.")]
    public float bounceSpeed = 2f;
    [Tooltip("Controls how high the object bounces.")]
    public float bounceHeight = 0.5f;

    //--- Movement & Player Collision Settings ---
    [Header("Player Movement & Collision Settings")]
    [Tooltip("Distance from the player (camera) within which the chicken begins attacking.")]
    public float attractionDistance = 5f;
    [Tooltip("Speed at which the chicken moves toward the player.")]
    public float moveSpeed = 2f;
    [Tooltip("Distance at which the chicken is considered to have collided with the player.")]
    public float collisionDistance = 1f;
    [Tooltip("Extra speed imparted when bouncing off the player.")]
    public float bounceInertia = 3f;
    [Tooltip("Rate at which the bounce inertia decays.")]
    public float inertiaDecayRate = 2f;
    [Tooltip("Cooldown (in seconds) to prevent repeated bounce triggers with the player.")]
    public float bounceCooldown = 0.5f;

    //--- Wall Collision Settings ---
    [Header("Wall Collision Settings")]
    [Tooltip("Layer mask that defines which layers are considered walls.")]
    public LayerMask wallLayer;
    [Tooltip("How far ahead to check for walls (in units).")]
    public float wallCheckDistance = 0.5f;

    //--- Internal Data ---
    private Vector3 initialPosition;
    private Vector3 extraVelocity = Vector3.zero;
    private float bounceCooldownTimer = 0f;

    void Start()
    {
        initialPosition = transform.position;
        currentState = ChickenState.Waiting;
    }

    void Update()
    {
        // Track first attack
        if (currentState == ChickenState.Attacking && !hasAttackedOnce)
        {
            hasAttackedOnce = true;
        }

        // Always compute vertical bounce.
        float verticalOffset = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * bounceHeight;

        // Decrease the bounce cooldown timer.
        if (bounceCooldownTimer > 0f)
            bounceCooldownTimer -= Time.deltaTime;

        // If no camera, only apply vertical bounce.
        if (Camera.main == null)
        {
            Vector3 pos = transform.position;
            pos.y = initialPosition.y + verticalOffset;
            transform.position = pos;
            return;
        }

        // Get camera position (only horizontal).
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 cameraHorizontal = new Vector3(cameraPos.x, transform.position.y, cameraPos.z);
        float distanceToCamera = Vector3.Distance(transform.position, cameraHorizontal);

        // --- State Transitions ---
        switch (currentState)
        {
            case ChickenState.Waiting:
                // In WAITING, if the player is close enough, start ATTACKING.
                if (distanceToCamera < attractionDistance)
                {
                    currentState = ChickenState.Attacking;
                }
                break;

            case ChickenState.Attacking:
                // In ATTACKING, if the player moves out of range, go back to WAITING.
                if (distanceToCamera >= attractionDistance)
                {
                    currentState = ChickenState.Waiting;
                }
                break;

            case ChickenState.Resting:
                // In RESTING, check for a direct line of sight to the player.
                bool hasLineOfSight = true;
                Vector3 directionToPlayer = (cameraHorizontal - transform.position).normalized;
                RaycastHit sightHit;
                if (Physics.Raycast(transform.position, directionToPlayer, out sightHit, distanceToCamera, wallLayer))
                {
                    hasLineOfSight = false;
                }
                if (hasLineOfSight)
                {
                    // Once the player is visible again, choose state based on distance.
                    if (distanceToCamera < attractionDistance)
                    {
                        currentState = ChickenState.Attacking;
                    }
                    else
                    {
                        currentState = ChickenState.Waiting;
                    }
                }
                break;
        }

        // --- Horizontal Movement ---
        Vector3 horizontalMovement = Vector3.zero;
        if (currentState == ChickenState.Attacking)
        {
            // If within collision distance, bounce off the player and deal damage.
            if (distanceToCamera < collisionDistance && bounceCooldownTimer <= 0f)
            {
                extraVelocity = (transform.position - cameraHorizontal).normalized * bounceInertia;
                bounceCooldownTimer = bounceCooldown;
                GameManager.Instance?.OnPlayerHit();
            }

            // Compute desired movement toward the player.
            Vector3 desiredVelocity = (cameraHorizontal - transform.position).normalized * moveSpeed;
            // Decay extra velocity over time.
            extraVelocity = Vector3.Lerp(extraVelocity, Vector3.zero, Time.deltaTime * inertiaDecayRate);
            // Combine desired velocity with any extra (bounce) velocity.
            horizontalMovement = (desiredVelocity + extraVelocity) * Time.deltaTime;

            // Check for wall collision along the intended movement.
            RaycastHit wallHit;
            if (horizontalMovement.sqrMagnitude > 0.0001f &&
                Physics.Raycast(transform.position, horizontalMovement.normalized, out wallHit, wallCheckDistance + horizontalMovement.magnitude, wallLayer))
            {
                currentState = ChickenState.Resting;
                horizontalMovement = Vector3.zero;
            }
        }
        // In WAITING and RESTING, no horizontal movement is applied.

        // --- Apply Final Movement ---
        Vector3 newPosition = transform.position + horizontalMovement;
        newPosition.y = initialPosition.y + verticalOffset;
        transform.position = newPosition;
    }

    // Returns true if this chicken has entered Attacking state at least once
    public bool HasAttacked()
    {
        return hasAttackedOnce;
    }
}
