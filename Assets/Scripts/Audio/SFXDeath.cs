using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call unit death sounds.
/// </summary>
/// 

[System.Serializable]
public class SFXDeath
{
    [SerializeField] private AudioClip[] _deathScrapbot;
    [SerializeField] private AudioClip[] _deathDrone;

    private Dictionary<DeathType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<DeathType, AudioClip[]>
        {
            {DeathType.NONE, null },
            {DeathType.SCRAPBOT, _deathScrapbot },
            {DeathType.DRONE, _deathDrone },
        };
        return dict;
    }

    public AudioClip GetSound(DeathType animationSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        if (animationSFX == DeathType.NONE)
            return null;

        var dict = GetDict();
        return dict[animationSFX][index];
    }

    public AudioClip GetSound(DeathType animationSFX)
    {
        // Returns a random sound for the provided type

        if (animationSFX == DeathType.NONE)
            return null;

        var dict = GetDict();
        int range = dict[animationSFX].Length;
        return dict[animationSFX][Random.Range(0, range)];
    }
}

public enum DeathType { NONE, SCRAPBOT, DRONE };
