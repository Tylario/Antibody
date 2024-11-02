using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private bool isOpen = false;
    private Quaternion targetRotation;
    private float rotationSpeed = 90f / 2f; // 90 degrees over 2 seconds

    // Method to open the door
    public void Open()
    {
        if (!isOpen)
        {
            targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 90f, 0);
            StartCoroutine(RotateDoor());
            isOpen = true;
        }
    }

    private System.Collections.IEnumerator RotateDoor()
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            // Rotate smoothly toward the target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Ensure the door reaches exactly the target rotation
        transform.rotation = targetRotation;
    }
}
