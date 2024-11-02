using UnityEngine;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    // Audio clips to play in sequence
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;

    public DoorScript doorA;
    public DoorScript doorB;
    public GameObject storyItem3;

    private AudioSource audioSource;

    // Flags to check if each audio sequence has already started
    private bool audio1Played = false;
    private bool audio2Played = false;
    private bool audio3Played = false;

    private void Start()
    {
        // Create an AudioSource component to play audio clips
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayInitialAudioSequence());
    }

    private IEnumerator PlayInitialAudioSequence()
    {
        // Play audio 1 and wait for it to finish
        if (!audio1Played)
        {
            audio1Played = true; // Set flag to true to prevent re-triggering
            audioSource.clip = audio1;
            audioSource.Play();
            yield return new WaitForSeconds(audio1.length - 5f); // Adjusted for early door opening

            // Open doorA 5 seconds before audio 1 ends
            doorA.Open();
            yield return new WaitForSeconds(5f);
        }
    }

    public void StartAudio2Sequence()
    {
        if (!audio2Played) // Check flag before starting sequence
        {
            audio2Played = true; // Set flag to true to prevent re-triggering
            StartCoroutine(PlayAudio2Sequence());
        }
    }

    private IEnumerator PlayAudio2Sequence()
    {
        // Play audio 2
        audioSource.clip = audio2;
        audioSource.Play();
        yield return new WaitForSeconds(audio2.length - 5f); // Adjusted for early door opening

        // Open doorB 5 seconds before audio 2 ends
        doorB.Open();
        yield return new WaitForSeconds(5f);
    }

    public void StartAudio3Sequence()
    {
        if (!audio3Played) // Check flag before starting sequence
        {
            audio3Played = true; // Set flag to true to prevent re-triggering
            StartCoroutine(PlayAudio3Sequence());
        }
    }

    private IEnumerator PlayAudio3Sequence()
    {
        // Play audio 3
        audioSource.clip = audio3;
        audioSource.Play();
        yield return new WaitForSeconds(audio3.length);

        // Enable storyItem3 once audio 3 finishes
        if (storyItem3 != null)
        {
            storyItem3.SetActive(true);
        }
    }
}
