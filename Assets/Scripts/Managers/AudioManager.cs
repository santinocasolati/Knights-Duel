using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public List<Audio> audioStore;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string audio)
    {
        Audio selectedAudio = audioStore.FirstOrDefault(a => a.name == audio);

        if (selectedAudio == null) return;

        audioSource.clip = selectedAudio.clip;
        audioSource.PlayOneShot(selectedAudio.clip);
    }
}

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;
}