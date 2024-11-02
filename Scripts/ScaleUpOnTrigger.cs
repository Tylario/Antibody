using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleUpOnTrigger : MonoBehaviour
{
    // GameObjects to scale
    public GameObject gameObject1;
    public GameObject gameObject2;

    // Target scales for each GameObject
    public Vector3 gameObject1TargetScale = new Vector3(2f, 2f, 2f);
    public Vector3 gameObject2TargetScale = new Vector3(3f, 3f, 3f);

    // Time in seconds to reach the target scale
    public float timeToScale = 2f;

    // List of GameObjects with colliders to enable when triggered
    public List<GameObject> newColliders;

    // A new GameObject with collider to enable after scaling is complete
    public GameObject newColliderGameObject;
    public GameObject germspawner;

    // The point light to adjust
    public Light pointLight;

    // Target intensity for the point light
    public float pointLightTargetIntensity = 3500f;

    // Store original scales and intensity
    private Vector3 originalScale1;
    private Vector3 originalScale2;
    private float originalIntensity = 0f;

    // Keep track of scaling state
    private bool isScaling = false;

    void Start()
    {
        // Save the original scales of the target objects
        if (gameObject1 != null)
        {
            originalScale1 = gameObject1.transform.localScale;
        }
        else
        {
            Debug.LogWarning("GameObject 1 is not assigned.");
        }

        if (gameObject2 != null)
        {
            originalScale2 = gameObject2.transform.localScale;
        }
        else
        {
            Debug.LogWarning("GameObject 2 is not assigned.");
        }

        // Set initial intensity of pointLight to 0
        if (pointLight != null)
        {
            originalIntensity = 0f;
            pointLight.intensity = originalIntensity;
        }
        else
        {
            Debug.LogWarning("Point Light is not assigned.");
        }

        // Initially disable all new colliders
        foreach (GameObject colliderObject in newColliders)
        {
            if (colliderObject != null)
                colliderObject.SetActive(false);
        }

        // Initially disable the newColliderGameObject
        if (newColliderGameObject != null)
        {
            newColliderGameObject.SetActive(false);
        }

        if (germspawner != null)
        {
            germspawner.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player") && !isScaling)
        {
            isScaling = true;
            EnableNewColliders();
            StartCoroutine(ScaleObjectsOverTime());
        }
    }

    private void EnableNewColliders()
    {
        // Enable all new collider GameObjects
        foreach (GameObject colliderObject in newColliders)
        {
            if (colliderObject != null)
                colliderObject.SetActive(true);
        }
    }

    private IEnumerator ScaleObjectsOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeToScale)
        {
            float t = elapsedTime / timeToScale;

            // Scale gameObject1 towards its target scale
            if (gameObject1 != null)
            {
                gameObject1.transform.localScale = Vector3.Lerp(
                    originalScale1,
                    gameObject1TargetScale,
                    t
                );
            }

            // Scale gameObject2 towards its target scale
            if (gameObject2 != null)
            {
                gameObject2.transform.localScale = Vector3.Lerp(
                    originalScale2,
                    gameObject2TargetScale,
                    t
                );
            }

            // Adjust point light intensity using a cubic function
            if (pointLight != null)
            {
                float volumeScaleFactor = Mathf.Pow(t, 3f); // Cubic to simulate volume
                pointLight.intensity = Mathf.Lerp(
                    originalIntensity,
                    pointLightTargetIntensity,
                    volumeScaleFactor
                );
            }

            // Increase the elapsed time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is exactly at the target scale
        if (gameObject1 != null)
            gameObject1.transform.localScale = gameObject1TargetScale;

        if (gameObject2 != null)
            gameObject2.transform.localScale = gameObject2TargetScale;

        // Ensure final intensity is exactly at the target intensity
        if (pointLight != null)
            pointLight.intensity = pointLightTargetIntensity;

        // Enable the newColliderGameObject after scaling is complete
        if (newColliderGameObject != null)
            newColliderGameObject.SetActive(true);
            
        if (germspawner != null)
            germspawner.SetActive(true);

        isScaling = false;
    }

    void OnTriggerExit(Collider other)
    {
        // Stop scaling when the object exits the trigger, if desired
        if (other.CompareTag("Player"))
        {
            isScaling = false;
            StopAllCoroutines();
        }
    }
}
