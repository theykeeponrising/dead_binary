using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call impact sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXImpact
{
    [SerializeField] AudioClip[] impactFlesh;
    [SerializeField] AudioClip[] impactRobot;
    [SerializeField] AudioClip[] impactConcrete;
    [SerializeField] AudioClip[] impactMetal;

    Dictionary<ImpactType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<ImpactType, AudioClip[]> {
            {ImpactType.FLESH, impactFlesh },
            {ImpactType.ROBOT, impactRobot },
            {ImpactType.CONCRETE, impactConcrete },
            {ImpactType.METAL, impactMetal },
        };

        return dict;
    }
    public AudioClip GetSound(ImpactType impactType, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[impactType][index];
    }

    public AudioClip GetSound(ImpactType impactType)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[impactType].Length;
        return dict[impactType][Random.Range(0, range)];
    }
}

public enum ImpactType { FLESH, ROBOT, CONCRETE, METAL };
