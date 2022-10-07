using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Used to store and quickly reference various audio clip arrays

    public static AudioManager Instance = null;
    AudioSource audioSource;

    ////////////////
    // Soundtrack //
    ////////////////

    public AudioClip musicGroundZero;

    ///////////////////
    // Sound Effects //
    ///////////////////

    public SFXAmbience Ambience;
    public SFXAnimations Animation;
    public SFXFootsteps Footstep;
    public SFXImpact Impact;
    public SFXInterface Interface;
    public SFXItems Items;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        // TO DO -- Add a more robust music system
        PlayMusic(musicGroundZero);
    }
    
    public void PlayMusic(AudioClip audioClip)
    {
        // Plays provided music clip

        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
