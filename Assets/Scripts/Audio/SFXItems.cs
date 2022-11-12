using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call item sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXItems
{
    [SerializeField] private AudioClip[] _itemEffectExplGrenade;
    [SerializeField] private AudioClip[] _itemEffectExplRocket;
    [SerializeField] private AudioClip[] _itemEffectMedkitSpray;
    [SerializeField] private AudioClip[] _itemEffectMedkitBag;

    private Dictionary<ItemEffectType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<ItemEffectType, AudioClip[]>
        {
            {ItemEffectType.EXPL_GRENADE, _itemEffectExplGrenade },
            {ItemEffectType.EXPL_ROCKET, _itemEffectExplRocket },
            {ItemEffectType.MEDKIT_SPRAY, _itemEffectMedkitSpray },
            {ItemEffectType.MEDKIT_BAG, _itemEffectMedkitBag },
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

public enum ItemEffectType { NONE, EXPL_GRENADE, EXPL_ROCKET, MEDKIT_SPRAY, MEDKIT_BAG };
