using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call item sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXItems
{
    [SerializeField] private AudioClip[] _itemEffectExplosion;
    [SerializeField] private AudioClip[] _itemEffectMedkit;

    private Dictionary<ItemEffectType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<ItemEffectType, AudioClip[]>
        {
            {ItemEffectType.EXPLOSION, _itemEffectExplosion },
            {ItemEffectType.MEDKIT_SPRAY, _itemEffectMedkit },
        };
        return dict;
    }

    public AudioClip GetSound(ItemEffectType animationSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[animationSFX][index];
    }

    public AudioClip GetSound(ItemEffectType animationSFX)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[animationSFX].Length;
        return dict[animationSFX][Random.Range(0, range)];
    }
}

public enum ItemEffectType { NONE, EXPLOSION, MEDKIT_SPRAY, MEDKIT_BAG };
