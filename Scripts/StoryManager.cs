using UnityEngine;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    // Audio clips to play in sequence
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;

    public DoorScript doorA;
    public DoorScript doorB;
    public GameObject storyItem3;

    private AudioSource audioSource;

    private void Start()
    {
        // Create an AudioSource component to play audio clips
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayStorySequence());
    }

    private IEnumerator PlayStorySequence()
    {
        // Play audio 1 and wait for it to finish
        audioSource.clip = audio1;
        audioSource.Play();
        yield return new WaitForSeconds(audio1.length);

        // Open doorA and wait for it to finish
        doorA.Open();
        yield return new WaitForSeconds(2f); // Wait for doorA to open

        // Play audio 2 and wait for it to finish
        audioSource.clip = audio2;
        audioSource.Play();
        yield return new WaitForSeconds(audio2.length);

        // Open doorB and wait for it to finish
        doorB.Open();
        yield return new WaitForSeconds(2f); // Wait for doorB to open

        // Play audio 3 and wait for it to finish
        audioSource.clip = audio3;
        audioSource.Play();
        yield return new WaitForSeconds(audio3.length);

        // Enable storyItem3
        if (storyItem3 != null)
        {
            storyItem3.SetActive(true);
        }

        // Play audio 4
        audioSource.clip = audio4;
        audioSource.Play();
    }
}
