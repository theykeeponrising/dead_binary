using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Main class used for weapon objects

    public enum WeaponType { Gun, Melee, Shield }
    public WeaponType weaponType;

    public enum WeaponHeld { Onehand, Offhand, Twohand }
    public WeaponHeld weaponHeld;
    public int weaponLayer;

    public enum WeaponFamily { MELEE, PISTOL, SMG, SHOTGUN, RIFLE, AR, LMG, SHIELD, LAUNCHER }
    public WeaponFamily weaponFamily;
    public enum WeaponImpact { LIGHT, MEDIUM, HEAVY }
    public WeaponImpact weaponImpact;
    public enum WeaponSound { FIRE, RELOAD, SWAP };

    [SerializeField]  AudioClip[] fireSound;
    [SerializeField] AudioClip[] reloadSound;
    [SerializeField] AudioClip[] swapSound;

    [SerializeField] Vector3 offset;

    ParticleSystem gunParticles;
    Light gunLight;

    [System.Serializable]
    public class Attributes
    {
        public float animSpeed = 1.0f;
    }
    public Attributes attributes;

    [System.Serializable]
    public class Stats
    {
        public int damage;
        public int minRange;
        public int maxRange;
        public int ammoMax;
        public int ammoCurrent;
        public float reloadSpeed;
        //Note: Not capped at 1.
        public float baseAccuracyModifier;
        public float overRangeAccuracyPenalty;
    }
    public Stats stats;

    private void Start()
    {
        // Weapon always starts with full ammo
        stats.ammoCurrent = stats.ammoMax;
    }

    private void Awake()
    {
        gunParticles = GetComponentInChildren<ParticleSystem>();
        gunLight = GetComponentInChildren<Light>();
    }

    public void DefaultPosition(Character parent)
    {
        // Used to place newly-created weapon objects into the default position

        transform.parent = parent.body.handRight.Find("AttachPoint");
        transform.position = transform.parent.position;
        transform.localPosition = transform.localPosition + offset;

        if (weaponType == WeaponType.Gun)
            transform.rotation = transform.parent.transform.rotation;
    }

    public void Shoot()
    {
        // Used by animation to kick off shoot effect

        StartCoroutine(ShootEffect());
    }

    public int GetMinimumRange()
    {
        return stats.minRange;
    }

    public int GetMaximumRange()
    {
        return stats.maxRange;
    }

    public float GetAccuracyPenalty(int range)
    {
        int rangeDiff = range - GetMaximumRange();
        float penalty = rangeDiff > 0 ? rangeDiff * stats.overRangeAccuracyPenalty : 0.0f;
        Debug.Log(string.Format("Penalty: {0}, RangePenalty: {1}", penalty, stats.overRangeAccuracyPenalty));
        return penalty;
    }

    public IEnumerator ShootEffect()
    {
        // Display gun flash
        gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        gunParticles.Stop();
        gunParticles.Play();
        PlaySound(WeaponSound.FIRE);
        yield return new WaitForSecondsRealtime(gunParticles.main.duration);
        gunLight.enabled = false;
    }

    public void Reload()
    {
        // Reload sound effect

        PlaySound(WeaponSound.RELOAD);
    }

    public void DropGun()
    {
        // Detaches gun from Character and adds physics

        transform.parent = null;
        gameObject.AddComponent<Rigidbody>();
    }

    public void PlaySound(WeaponSound weaponSound, Character character=null)
    {
        // Plays sound from selected weapon sound choice
        // Optionally can play sound from Character's audio source instead of weapon

        AudioClip audioClip = null;
        AudioSource audioSource = GetComponent<AudioSource>();

        if (character)
            audioSource = character.GetComponent<AudioSource>();

        // Determine sound to play
        switch (weaponSound)
        {
            case (WeaponSound.FIRE):
                audioClip = fireSound[Random.Range(0, fireSound.Length)];
                break;
            case (WeaponSound.RELOAD):
                audioClip = reloadSound[Random.Range(0, reloadSound.Length)];
                break;
            case (WeaponSound.SWAP):
                audioClip = swapSound[Random.Range(0, swapSound.Length)];
                break;
        }

        // Play the sound at the desired audio source
        audioSource.PlayOneShot(audioClip);
    }
}