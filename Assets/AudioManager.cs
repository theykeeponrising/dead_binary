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
    }

    public AudioClip GetImpactSound(ImpactType impactType, int index)
    {
        // Used to get a specific impact sound

        return ImpactSounds[impactType][index];
    }

    public AudioClip GetRandomImpactSound(ImpactType impactType)
    {
        // Returns a random impact sound for impact type

        int range = Instance.impactFlesh.Length;
        return ImpactSounds[impactType][Random.Range(0, range)];
    }

    public enum ImpactType { FLESH, ROBOT, CONCRETE };

    public AudioClip[] impactFlesh;
    public AudioClip[] impactRobot;
    public AudioClip[] impactConcrete;

    public Dictionary<ImpactType, AudioClip[]> ImpactSounds;
}
