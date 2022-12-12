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
    [SerializeField] private AudioClip[] _footstepMetal;
    [SerializeField] private AudioClip[] _footstepFlying;

    private Dictionary<ImpactTypes, Dictionary<FootstepSource, AudioClip[]>> GetDict()
    {
        var footstepsType = new Dictionary<FootstepSource, AudioClip[]>
        {
            {FootstepSource.HUMAN, _footstepConcrete },
            {FootstepSource.METAL, _footstepMetal },
            {FootstepSource.FLYING, _footstepFlying },
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

        ImpactTypes impactType = footstepData.ImpactType;
        FootstepSource footstepSource = footstepData.FootstepSource;

        var dict = GetDict();
        return dict[impactType][footstepSource][index];
    }

    public AudioClip GetSound(FootstepData footstepData)
    {
        // Returns a random sound for the provided type

        ImpactTypes impactType = footstepData.ImpactType;
        FootstepSource footstepSource = footstepData.FootstepSource;

        var dict = GetDict();
        int range = dict[impactType][footstepSource].Length;
        return dict[impactType][footstepSource][Random.Range(0, range)];
    }
}

public class FootstepData
{
    private readonly Unit Unit;
    public ImpactTypes ImpactType => Unit.Tile.ImpactType;
    public FootstepSource FootstepSource => Unit.Attributes.FootstepSource;

    public FootstepData(Unit unit) { this.Unit = unit; }
}

public enum FootstepSource { HUMAN, METAL, FLYING }
