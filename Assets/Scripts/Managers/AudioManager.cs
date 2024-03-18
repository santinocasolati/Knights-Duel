using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public List<Audio> audioStore;

    private AudioSource audioSource;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string audio)
    {
        // By using only one AudioSource, the general performance is improved
        Audio selectedAudio = _instance.audioStore.FirstOrDefault(a => a.name == audio);

        if (selectedAudio == null) return;

        _instance.audioSource.clip = selectedAudio.clip;
        _instance.audioSource.PlayOneShot(selectedAudio.clip);
    }
}

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;
}