using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call interface sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXInterface
{
    [SerializeField] private AudioClip[] _interfaceMouseClick;
    [SerializeField] private AudioClip[] _interfaceMouseOver;
    [SerializeField] private AudioClip[] _interfaceTargeting;
    [SerializeField] private AudioClip[] _interfaceSwitchTarget;

    private Dictionary<InterfaceType, AudioClip[]> GetDict()
    {
        var interfaceSounds = new Dictionary<InterfaceType, AudioClip[]> 
        {
            {InterfaceType.MOUSE_CLICK, _interfaceMouseClick },
            {InterfaceType.MOUSE_OVER, _interfaceMouseOver },
            {InterfaceType.TARGETING, _interfaceTargeting },
            {InterfaceType.SWITCH_TARGET, _interfaceSwitchTarget },
        };

        return interfaceSounds;
    }

    public AudioClip GetSound(InterfaceType interfaceSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[interfaceSFX][index];
    }

    public AudioClip GetSound(InterfaceType interfaceSFX)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[interfaceSFX].Length;
        return dict[interfaceSFX][Random.Range(0, range)];
    }
}

public enum InterfaceType { MOUSE_OVER, MOUSE_CLICK, TARGETING, SWITCH_TARGET }; 
