using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call unit idle sounds.
/// </summary>
/// 

[System.Serializable]
public class SFXIdle
{
    [SerializeField] private AudioClip[] _idleDrone;

    private Dictionary<IdleType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<IdleType, AudioClip[]>
        {
            {IdleType.NONE, null },
            {IdleType.DRONE, _idleDrone },
        };
        return dict;
    }

    public AudioClip GetSound(IdleType animationSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        if (animationSFX == IdleType.NONE)
            return null;

        var dict = GetDict();
        return dict[animationSFX][index];
    }

    public AudioClip GetSound(IdleType animationSFX)
    {
        // Returns a random sound for the provided type

        if (animationSFX == IdleType.NONE)
            return null;

        var dict = GetDict();
        int range = dict[animationSFX].Length;
        return dict[animationSFX][Random.Range(0, range)];
    }
}

public enum IdleType { NONE, DRONE };
