using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//Handles all the SFX for a particular character, e.g. footsteps
public class CharacterSFX
{
    Unit unit;
    AudioSource audioSource;
    public enum AnimationEventSound { NONE, IMPACT, FOOTSTEP_LEFT, FOOTSTEP_RIGHT, SHOOT };
    public AudioManager.FootstepSource footstepSource;

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

    
    void Footsteps(AnimationEventSound whichFoot)
    {
        // Plays a random footstep sound based on tile data

        AudioClip footstep = AudioManager.Instance.GetRandomFootstepSound(unit.currentTile.footstepMaterial, footstepSource);
        AudioSource footAudioSource;

        // Determine which foot to play the sound at
        if (whichFoot == AnimationEventSound.FOOTSTEP_LEFT)
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
        AudioClip impactSound = AudioManager.Instance.GetRandomImpactSound(unit.impactType);
        PlayOneShot(impactSound);
    }

    public void Event_PlaySound(AnimationEventSound sound)
    {
        //Nothing for now.
        switch (sound)
        {
            case (AnimationEventSound.FOOTSTEP_LEFT):
                Footsteps(AnimationEventSound.FOOTSTEP_LEFT);
                break;
            case (AnimationEventSound.FOOTSTEP_RIGHT):
                Footsteps(AnimationEventSound.FOOTSTEP_RIGHT);
                break;
            case (AnimationEventSound.SHOOT):
                unit.inventory.equippedWeapon.Shoot();
                break;
        }
        return;
    }
}