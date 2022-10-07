using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour
{
    // Used by parent Tile script to locate any appplicable cover objects

    public enum CoverSize { half, full }
    public CoverSize coverSize;
    public bool canVaultOver;

    AudioSource audioSource;
    public ImpactType impactType;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Impact()
    {
        // Impact noise that is played when character is protected by cover

        AudioClip audioClip = AudioManager.Instance.Impact.GetSound(impactType);
        audioSource.PlayOneShot(audioClip);
    }

    public int CoverBonus()
    {
        // Returns dodge chance percent bonus provided by cover

        if (coverSize == CoverSize.half)
            return 30;
        else if (coverSize == CoverSize.full)
            return 60;
        return 0;
    }
}
