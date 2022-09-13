using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    bool impact;

    private void OnCollisionEnter(Collision collision)
    {
        if (impact)
            return;

        if (collision.collider.gameObject.layer != LayerMask.NameToLayer("TileMap") && collision.collider.gameObject.layer != LayerMask.NameToLayer("CoverObject"))
            return;

        AudioSource audioSource = GetComponent<AudioSource>();
        AudioClip audioClip = AudioManager.Instance.GetRandomShellSound();

        audioSource.PlayOneShot(audioClip);
        impact = true;
    }
}
