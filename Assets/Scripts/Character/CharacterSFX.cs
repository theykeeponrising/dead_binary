using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Handles all the SFX for a particular character, e.g. footsteps
public class CharacterSFX
{
    Unit unit;
    AudioSource audioSource;

    public CharacterSFX(Unit unit)
    {
        this.unit = unit;
    }

    public void PlayOneShot(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void SetAudioSource(AudioSource audioSource)
    {
        this.audioSource = audioSource;
    }

    void Footsteps(AnimationType whichFoot)
    {
        // Plays a random footstep sound based on tile data

        AudioClip footstep = AudioManager.Instance.Footstep.GetRandomSound(unit.currentTile.footstepMaterial, unit.attributes.footstepSource);
        AudioSource footAudioSource;

        // Determine which foot to play the sound at
        if (whichFoot == AnimationType.FOOTSTEP_LEFT)
            footAudioSource = unit.GetAnimator().GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<AudioSource>();
        else
            footAudioSource = unit.GetAnimator().GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<AudioSource>();

        // Prevent overlapping footstep sounds from the same foot
        if (!footAudioSource.isPlaying)
            footAudioSource.Stop();
        
        if (unit.velocityZ > 0.75f)
            footAudioSource.PlayOneShot(footstep);
    }

    public void PlayRandomImpactSound()
    {
        AudioClip impactSound = AudioManager.Instance.Impact.GetRandomSound(unit.impactType);
        PlayOneShot(impactSound);
    }

    public void PlayRandomAnimationSound(AnimationType sound)
    {
        AudioClip throwSound = AudioManager.Instance.Animation.GetRandomSound(sound);
        PlayOneShot(throwSound);
    }

    public void Event_PlaySound(AnimationType sound)
    {
        //Nothing for now.
        switch (sound)
        {
            case (AnimationType.FOOTSTEP_LEFT):
                Footsteps(AnimationType.FOOTSTEP_LEFT);
                break;
            case (AnimationType.FOOTSTEP_RIGHT):
                Footsteps(AnimationType.FOOTSTEP_RIGHT);
                break;
            case (AnimationType.SHOOT):
                unit.GetEquippedWeapon().Shoot();
                break;
            case (AnimationType.THROW): case (AnimationType.PRIME): case (AnimationType.SWAP):
                PlayRandomAnimationSound(sound);
                break;
        }
        return;
    }
}
