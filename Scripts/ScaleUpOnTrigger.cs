using UnityEngine;
using System.Collections.Generic;

public class ScaleUpOnTrigger : MonoBehaviour
{
    // Public GameObject that will be scaled
    public GameObject targetObject;

    // List of GameObjects with colliders to enable when triggered
    public List<GameObject> newColliders;

    // Scale multiplier per second
    public float scaleMultiplier = 1.5f;

    // Maximum scale multiplier
    private float maxScale = 50f;

    // Store original scale
    private Vector3 originalScale;

    // Keep track of scaling state
    private bool isScaling = false;

    void Start()
    {
        // Save the original scale of the target object
        if (targetObject != null)
        {
            originalScale = targetObject.transform.localScale;
        }
        else
        {
            Debug.LogWarning("Target Object is not assigned.");
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
        if (other.CompareTag("Player") && targetObject != null && !isScaling)
        {
            isScaling = true;
            EnableNewColliders();
            StartCoroutine(ScaleUp());
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

    private System.Collections.IEnumerator ScaleUp()
    {
        while (isScaling && targetObject.transform.localScale.x < originalScale.x * maxScale)
        {
            // Increase scale
            targetObject.transform.localScale *= (1 + (scaleMultiplier * Time.deltaTime));

            // Check if scale has reached the limit
            if (targetObject.transform.localScale.x >= originalScale.x * maxScale)
            {
                targetObject.transform.localScale = originalScale * maxScale;
                isScaling = false;
            }

            yield return null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Stop scaling when the object exits the trigger, if desired
        if (other.CompareTag("Player"))
        {
            isScaling = false;
        }
    }
}
