using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call ambience sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXAmbience
{
    [SerializeField] AudioClip[] ambienceCityOutside;

    Dictionary<AmbienceType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<AmbienceType, AudioClip[]> 
        {
            {AmbienceType.URBAN_OUTSIDE, ambienceCityOutside },
        };
        return dict;
    }

    public AudioClip GetSound(AmbienceType ambienceSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[ambienceSFX][index];
    }

    public AudioClip GetSound(AmbienceType ambienceSFX)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[ambienceSFX].Length;
        return dict[ambienceSFX][Random.Range(0, range)];
    }
}
public enum AmbienceType { URBAN_OUTSIDE }; // URBAN_INSIDE, OTHER ENVS??
