using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    bool impact;
    [SerializeField] AudioClip[] sfxShellCollision;

    private void OnCollisionEnter(Collision collision)
    {
        if (impact)
            return;

        if (collision.collider.gameObject.layer != LayerMask.NameToLayer("TileMap") && collision.collider.gameObject.layer != LayerMask.NameToLayer("CoverObject"))
            return;

        int range = sfxShellCollision.Length;
        AudioSource audioSource = GetComponent<AudioSource>();
        AudioClip audioClip = sfxShellCollision[Random.Range(0, range)];

        audioSource.PlayOneShot(audioClip);
        impact = true;
    }
}
