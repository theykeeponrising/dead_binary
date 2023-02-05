using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Used to store and quickly reference various audio clip arrays

    public static AudioManager Instance = null;
    private AudioSource[] _audioSources;
    private float _musicVolumeTarget;
    [SerializeField] private Player _player;
    [SerializeField] private bool _playMusic = true; // USED UNTIL OPTIONS ARE MADE
    [SerializeField] private float _musicTransitionTime = 20f;

    ////////////////
    // Soundtrack //
    ////////////////

    public AudioClip OSTGroundZero;
    public AudioClip OSTRestInTheShadows;
    public AudioClip OSTRestInTheShadowsCombat;

    /////////////////
    // SFX Classes //
    /////////////////

    [SerializeField] private SFXAmbience Ambience;
    [SerializeField] private SFXAnimations Animation;
    [SerializeField] private SFXDeath Death;
    [SerializeField] private SFXDialog Dialog;
    [SerializeField] private SFXFootsteps Footstep;
    [SerializeField] private SFXIdle Idle;
    [SerializeField] private SFXImpact Impact;
    [SerializeField] private SFXInterface Interface;
    [SerializeField] private SFXItems Items;


    private void Awake()
    {
        Instance = this;
        _audioSources = GetComponents<AudioSource>();
        _musicVolumeTarget = _audioSources[0].volume;

        // TO DO -- Add a more robust music system
        PlayMusic(OSTRestInTheShadows, OSTRestInTheShadowsCombat);
    }

    private void Update()
    {
        CombatMusicTransition();
    }

    public void PlayMusic(AudioClip audioClip)
    {
        // Plays provided music clip

        if (!_playMusic) return;

        _audioSources[0].clip = audioClip;
        _audioSources[0].Play();
        _audioSources[1].Play();
    }

    public void PlayMusic(AudioClip audioClip, AudioClip audioClipCombat)
    {
        // Plays provided music clip
        // Switches between combat and non-combat tracks

        if (!_playMusic) return;

        _audioSources[0].clip = audioClip;
        _audioSources[1].clip = audioClipCombat;
        _audioSources[0].Play();
        _audioSources[1].Play();
    }

    private void CombatMusicTransition()
    {
        // Gradually transitions volume between combat and non-combat audio sources

        if (_player.InCombat())
        {
            _audioSources[1].volume = Mathf.Lerp(_audioSources[1].volume, _musicVolumeTarget, Time.deltaTime / _musicTransitionTime);
            _audioSources[0].volume = Mathf.Lerp(_audioSources[0].volume, 0, Time.deltaTime / _musicTransitionTime);
        }
        else
        {
            _audioSources[0].volume = Mathf.Lerp(_audioSources[0].volume, _musicVolumeTarget, Time.deltaTime / _musicTransitionTime);
            _audioSources[1].volume = Mathf.Lerp(_audioSources[1].volume, 0, Time.deltaTime / _musicTransitionTime);
        }
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

    // Death
    public static AudioClip GetSound(DeathType soundType)
    { return Instance.Death.GetSound(soundType); }

    // Idle
    public static AudioClip GetSound(IdleType soundType)
    { return Instance.Idle.GetSound(soundType); }
}
