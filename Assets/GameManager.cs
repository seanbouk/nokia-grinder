using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour 
{
    [Header("UI References")]
    [Tooltip("Text displaying number of roasts collected")]
    public TextMeshProUGUI chickensText;
    [Tooltip("Text displaying score")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Text displaying player health")]
    public TextMeshProUGUI healthText;

    [Header("Visual Effects")]
    [Tooltip("Material used for the Nokia 1-bit effect")]
    public Material nokia1BitMaterial;
    [Tooltip("Duration of the flash effect when player is hit")]
    public float flashDuration = 0.5f;
    [Tooltip("How quickly the flash effect toggles (times per second)")]
    public float flashFrequency = 10f;

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    private int totalChickens;
    private int currentRoasts;
    private int roastsCollected;
    private int groundRoasts;
    private float playerHealth = 99f;
    private bool isGameOver = false;
    private float originalThreshold = 0.5f;  // Default threshold value

    // Public state
    public bool IsGameOver => isGameOver;

    // Events
    public event System.Action onScoreChanged;
    public event System.Action onGameOver;

    // Public accessors
    public int GroundRoasts => groundRoasts;
    public int RoastsCollected => roastsCollected;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        // Store the original threshold value
        if (nokia1BitMaterial != null)
        {
            originalThreshold = nokia1BitMaterial.GetFloat("_Threshold");
        }

        // Count all chickens in the scene at start
        totalChickens = FindObjectsOfType<Chicken>().Length;
        UpdateUI();
        LogGameState();
    }

    public void UpdateChickenCount(int newCount)
    {
        totalChickens = newCount;
        UpdateUI();
        LogGameState();
    }

    public void OnChickenDestroyed()
    {
        totalChickens--;
        currentRoasts++;
        if (SoundEngine.Instance != null)
        {
            SoundEngine.Instance.PlaySound(SoundType.Kill);
        }
        UpdateUI();
        LogGameState();
    }

    public bool CanShootRoast()
    {
        return roastsCollected > 0 && !isGameOver;
    }

    public void OnRoastShot()
    {
        roastsCollected--;
        if (SoundEngine.Instance != null)
        {
            SoundEngine.Instance.PlaySound(SoundType.RoastShoot);
        }
        UpdateUI();
        LogGameState();
    }

    public void OnRoastCollected()
    {
        currentRoasts--;
        roastsCollected++;
        if (SoundEngine.Instance != null)
        {
            SoundEngine.Instance.PlaySound(SoundType.Collect);
        }
        UpdateUI();
        LogGameState();
    }

    public void OnRoastGround()
    {
        groundRoasts++;
        if (SoundEngine.Instance != null)
        {
            SoundEngine.Instance.PlaySound(SoundType.Grind);
        }
        UpdateUI();
        LogGameState();
        onScoreChanged?.Invoke();
    }

    public void OnPlayerHit()
    {
        if (isGameOver) return;
        
        playerHealth = Mathf.Clamp(playerHealth - 7f, 0f, 99f);
        if (SoundEngine.Instance != null)
        {
            SoundEngine.Instance.PlaySound(SoundType.Attacked);
        }
        UpdateUI();
        LogGameState();

        if (playerHealth <= 0)
        {
            isGameOver = true;
            Debug.Log("Game Over!");
            ResetThreshold(); // Ensure threshold is reset on game over
            if (SoundEngine.Instance != null)
            {
                SoundEngine.Instance.PlaySound(SoundType.GameEnd);
            }
            onGameOver?.Invoke();
        }
        else if (nokia1BitMaterial != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    private void ResetThreshold()
    {
        if (nokia1BitMaterial != null)
        {
            nokia1BitMaterial.SetFloat("_Threshold", originalThreshold);
        }
    }

    private IEnumerator FlashEffect()
    {
        float interval = 1f / flashFrequency;
        float endTime = Time.time + flashDuration;
        bool isFlashing = true;

        while (Time.time < endTime)
        {
            nokia1BitMaterial.SetFloat("_Threshold", isFlashing ? 1f : 0f);
            isFlashing = !isFlashing;
            yield return new WaitForSeconds(interval);
        }

        ResetThreshold();
    }

    private void UpdateUI()
    {
        if (chickensText != null) chickensText.text = roastsCollected.ToString();
        if (scoreText != null) scoreText.text = groundRoasts.ToString();
        if (healthText != null) healthText.text = Mathf.RoundToInt(playerHealth).ToString();
    }

    private void LogGameState()
    {
        //Debug.Log($"Game State: Health={playerHealth}, Chickens={totalChickens}, Active Roasts={currentRoasts}, Collected Roasts={roastsCollected}, Ground Roasts={groundRoasts}");
    }
}
