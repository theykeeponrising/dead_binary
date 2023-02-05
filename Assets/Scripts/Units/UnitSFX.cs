using System.Collections.Generic;
using UnityEngine;

//Handles all the SFX for a particular character, e.g. footsteps
public class UnitSFX
{
    private readonly Unit _unit;
    private readonly AudioSource _audioSource;
    private readonly AudioSource _audioSourceLoop;
    private readonly FootstepData _footstepData;
    private readonly Dictionary<AnimationType, AudioSource> _bodyAudioSources = new();

    private UnitAttributes Attributes { get { return _unit.Attributes; } }

    public UnitSFX(Unit unit)
    {
        _unit = unit;
        _audioSource = unit.GetComponents<AudioSource>()[0];
        _audioSourceLoop = unit.GetComponents<AudioSource>()[1];
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

    public void StopSound()
    { _audioSource.Stop(); }

    public void StopSoundLoop()
    { _audioSourceLoop.Stop(); }

    public void PlayImpactSound()
    { PlaySound(AudioManager.GetSound(_unit.impactType)); }

    public void PlayRandomAnimationSound(AnimationType sound)
    { PlaySound(AudioManager.GetSound(sound)); }

    public void PlayIdleSound()
    { PlaySoundLoop(AudioManager.GetSound(Attributes.UnitIdleType)); }

    public void PlayDeathSound()
    { PlaySound(AudioManager.GetSound(Attributes.UnitDeathType)); }

    public void PlaySound(AudioClip clip)
    {
        if (clip)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundLoop(AudioClip clip)
    {
        if (!_audioSourceLoop.isPlaying)
        {
            _audioSourceLoop.clip = clip;
            _audioSourceLoop.Play();
        }
    }

    public void PlaySound(AnimationType sound)
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
            case (AnimationType.SHOOT_ALT):
                _unit.AltWeapon.Shoot();
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
}
