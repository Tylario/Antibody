using UnityEngine;
using TMPro;

public class BackpackRaycast : MonoBehaviour
{
    public TextMeshProUGUI instructionText; // Assign this in the Inspector
    public GunController gunController; // Assign this in the Inspector
    public float maxRayDistance = 100f; // Max distance to check
    public float rayStepDistance = 0.5f; // Step distance when hitting player

    void Update()
    {
        bool isLookingAtBackpack = false;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        float remainingDistance = maxRayDistance;

        // Continue casting rays in small steps until we hit a valid object or reach max distance
        while (remainingDistance > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, remainingDistance))
            {
                if (hit.collider.CompareTag("Backpack"))
                {
                    isLookingAtBackpack = true;
                    instructionText.text = "Press Z to Equip";

                    // Check for input when looking at backpack
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        gunController.gunsUnlocked = true;
                        instructionText.text = ""; // Clear the text
                        Debug.Log("Gun unlocked!"); // Confirm that gunsUnlocked has been set
                    }
                    break; // Exit loop when backpack is found
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    // Move the ray origin forward to continue raymarching beyond the player object
                    rayOrigin = hit.point + rayDirection * rayStepDistance;
                    remainingDistance -= rayStepDistance;
                }
                else
                {
                    // If it hits any other object that's not the player or backpack, stop raycasting
                    break;
                }
            }
            else
            {
                // Stop if we don't hit anything
                break;
            }
        }

        // Clear the text if not looking at a backpack or if gunsUnlocked is true
        if (!isLookingAtBackpack || gunController.gunsUnlocked)
        {
            instructionText.text = "";
        }
    }
}
