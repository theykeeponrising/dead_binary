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
    public bool playMusic = true; // USED UNTIL OPTIONS ARE MADE

    /////////////////
    // SFX Classes //
    /////////////////

    [SerializeField] private SFXAmbience Ambience;
    [SerializeField] private SFXAnimations Animation;
    [SerializeField] private SFXFootsteps Footstep;
    [SerializeField] private SFXImpact Impact;
    [SerializeField] private SFXInterface Interface;
    [SerializeField] private SFXItems Items;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        // TO DO -- Add a more robust music system
        PlayMusic(musicGroundZero);
    }
    
    public void PlayMusic(AudioClip audioClip)
    {
        // Plays provided music clip

        if (!playMusic) return;

        audioSource.clip = audioClip;
        audioSource.Play();
    }

    // Ambience
    public static AudioClip GetSound(AmbienceType soundType)
    { return Instance.Ambience.GetSound(soundType); }

    public static AudioClip GetSound(AmbienceType soundType, int index) 
    { return Instance.Ambience.GetSound(soundType, index); }

    // Animation
    public static AudioClip GetSound(AnimationType soundType) 
    { return Instance.Animation.GetSound(soundType); }

    public static AudioClip GetSound(AnimationType soundType, int index) 
    { return Instance.Animation.GetSound(soundType, index); }

    // Footsteps
    public static AudioClip GetSound(FootstepData soundType)
    { return Instance.Footstep.GetSound(soundType); }

    public static AudioClip GetSound(FootstepData soundType, int index)
    { return Instance.Footstep.GetSound(soundType, index); }

    // Impact
    public static AudioClip GetSound(ImpactType soundType)
    { return Instance.Impact.GetSound(soundType); }

    public static AudioClip GetSound(ImpactType soundType, int index)
    { return Instance.Impact.GetSound(soundType, index); }

    // Interface
    public static AudioClip GetSound(InterfaceType soundType)
    { return Instance.Interface.GetSound(soundType); }

    public static AudioClip GetSound(InterfaceType soundType, int index)
    { return Instance.Interface.GetSound(soundType, index); }

    // Items
    public static AudioClip GetSound(ItemEffectType soundType)
    { return Instance.Items.GetSound(soundType); }

    public static AudioClip GetSound(ItemEffectType soundType, int index)
    { return Instance.Items.GetSound(soundType, index); }

}
