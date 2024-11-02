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

    // Store original scales
    private Vector3 originalScale1;
    private Vector3 originalScale2;

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

        // Initially disable all new colliders
        foreach (GameObject colliderObject in newColliders)
        {
            if (colliderObject != null)
                colliderObject.SetActive(false);
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
            // Scale gameObject1 towards its target scale
            if (gameObject1 != null)
            {
                gameObject1.transform.localScale = Vector3.Lerp(
                    originalScale1, 
                    gameObject1TargetScale, 
                    elapsedTime / timeToScale
                );
            }

            // Scale gameObject2 towards its target scale
            if (gameObject2 != null)
            {
                gameObject2.transform.localScale = Vector3.Lerp(
                    originalScale2, 
                    gameObject2TargetScale, 
                    elapsedTime / timeToScale
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
