using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Main class used for weapon objects
    [SerializeField] GameObject shellPrefab;
    Transform shellEject;

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

    [SerializeField] AudioClip[] fireSound;
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
        public int maxRangeNoPenalty;
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
        shellEject = transform.Find("ShellEject");
    }

    public void DefaultPosition(Unit parent)
    {
        // Used to place newly-created weapon objects into the default position

        transform.parent = parent.GetAnimator().GetWeaponDefaultPosition();
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

    public int GetDamage()
    {
        return stats.damage;
    }

    public int GetMinimumRange()
    {
        return stats.minRange;
    }

    public int GetMaximumRange()
    {
        return stats.maxRangeNoPenalty;
    }

    public float GetAccuracyPenalty(int range)
    {
        int rangeDiff = range - GetMaximumRange();
        float penalty = rangeDiff > 0 ? rangeDiff * stats.overRangeAccuracyPenalty : 0.0f;
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

        if (shellEject) EjectShell();

        yield return new WaitForSecondsRealtime(gunParticles.main.duration);
        gunLight.enabled = false;
    }

    public void ReloadEffect()
    {
        // Reload sound effect

        PlaySound(WeaponSound.RELOAD);
        stats.ammoCurrent = stats.ammoMax;
    }

    public void Reload()
    {
        // Sets current ammo to weapon's maximum

        stats.ammoCurrent = stats.ammoMax;
    }

    public void DropGun()
    {
        // Detaches gun from Character and adds physics

        transform.parent = null;
        gameObject.AddComponent<Rigidbody>();
    }

    public void PlaySound(WeaponSound weaponSound, Unit character=null)
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

    void EjectShell()
    {
        // Generate a shell as an effect and eject it with force

        float randomForce = (Random.Range(3, 5));
        GameObject shell = GlobalManager.Instance.activeMap.CreateEffect(shellPrefab, shellEject.position, Quaternion.Euler(Random.Range(5, 15), 0, Random.Range(-10, -15)));
        shell.GetComponent<Rigidbody>().AddForce(shellEject.forward * randomForce, ForceMode.VelocityChange);
        shell.GetComponent<Rigidbody>().AddForce(-shellEject.right * randomForce/2, ForceMode.VelocityChange);

    }

    public void SpendAmmo(int amount = 1)
    {
        // Spends a shot from the current ammo, defaults to 1 shot

        stats.ammoCurrent -= amount;
    }
}
