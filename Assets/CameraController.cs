using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;

/// <summary>
/// Handles player movement, rotation, wall collision, and palette switching for the Nokia 3310 style renderer.
/// Requires:
/// - A Rigidbody component with "Use Gravity" off and "Is Kinematic" on
/// - Wall objects with MeshCollider components (Convex enabled)
/// </summary>
public class NokiaInputHandler : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void UpdateWebColors(string background, string foreground);
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 90.0f;

    [Header("Projectile Settings")]
    [Tooltip("Reference to the Fireball prefab")]
    public GameObject fireballPrefab;
    [Tooltip("Reference to the Roast prefab for shooting")]
    public GameObject roastPrefab;
    private Vector2 moveInput;
    private float rotationY;
    private float stepSoundTimer = 0.5f;  // Start at 0.5f so first step plays immediately
    private int paletteIndex = 0;
    private Color[][] palettes = new Color[][]
    {
        new Color[] { new Color(0.78f, 0.94f, 0.85f), new Color(0.26f, 0.32f, 0.24f) }, // c7f0d8 and 43523d
        new Color[] { new Color(0.61f, 0.78f, 0.00f), new Color(0.17f, 0.25f, 0.04f) }, // 9bc700 and 2b3f09
        new Color[] { new Color(0.53f, 0.57f, 0.53f), new Color(0.10f, 0.09f, 0.08f) }  // 879188 and 1a1914
    };
    private Material effectMaterial;

    [DllImport("__Internal")]
    private static extern void InitializeWebInput();
    
    [DllImport("__Internal")]
    private static extern bool IsWPressed();

    void Start()
    {
        rotationY = transform.eulerAngles.y;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        effectMaterial = FindObjectOfType<Nokia1BitPostProcess>()?.EffectMaterial;
        ApplyPalette();
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        InitializeWebInput();
        #endif
    }

    [Header("Wall Collision Settings")]
    [Tooltip("How far ahead to check for walls (in units)")]
    public float wallCheckDistance = 1.0f;
    
    [Tooltip("Minimum angle (dot product) at which sliding begins")]
    public float wallSlideThreshold = 0.1f;
    
    [Tooltip("Which layers to check for wall collisions")]
    public LayerMask wallLayer = -1;

    void Update()
    {
        // Handle palette switching (always enabled)
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            paletteIndex = (paletteIndex + 1) % palettes.Length;
            ApplyPalette();
        }

        // Don't process gameplay input if game is over
        if (gameManager != null && gameManager.IsGameOver)
            return;

        // Get movement input
        if (Keyboard.current != null)
        {
            float webWInput = 0;
            #if UNITY_WEBGL && !UNITY_EDITOR
            if (IsWPressed()) webWInput = 1;
            #endif

            moveInput = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed || webWInput > 0 ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
            );
        }

        // Handle step sounds
        if (moveInput.magnitude > 0)
        {
            stepSoundTimer += Time.deltaTime;
            if (stepSoundTimer >= 0.5f)
            {
                if (SoundEngine.Instance != null)
                {
                    SoundEngine.Instance.PlaySound(SoundType.Step);
                }
                stepSoundTimer = 0f;
            }
        }
        else
        {
            stepSoundTimer = 0.5f;  // Reset timer when not moving so next movement plays sound immediately
        }

        // Calculate desired movement direction
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (moveDirection.magnitude > 0)
        {
            moveDirection.Normalize();
            Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

            // Check for wall collision
            bool hitWall = Physics.Raycast(transform.position, moveDirection, out RaycastHit hit, wallCheckDistance, wallLayer);
            
            // Debug visualization
            Debug.DrawRay(transform.position, moveDirection * wallCheckDistance, hitWall ? Color.red : Color.green);
            
            if (hitWall)
            {
                // Calculate slide direction along the wall
                Vector3 wallNormal = hit.normal;
                Vector3 slideDirection = Vector3.ProjectOnPlane(moveDirection, wallNormal).normalized;
                
                float dotProduct = Vector3.Dot(moveDirection, wallNormal);
                
                // Only slide if we're not trying to move directly into the wall
                if (dotProduct < -wallSlideThreshold && slideDirection.magnitude > 0.1f)
                {
                    // Reduce speed when sliding along walls
                    float slideSpeed = moveSpeed * (1.0f - Mathf.Abs(dotProduct));
                    targetPosition = transform.position + slideDirection * slideSpeed * Time.deltaTime;
                    
                    // Debug visualization for slide direction
                    Debug.DrawRay(transform.position, slideDirection * wallCheckDistance, Color.yellow);
                }
                else
                {
                    targetPosition = transform.position;
                }
            }

            transform.position = targetPosition;
        }

        // Keep Y position constant
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);

        // Rotation
        if (Keyboard.current.qKey.isPressed)
        {
            rotationY -= rotateSpeed * Time.deltaTime;
        }
        if (Keyboard.current.eKey.isPressed)
        {
            rotationY += rotateSpeed * Time.deltaTime;
        }
        transform.rotation = Quaternion.Euler(0, rotationY, 0);

        // Projectile spawning
        if (Keyboard.current != null)
        {
            // Fireball spawning with 2 key
            if (Keyboard.current.digit2Key.wasPressedThisFrame && fireballPrefab != null)
            {
                // Calculate spawn position 1 unit in front of the camera
                Vector3 spawnPosition = transform.position + transform.forward;
                
                // Instantiate the fireball at the calculated position with the camera's rotation
                GameObject fireball = Instantiate(fireballPrefab, spawnPosition, transform.rotation);
                
                // Play fireball shoot sound
                if (SoundEngine.Instance != null)
                {
                    SoundEngine.Instance.PlaySound(SoundType.FireballShoot);
                }
            }

            // Roast shooting with 3 key
            if (Keyboard.current.digit3Key.wasPressedThisFrame && roastPrefab != null && GameManager.Instance != null)
            {
                ShootRoast();
            }
        }
    }

    private void ShootRoast()
    {
        if (GameManager.Instance.CanShootRoast())
        {
            // Calculate spawn position 1 unit in front of the camera
            Vector3 spawnPosition = transform.position + transform.forward;
            
            // Instantiate the roast at the camera position with the camera's rotation
            GameObject roast = Instantiate(roastPrefab, spawnPosition, transform.rotation);
            
            // Set the roast to REPEL state
            Roast roastComponent = roast.GetComponent<Roast>();
            if (roastComponent != null)
            {
                roastComponent.SetState(Roast.RoastState.REPEL);
            }

            // Notify GameManager that a roast was shot
            GameManager.Instance.OnRoastShot();
        }
    }

    void ApplyPalette()
    {
        if (effectMaterial != null)
        {
            Color foregroundColor = palettes[paletteIndex][0];  // Color1 - lighter color
            Color backgroundColor = palettes[paletteIndex][1];  // Color2 - darker color
            effectMaterial.SetColor("_Color1", foregroundColor);
            effectMaterial.SetColor("_Color2", backgroundColor);
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            string backgroundHex = ColorUtility.ToHtmlStringRGB(backgroundColor);
            string foregroundHex = ColorUtility.ToHtmlStringRGB(foregroundColor);
            UpdateWebColors('#' + backgroundHex, '#' + foregroundHex);
            #endif
        }
    }
}
