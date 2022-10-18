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

    private Dictionary<FootstepMaterial, Dictionary<FootstepSource, AudioClip[]>> GetDict()
    {
        var footstepsType = new Dictionary<FootstepSource, AudioClip[]>
        {
            {FootstepSource.HUMAN, _footstepConcrete },
            {FootstepSource.SCRAPBOT, _footstepScrapBot },
        };

        var FootstepSounds = new Dictionary<FootstepMaterial, Dictionary<FootstepSource, AudioClip[]>>
        {
            {FootstepMaterial.CONCRETE, footstepsType },
        };

        return FootstepSounds;
    }

    public AudioClip GetSound(FootstepData footstepData, int index)
    {
        // Returns a specific sound for the provided type and index

        FootstepMaterial footstepMaterial = footstepData.footstepMaterial;
        FootstepSource footstepSource = footstepData.footstepSource;

        var dict = GetDict();
        return dict[footstepMaterial][footstepSource][index];
    }

    public AudioClip GetSound(FootstepData footstepData)
    {
        // Returns a random sound for the provided type

        FootstepMaterial footstepMaterial = footstepData.footstepMaterial;
        FootstepSource footstepSource = footstepData.footstepSource;

        var dict = GetDict();
        int range = dict[footstepMaterial][footstepSource].Length;
        return dict[footstepMaterial][footstepSource][Random.Range(0, range)];
    }
}

public class FootstepData
{
    Unit unit;
    public FootstepMaterial footstepMaterial => unit.currentTile.footstepMaterial;
    public FootstepSource footstepSource => unit.attributes.footstepSource;

    public FootstepData(Unit unit) { this.unit = unit; }
}

public enum FootstepSource { HUMAN, SCRAPBOT }
public enum FootstepMaterial { CONCRETE }; // METAL, DIRT, WATER
