using UnityEngine;
using TMPro;
using System.Collections;

public class ScrollingTextReveal : MonoBehaviour
{
    public TextMeshPro textDisplay; // Reference to the TextMeshPro text object
    public float revealDuration = 45f; // Total time to go through the entire text
    public float segmentDisplayTime = 3f; // Time each segment is displayed before scrolling to the next
    private string fullText; // Complete message text
    private string[] sentences; // Array of sentences or segments to reveal individually
    private int currentSegmentIndex = 0; // Index of the current segment being displayed

    private void Start()
    {
        // Define the full text as segments (by splitting into sentences or phrases)
        fullText = "Good morning, Operative. Welcome back to duty. We have a critical assignment for you, and every second counts. Today, you’re not just a soldier—you’re humanity’s line of defense.\n\n" +
                   "The world beyond these walls is under siege. This epidemic has spread faster and more dangerously than anything we’ve faced. Our standard forces have been outmatched. But you… you are part of something different. Something that just might turn the tide.\n\n" +
                   "Prepare yourself. Arm up and proceed to the training range. Today, you’ll need every skill, every instinct, and every bit of resolve. The fate of thousands is in your hands.\n\n" +
                   "Welcome to the front line.";

        sentences = fullText.Split(new char[] { '.', '!', '?' }); // Split into segments based on punctuation

        // Start the scrolling reveal
        StartCoroutine(DisplaySegments());
    }

    private IEnumerator DisplaySegments()
    {
        float totalRevealTime = revealDuration; // Total time to go through the entire text
        float timePerSegment = totalRevealTime / sentences.Length; // Time for each segment to appear
        float displayTime = Mathf.Min(segmentDisplayTime, timePerSegment); // Choose the shorter of the two times

        while (currentSegmentIndex < sentences.Length)
        {
            textDisplay.text = sentences[currentSegmentIndex].Trim() + "."; // Display the current segment with a period
            yield return new WaitForSeconds(displayTime); // Wait before moving to the next segment
            currentSegmentIndex++; // Move to the next segment
        }

        textDisplay.text = ""; // Clear the display after finishing
    }
}
