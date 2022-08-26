using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Used to store and quickly reference various audio clip arrays

    public static AudioManager Instance = null;

    void Start()
    {
        Instance = this;
        ImpactSounds = new Dictionary<ImpactType, AudioClip[]> {
            {ImpactType.FLESH, Instance.impactFlesh },
            {ImpactType.ROBOT, Instance.impactRobot },
            {ImpactType.CONCRETE, Instance.impactConcrete },
        };
        FootstepSounds = new Dictionary<FootstepType, AudioClip[]> {
            {FootstepType.CONCRETE, Instance.footstepConcrete },
        };
    }

    // Impact sounds //

    public AudioClip GetImpactSound(ImpactType impactType, int index)
    {
        // Used to get a specific impact sound

        return ImpactSounds[impactType][index];
    }

    public AudioClip GetRandomImpactSound(ImpactType impactType)
    {
        // Returns a random impact sound for impact type

        int range = Instance.ImpactSounds[impactType].Length;
        return ImpactSounds[impactType][Random.Range(0, range)];
    }

    public enum ImpactType { FLESH, ROBOT, CONCRETE };

    public AudioClip[] impactFlesh;
    public AudioClip[] impactRobot;
    public AudioClip[] impactConcrete;

    public Dictionary<ImpactType, AudioClip[]> ImpactSounds;

    // Footstep sounds //

    public AudioClip GetFootstepSound(FootstepType footstepType, int index)
    {
        // Used to get a specific footstep sound

        return FootstepSounds[footstepType][index];
    }

    public AudioClip GetRandomFootstepSound(FootstepType footstepType)
    {
        // Returns a random footstep sound for footstep type

        int range = Instance.FootstepSounds[footstepType].Length;
        return FootstepSounds[footstepType][Random.Range(0, range)];
    }

    public enum FootstepType { CONCRETE }; // METAL, DIRT, WATER

    public AudioClip[] footstepConcrete;
    // public AudioClip[] footstepMetal;
    // public AudioClip[] footstepDirt;
    // public AudioClip[] footstepWater;

    public Dictionary<FootstepType, AudioClip[]> FootstepSounds;
}
