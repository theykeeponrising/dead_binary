using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CharacterSFX
{
    Character character;
    AudioSource audioSource;
    public enum AnimationEventSound { NONE, IMPACT, FOOTSTEP_LEFT, FOOTSTEP_RIGHT };

    public CharacterSFX(Character character)
    {
        this.character = character;
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

        AudioClip footstep = AudioManager.Instance.GetRandomFootstepSound(character.currentTile.footstepType);
        AudioSource footAudioSource;

        // Determine which foot to play the sound at
        if (whichFoot == AnimationEventSound.FOOTSTEP_LEFT)
            footAudioSource = character.GetAnimator().GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<AudioSource>();
        else
            footAudioSource = character.GetAnimator().GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<AudioSource>();

        // Prevent overlapping footstep sounds from the same foot
        if (!footAudioSource.isPlaying)
            footAudioSource.Stop();
        
        if (character.velocityZ > 0.75f)
            footAudioSource.PlayOneShot(footstep);
    }

    public void PlayRandomImpactSound()
    {
        AudioClip impactSound = AudioManager.Instance.GetRandomImpactSound(character.impactType);
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
        }
        return;
    }
}