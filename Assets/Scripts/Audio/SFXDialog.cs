using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call interface sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXDialog
{
    [SerializeField] private AudioClip _dialog;
    [SerializeField] private AudioClip _dialogLow;
    [SerializeField] private AudioClip _dialogDeep;

    private Dictionary<DialogVoice, AudioClip> GetDict()
    {
        var interfaceSounds = new Dictionary<DialogVoice, AudioClip> 
        {
            {DialogVoice.NORMAL, _dialog },
            {DialogVoice.LOW, _dialogLow },
            {DialogVoice.DEEP, _dialogDeep },
        };

        return interfaceSounds;
    }

    public AudioClip GetSound(DialogVoice interfaceSFX)
    {
        // Returns a specific sound for the provided type

        var dict = GetDict();
        return dict[interfaceSFX];
    }

}

public enum DialogVoice { NORMAL, LOW, DEEP }; 
