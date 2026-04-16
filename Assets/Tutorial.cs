using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    private const float QUICK_TWEEN_TIME = 0.3f;
    private const float SLOW_TWEEN_TIME = 2.0f;
    
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI forwards;
    [SerializeField] private TextMeshProUGUI shoot;
    [SerializeField] private TextMeshProUGUI grind;
    [SerializeField] private TextMeshProUGUI gameOver;

    [Header("References")]
    [SerializeField] private GameObject grinderHenge;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask defaultLayer;
    
    private TutorialState currentState = TutorialState.Forwards;
    private enum TutorialState { Forwards, Shoot, Waiting, Grind, Play, GameOver }
    
    // Debug visualization
    private bool isGrinderVisible = false;

    private void Start()
    {
        // Set all text alphas to 0
        SetAlpha(forwards, 0f);
        SetAlpha(shoot, 0f);
        SetAlpha(grind, 0f);
        SetAlpha(gameOver, 0f);
        SetBackgroundAlpha(0f);

        // Start with forwards text
        StartCoroutine(FadeText(forwards, 1f, QUICK_TWEEN_TIME));

        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.onScoreChanged += OnScoreChanged;
            gameManager.onGameOver += OnGameOver;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.onScoreChanged -= OnScoreChanged;
            gameManager.onGameOver -= OnGameOver;
        }
    }

    private void Update()
    {
        CheckStateTransitions();
    }

    private void CheckStateTransitions()
    {
        switch (currentState)
        {
            case TutorialState.Forwards:
                if (Input.GetKeyDown(KeyCode.W))
                    TransitionToShoot();
                break;
            case TutorialState.Shoot:
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    TransitionToWaiting();
                break;
            case TutorialState.Waiting:
                CheckGrinderVisibility();
                if (isGrinderVisible && gameManager != null && gameManager.RoastsCollected > 0)
                    TransitionToGrind();
                break;
            case TutorialState.Grind:
                // Handled by OnScoreChanged
                break;
            case TutorialState.Play:
                // Handled by OnGameOver
                break;
            case TutorialState.GameOver:
                break;
        }
    }

    private void CheckGrinderVisibility()
    {
        if (Camera.main == null || grinderHenge == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit wallHit, grinderHit;

        // Check both wall and grinder hits
        bool hitWall = Physics.Raycast(ray, out wallHit, Mathf.Infinity, wallLayer);
        bool hitGrinder = Physics.Raycast(ray, out grinderHit, Mathf.Infinity, defaultLayer) && 
                         grinderHit.collider.gameObject == grinderHenge;

        // The grinder is visible if we hit it AND either:
        // 1. We didn't hit a wall, or
        // 2. The grinder is closer than the wall
        isGrinderVisible = hitGrinder && (!hitWall || grinderHit.distance < wallHit.distance);
    }

    private void TransitionToShoot()
    {
        currentState = TutorialState.Shoot;
        StartCoroutine(FadeText(forwards, 0f, QUICK_TWEEN_TIME));
        StartCoroutine(FadeText(shoot, 1f, QUICK_TWEEN_TIME));
    }

    private void TransitionToWaiting()
    {
        currentState = TutorialState.Waiting;
        StartCoroutine(FadeText(shoot, 0f, QUICK_TWEEN_TIME));
    }

    private void TransitionToGrind()
    {
        currentState = TutorialState.Grind;
        StartCoroutine(FadeText(grind, 1f, QUICK_TWEEN_TIME));
    }

    private void TransitionToPlay()
    {
        currentState = TutorialState.Play;
        StartCoroutine(FadeText(grind, 0f, QUICK_TWEEN_TIME));
    }

    private void TransitionToGameOver()
    {
        currentState = TutorialState.GameOver;
        
        // Set all tutorial text alphas to 0 immediately
        SetAlpha(forwards, 0f);
        SetAlpha(shoot, 0f);
        SetAlpha(grind, 0f);

        // Set game over text with score
        if (gameOver != null && gameManager != null)
        {
            gameOver.text = $"GAME\nOVER\n{gameManager.GroundRoasts}";
        }

        // Fade in game over text and background
        StartCoroutine(FadeText(gameOver, 1f, QUICK_TWEEN_TIME));
        StartCoroutine(FadeImage(background, 1f, SLOW_TWEEN_TIME));
    }

    private void OnScoreChanged()
    {
        if (currentState == TutorialState.Grind && gameManager != null && gameManager.GroundRoasts > 0)
        {
            TransitionToPlay();
        }
    }

    private void OnGameOver()
    {
        if (currentState != TutorialState.GameOver)
        {
            TransitionToGameOver();
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.onScoreChanged += OnScoreChanged;
            gameManager.onGameOver += OnGameOver;
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        if (text == null) yield break;

        float startAlpha = text.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            SetAlpha(text, currentAlpha);
            yield return null;
        }

        SetAlpha(text, targetAlpha);
    }

    private IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        if (image == null) yield break;

        float startAlpha = image.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            SetImageAlpha(image, currentAlpha);
            yield return null;
        }

        SetImageAlpha(image, targetAlpha);
    }

    private void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private void SetBackgroundAlpha(float alpha)
    {
        SetImageAlpha(background, alpha);
    }

    private void OnDrawGizmos()
    {
        if (currentState == TutorialState.Waiting && Camera.main != null)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            
            // Draw red for wall hit, green for visible grinder, yellow for invisible grinder
            RaycastHit wallHit, grinderHit;
            bool hitWall = Physics.Raycast(ray, out wallHit, Mathf.Infinity, wallLayer);
            bool hitGrinder = Physics.Raycast(ray, out grinderHit, Mathf.Infinity, defaultLayer) && 
                             grinderHit.collider.gameObject == grinderHenge;

            if (hitGrinder && (!hitWall || grinderHit.distance < wallHit.distance))
            {
                // Grinder is visible - draw green ray to grinder
                Gizmos.color = Color.green;
                Gizmos.DrawRay(ray.origin, ray.direction * grinderHit.distance);
            }
            else if (hitWall)
            {
                // Hit wall first - draw red ray to wall
                Gizmos.color = Color.red;
                Gizmos.DrawRay(ray.origin, ray.direction * wallHit.distance);
            }
            else
            {
                // No hits - draw yellow ray
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(ray.origin, ray.direction * 100f);
            }
        }
    }
}
