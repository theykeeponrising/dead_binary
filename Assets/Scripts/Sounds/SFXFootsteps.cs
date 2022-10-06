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
    [SerializeField] AudioClip[] footstepConcrete;
    [SerializeField] AudioClip[] footstepScrapBot;

    Dictionary<FootstepMaterial, Dictionary<FootstepSource, AudioClip[]>> GetDict()
    {
        var footstepsType = new Dictionary<FootstepSource, AudioClip[]>
        {
            {FootstepSource.HUMAN, footstepConcrete },
            {FootstepSource.SCRAPBOT, footstepScrapBot },
        };

        var FootstepSounds = new Dictionary<FootstepMaterial, Dictionary<FootstepSource, AudioClip[]>>
        {
            {FootstepMaterial.CONCRETE, footstepsType },
        };

        return FootstepSounds;
    }

    public AudioClip GetSound(FootstepMaterial footstepMaterial, FootstepSource footstepSource, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[footstepMaterial][footstepSource][index];
    }

    public AudioClip GetRandomSound(FootstepMaterial footstepMaterial, FootstepSource footstepSource)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[footstepMaterial][footstepSource].Length;
        return dict[footstepMaterial][footstepSource][Random.Range(0, range)];
    }
}

public enum FootstepSource { HUMAN, SCRAPBOT }
public enum FootstepMaterial { CONCRETE }; // METAL, DIRT, WATER
