using UnityEngine;
using System.Collections.Generic;

public class Sanders : MonoBehaviour
{
    // Schedule of required chicken counts over time (seconds)
    private static readonly (float time, int chickens)[] schedule = new[]
    {
        (0f, 1),
        (15f, 3),
        (60f, 3),
        (120f, 10),
        (600f, 20)
    };

    [Header("References")]
    [Tooltip("GameObject containing all chickens as children")]
    public GameObject chickenContainer;
    [Tooltip("GameObject containing all eggs as children")]
    public GameObject eggContainer;
    [Tooltip("Prefab to use when spawning new chickens")]
    public GameObject chickenPrefab;
    [Tooltip("Layer mask for wall detection (should match Chicken's wallLayer)")]
    public LayerMask wallLayer;

    // Tracks which chickens came from which eggs and haven't attacked yet
    private Dictionary<GameObject, GameObject> chickenEggPairs = new Dictionary<GameObject, GameObject>();
    private float nextCheckTime;
    private const float CHECK_INTERVAL = 2f;

    private void Start()
    {
        nextCheckTime = Time.time + CHECK_INTERVAL;
    }

    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + CHECK_INTERVAL;
            
            int currentChickenCount = chickenContainer.transform.childCount;
            int requiredChickenCount = GetRequiredChickenCount();

            // Log current status
            string spawnMessage = "";
            
            if (currentChickenCount < requiredChickenCount)
            {
                GameObject bestEgg = FindBestEggForSpawning();
                if (bestEgg != null)
                {
                    GameObject newChicken = SpawnChickenAtEgg(bestEgg);
                    spawnMessage = $", spawned chicken at egg: {bestEgg.name}";
                }
            }

            // Update GameManager with current count
            GameManager.Instance?.UpdateChickenCount(currentChickenCount);
            
            //Debug.Log($"Sanders check - Current chickens: {currentChickenCount}, Required chickens: {requiredChickenCount}{spawnMessage}");

            // Clean up any destroyed chickens from our tracking
            List<GameObject> destroyedChickens = new List<GameObject>();
            foreach (var pair in chickenEggPairs)
            {
                if (pair.Key == null)
                {
                    destroyedChickens.Add(pair.Key);
                    continue;
                }

                // Check if chicken has entered attacking mode
                Chicken chicken = pair.Key.GetComponent<Chicken>();
                if (chicken != null && chicken.HasAttacked())
                {
                    destroyedChickens.Add(pair.Key);
                }
            }

            foreach (var chicken in destroyedChickens)
            {
                chickenEggPairs.Remove(chicken);
            }
        }
    }

    private int GetRequiredChickenCount()
    {
        float currentTime = Time.time;

        // Handle time before first schedule point
        if (currentTime <= schedule[0].time)
            return schedule[0].chickens;

        // Handle time after last schedule point
        if (currentTime >= schedule[schedule.Length - 1].time)
            return schedule[schedule.Length - 1].chickens;

        // Find the two schedule points we're between
        for (int i = 0; i < schedule.Length - 1; i++)
        {
            if (currentTime >= schedule[i].time && currentTime < schedule[i + 1].time)
            {
                float t = (currentTime - schedule[i].time) / (schedule[i + 1].time - schedule[i].time);
                float lerpedCount = Mathf.Lerp(schedule[i].chickens, schedule[i + 1].chickens, t);
                return Mathf.FloorToInt(lerpedCount);
            }
        }

        // Shouldn't reach here, but return minimum count if we do
        return schedule[0].chickens;
    }

    private GameObject FindBestEggForSpawning()
    {
        if (Camera.main == null) return null;

        GameObject bestEgg = null;
        float nearestDistance = float.MaxValue;
        Vector3 cameraPos = Camera.main.transform.position;

        foreach (Transform eggTransform in eggContainer.transform)
        {
            // Skip if egg is already spawning a chicken that hasn't attacked yet
            if (chickenEggPairs.ContainsValue(eggTransform.gameObject))
                continue;

            // Check line of sight to player
            Vector3 directionToCamera = (cameraPos - eggTransform.position).normalized;
            float distanceToCamera = Vector3.Distance(eggTransform.position, cameraPos);

            if (Physics.Raycast(eggTransform.position, directionToCamera, distanceToCamera, wallLayer))
            {
                // No line of sight to player, this egg is a candidate
                float distance = distanceToCamera;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    bestEgg = eggTransform.gameObject;
                }
            }
        }

        return bestEgg;
    }

    private GameObject SpawnChickenAtEgg(GameObject egg)
    {
        GameObject chicken = Instantiate(chickenPrefab, egg.transform.position, Quaternion.identity);
        chicken.transform.parent = chickenContainer.transform;
        chickenEggPairs.Add(chicken, egg);
        return chicken;
    }
}
