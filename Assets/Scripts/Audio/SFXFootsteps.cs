using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call footstep sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXFootsteps
{
    [SerializeField] private AudioClip[] _footstepConcrete;
    [SerializeField] private AudioClip[] _footstepScrapBot;

    private Dictionary<ImpactTypes, Dictionary<FootstepSource, AudioClip[]>> GetDict()
    {
        var footstepsType = new Dictionary<FootstepSource, AudioClip[]>
        {
            {FootstepSource.HUMAN, _footstepConcrete },
            {FootstepSource.SCRAPBOT, _footstepScrapBot },
        };

        var FootstepSounds = new Dictionary<ImpactTypes, Dictionary<FootstepSource, AudioClip[]>>
        {
            {ImpactTypes.CONCRETE, footstepsType },
        };

        return FootstepSounds;
    }

    public AudioClip GetSound(FootstepData footstepData, int index)
    {
        // Returns a specific sound for the provided type and index

        ImpactTypes impactType = footstepData.impactType;
        FootstepSource footstepSource = footstepData.footstepSource;

        var dict = GetDict();
        return dict[impactType][footstepSource][index];
    }

    public AudioClip GetSound(FootstepData footstepData)
    {
        // Returns a random sound for the provided type

        ImpactTypes impactType = footstepData.impactType;
        FootstepSource footstepSource = footstepData.footstepSource;

        var dict = GetDict();
        int range = dict[impactType][footstepSource].Length;
        return dict[impactType][footstepSource][Random.Range(0, range)];
    }
}

public class FootstepData
{
    Unit unit;
    public ImpactTypes impactType => unit.Tile.ImpactType;
    public FootstepSource footstepSource => unit.Attributes.FootstepSource;

    public FootstepData(Unit unit) { this.unit = unit; }
}

public enum FootstepSource { HUMAN, SCRAPBOT }
