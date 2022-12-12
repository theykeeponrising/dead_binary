using System.Collections.Generic;
using UnityEngine;

//Handles all the SFX for a particular character, e.g. footsteps
public class UnitSFX
{
    private readonly Unit _unit;
    private readonly AudioSource _audioSource;
    private readonly FootstepData _footstepData;
    private readonly Dictionary<AnimationType, AudioSource> _bodyAudioSources = new();

    private UnitAttributes Attributes { get { return _unit.Attributes; } }

    public UnitSFX(Unit unit, AudioSource audioSource)
    {
        _unit = unit;
        _audioSource = audioSource;
        _footstepData = new FootstepData(unit);

        GetFootsteps();
    }

    private void GetFootsteps()
    {
        if (Attributes.FootstepSource == FootstepSource.FLYING)
        {
            _bodyAudioSources[AnimationType.FOOTSTEP_LEFT] = _unit.GetComponent<AudioSource>();
            _bodyAudioSources[AnimationType.FOOTSTEP_RIGHT] = _unit.GetComponent<AudioSource>();
        }
        else
        {
            _bodyAudioSources[AnimationType.FOOTSTEP_LEFT] = _unit.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<AudioSource>();
            _bodyAudioSources[AnimationType.FOOTSTEP_RIGHT] = _unit.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<AudioSource>();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    private void PlayFootstepSound(AnimationType whichFoot)
    {
        // Plays a random footstep sound based on tile data

        AudioClip footstep = AudioManager.GetSound(_footstepData);
        AudioSource footAudioSource = _bodyAudioSources[whichFoot];

        // Prevent overlapping footstep sounds from the same foot
        if (!footAudioSource.isPlaying)
            footAudioSource.Stop();
        
        if (_unit.MoveData.Velocity.z > 0.75f)
            footAudioSource.PlayOneShot(footstep);
    }

    public void PlayImpactSound()
    {
        AudioClip impactSound = AudioManager.GetSound(_unit.impactType);
        PlayOneShot(impactSound);
    }

    public void PlayRandomAnimationSound(AnimationType sound)
    {
        AudioClip throwSound = AudioManager.GetSound(sound);
        PlayOneShot(throwSound);
    }

    public void Event_PlaySound(AnimationType sound)
    {
        switch (sound)
        {
            case (AnimationType.FOOTSTEP_LEFT):
                PlayFootstepSound(AnimationType.FOOTSTEP_LEFT);
                break;
            case (AnimationType.FOOTSTEP_RIGHT):
                PlayFootstepSound(AnimationType.FOOTSTEP_RIGHT);
                break;
            case (AnimationType.SHOOT):
                _unit.EquippedWeapon.Shoot();
                break;
            case (AnimationType.THROW): case (AnimationType.PRIME): case (AnimationType.SWAP):
                PlayRandomAnimationSound(sound);
                break;
            case (AnimationType.IMPACT):
                PlayImpactSound();
                break;
        }
        return;
    }
}
