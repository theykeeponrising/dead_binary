using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to catalogue and call one-shot animation sound effects.
/// </summary>
/// 

[System.Serializable]
public class SFXAnimations
{
    [SerializeField] private AudioClip[] _animationSwap;
    [SerializeField] private AudioClip[] _animationThrow;
    [SerializeField] private AudioClip[] _animationPrime;

    private Dictionary<AnimationType, AudioClip[]> GetDict()
    {
        var dict = new Dictionary<AnimationType, AudioClip[]>
        {
            {AnimationType.SWAP, _animationSwap },
            {AnimationType.THROW, _animationThrow },
            {AnimationType.PRIME, _animationPrime },
        };
        return dict;
    }

    public AudioClip GetSound(AnimationType animationSFX, int index)
    {
        // Returns a specific sound for the provided type and index

        var dict = GetDict();
        return dict[animationSFX][index];
    }

    public AudioClip GetSound(AnimationType animationSFX)
    {
        // Returns a random sound for the provided type

        var dict = GetDict();
        int range = dict[animationSFX].Length;
        return dict[animationSFX][Random.Range(0, range)];
    }
}

public enum AnimationType { NONE, IMPACT, FOOTSTEP_LEFT, FOOTSTEP_RIGHT, SHOOT, THROW, PRIME, SWAP, SHOOT_ALT };
