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

    private void Start()
    {
        // Create an AudioSource component to play audio clips
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayInitialAudioSequence());
    }

    private IEnumerator PlayInitialAudioSequence()
    {
        // Play audio 1 and wait for it to finish
        audioSource.clip = audio1;
        audioSource.Play();
        yield return new WaitForSeconds(audio1.length - 5f); // Adjusted for early door opening

        // Open doorA 5 seconds before audio 1 ends
        doorA.Open();
        yield return new WaitForSeconds(5f);
    }

    public void StartAudio2Sequence()
    {
        StartCoroutine(PlayAudio2Sequence());
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
        StartCoroutine(PlayAudio3Sequence());
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
