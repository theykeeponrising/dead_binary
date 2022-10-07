using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAmbience : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AmbienceType ambienceSFX;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayAmbience();
    }

    void PlayAmbience()
    {
        // Plays ambience sound
        if (!audioSource.isPlaying)
        {
            audioSource.clip = AudioManager.Instance.Ambience.GetSound(ambienceSFX);
            audioSource.Play();
        }
    }
}
