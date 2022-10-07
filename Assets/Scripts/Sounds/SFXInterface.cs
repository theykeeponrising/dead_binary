using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call interface sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXInterface
{
    [SerializeField] AudioClip[] interfaceMouseClick;
    [SerializeField] AudioClip[] interfaceMouseOver;

    Dictionary<InterfaceSFX, AudioClip[]> GetDict()
    {
        var interfaceSounds = new Dictionary<InterfaceSFX, AudioClip[]> 
        {
            {InterfaceSFX.MOUSE_CLICK, interfaceMouseClick },
            {InterfaceSFX.MOUSE_OVER, interfaceMouseOver },
        };

        return interfaceSounds;
    }

    public AudioClip GetSound(InterfaceSFX interfaceSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[interfaceSFX][index];
    }

    public AudioClip GetSound(InterfaceSFX interfaceSFX)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[interfaceSFX].Length;
        return dict[interfaceSFX][Random.Range(0, range)];
    }
}

public enum InterfaceSFX { MOUSE_OVER, MOUSE_CLICK }; 
