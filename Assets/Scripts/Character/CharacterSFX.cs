using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Handles all the SFX for a particular character, e.g. footsteps
public class CharacterSFX
{
    Unit unit;
    AudioSource audioSource;
    Dictionary<AnimationType, AudioSource> bodyAudioSources;
    FootstepData footstepData;

    public CharacterSFX(Unit unit)
    {
        this.unit = unit;
        footstepData = new FootstepData(unit);

        bodyAudioSources = new Dictionary<AnimationType, AudioSource>()
        { 
            { AnimationType.FOOTSTEP_LEFT, unit.GetAnimator().GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<AudioSource>() },
            { AnimationType.FOOTSTEP_RIGHT, unit.GetAnimator().GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<AudioSource>() },
        };
    }

    private void PlayOneShot(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void SetAudioSource(AudioSource audioSource)
    {
        this.audioSource = audioSource;
    }

    private void PlayFootstepSound(AnimationType whichFoot)
    {
        // Plays a random footstep sound based on tile data

        AudioClip footstep = AudioManager.GetSound(footstepData);
        AudioSource footAudioSource = bodyAudioSources[whichFoot];

        // Prevent overlapping footstep sounds from the same foot
        if (!footAudioSource.isPlaying)
            footAudioSource.Stop();
        
        if (unit.velocityZ > 0.75f)
            footAudioSource.PlayOneShot(footstep);
    }

    public void PlayRandomImpactSound()
    {
        AudioClip impactSound = AudioManager.GetSound(unit.impactType);
        PlayOneShot(impactSound);
    }

    public void PlayRandomAnimationSound(AnimationType sound)
    {
        AudioClip throwSound = AudioManager.GetSound(sound);
        PlayOneShot(throwSound);
    }

    public void Event_PlaySound(AnimationType sound)
    {
        //Nothing for now.
        switch (sound)
        {
            case (AnimationType.FOOTSTEP_LEFT):
                PlayFootstepSound(AnimationType.FOOTSTEP_LEFT);
                break;
            case (AnimationType.FOOTSTEP_RIGHT):
                PlayFootstepSound(AnimationType.FOOTSTEP_RIGHT);
                break;
            case (AnimationType.SHOOT):
                unit.EquippedWeapon.Shoot();
                break;
            case (AnimationType.THROW): case (AnimationType.PRIME): case (AnimationType.SWAP):
                PlayRandomAnimationSound(sound);
                break;
        }
        return;
    }
}
