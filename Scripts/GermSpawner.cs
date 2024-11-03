using UnityEngine;
using System.Collections.Generic;

public class GermSpawner : MonoBehaviour
{
    public GameObject germPrefab;                     // Prefab of the germ to spawn
    public Transform playerTransform;                 // Reference to the player
    public int initialGermCount = 5;                  // Number of germs to spawn initially
    public float spawnInterval = 5f;                  // Interval between spawns in seconds
    public List<Transform> spawnPoints;               // List of potential spawn points

    // Scaling parameters
    public float minScale = 30f;                      // Minimum scale of the germs
    public float maxScale = 90f;                      // Maximum scale of the germs

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("Player not found! Ensure there is an object tagged 'Player' in the scene.");
        }

        // Initial spawn of germs
        for (int i = 0; i < initialGermCount; i++)
        {
            SpawnGerm();
        }

        // Repeatedly spawn germs at set intervals
        InvokeRepeating(nameof(SpawnGerm), spawnInterval, spawnInterval);
    }

    private void SpawnGerm()
    {
        if (playerTransform != null && germPrefab != null && spawnPoints.Count > 0)
        {
            // Find the farthest spawn point from the player
            Transform farthestSpawnPoint = GetFarthestSpawnPoint();

            if (farthestSpawnPoint != null)
            {
                // Select a random scale between minScale and maxScale
                float randomScale = Random.Range(minScale, maxScale);

                // Instantiate the germ at the farthest spawn point with the random scale
                GameObject newGerm = Instantiate(germPrefab, farthestSpawnPoint.position, Quaternion.identity);
                newGerm.transform.localScale = Vector3.one * randomScale;
            }
        }
        else
        {
            Debug.LogWarning("GermPrefab, PlayerTransform, or spawn points are not properly assigned!");
        }
    }

    private Transform GetFarthestSpawnPoint()
    {
        Transform farthestPoint = null;
        float maxDistance = 0f;

        foreach (Transform spawnPoint in spawnPoints)
        {
            float distance = Vector3.Distance(playerTransform.position, spawnPoint.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPoint = spawnPoint;
            }
        }

        return farthestPoint;
    }
}
