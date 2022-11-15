using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Used to store and quickly reference various audio clip arrays

    public static AudioManager Instance = null;
    private AudioSource _audioSource;

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
    [SerializeField] private SFXDialog Dialog;

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();

        // TO DO -- Add a more robust music system
        PlayMusic(musicGroundZero);
    }
    
    public void PlayMusic(AudioClip audioClip)
    {
        // Plays provided music clip

        if (!playMusic) return;

        _audioSource.clip = audioClip;
        _audioSource.Play();
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
    public static AudioClip GetSound(ImpactTypes soundType)
    { return Instance.Impact.GetSound(soundType); }

    public static AudioClip GetSound(ImpactTypes soundType, int index)
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

    // Dialog
    public static AudioClip GetSound(DialogVoice soundType)
    { return Instance.Dialog.GetSound(soundType); }
}
