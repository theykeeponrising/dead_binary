using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call impact sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXImpact
{
    [SerializeField] private AudioClip[] _impactFlesh;
    [SerializeField] private AudioClip[] _impactRobot;
    [SerializeField] private AudioClip[] _impactConcrete;
    [SerializeField] private AudioClip[] _impactMetal;

    private Dictionary<ImpactTypes, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<ImpactTypes, AudioClip[]> {
            {ImpactTypes.FLESH, _impactFlesh },
            {ImpactTypes.ROBOT, _impactRobot },
            {ImpactTypes.CONCRETE, _impactConcrete },
            {ImpactTypes.METAL, _impactMetal },
        };

        return dict;
    }
    public AudioClip GetSound(ImpactTypes impactType, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[impactType][index];
    }

    public AudioClip GetSound(ImpactTypes impactType)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[impactType].Length;
        return dict[impactType][Random.Range(0, range)];
    }
}

public enum ImpactTypes { FLESH, ROBOT, CONCRETE, METAL };
