using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GermSpawner : MonoBehaviour
{
    // List of germ prefabs to spawn
    public List<GameObject> germs;

    // Spawn area boundaries (set in the inspector)
    public Vector3 spawnAreaMin = new Vector3(-10f, 0f, -10f);
    public Vector3 spawnAreaMax = new Vector3(10f, 0f, 10f);

    // Average time interval in seconds between spawns
    public float averageSpawnTime = 10f;

    // Variability in spawn time (e.g., ±5 seconds)
    public float spawnTimeVariance = 5f;

    // Start the spawning coroutine
    void Start()
    {
        StartCoroutine(SpawnGerms());
    }

    private IEnumerator SpawnGerms()
    {
        while (true)
        {
            // Choose a random germ from the list
            GameObject germToSpawn = germs[Random.Range(0, germs.Count)];

            // Choose a random position within the spawn area
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            // Instantiate the germ at the chosen position and rotation
            Instantiate(germToSpawn, spawnPosition, Quaternion.identity);

            // Determine the next spawn time with some random variance
            float spawnInterval = averageSpawnTime + Random.Range(-spawnTimeVariance, spawnTimeVariance);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
