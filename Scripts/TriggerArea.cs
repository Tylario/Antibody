using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public StoryManager storyManager; // Reference to the StoryManager script
    public int triggerID; // Unique ID to identify which audio to trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && storyManager != null)
        {
            if (triggerID == 2)
            {
                storyManager.StartAudio2Sequence();
            }
            else if (triggerID == 3)
            {
                storyManager.StartAudio3Sequence();
            }
        }
    }
}
