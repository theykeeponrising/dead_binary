using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Main class used for weapon objects
    [SerializeField] GameObject shellPrefab;
    Transform shellEject;

    public UnitAction WeaponAction;
    [SerializeField] AudioClip[] fireSound;
    [SerializeField] AudioClip[] reloadSound;

    [SerializeField] Vector3 offset;

    ParticleSystem[] gunParticles;
    Light[] gunLights;

    [System.Serializable]
    public class Attributes
    {
        public int animationLayer;
        public float animationSpeed = 1.0f;
        public WeaponType weaponType;
        public WeaponFamily weaponFamily;
        public WeaponImpact weaponImpact;
    }
    public Attributes attributes;

    [System.Serializable]
    public class Stats
    {
        public int damage;
        public int minRangeNoPenalty;
        public int maxRangeNoPenalty;
        public int ammoMax;
        public int ammoCurrent;
        public float reloadSpeed;
        //Note: Not capped at 1.
        public float baseAccuracyModifier;
        public float overRangeAccuracyPenalty;
        public float underRangeAccuracyPenalty;
        public float areaOfEffect = 1;
    }
    public Stats stats;

    private void Start()
    {
        // Weapon always starts with full ammo
        stats.ammoCurrent = stats.ammoMax;
    }

    private void Awake()
    {
        gunParticles = GetComponentsInChildren<ParticleSystem>();
        gunLights = GetComponentsInChildren<Light>();
        shellEject = transform.Find("ShellEject");
    }

    public void DefaultPosition(Unit parent)
    {
        // Used to place newly-created weapon objects into the default position

        transform.parent = parent.GetAnimator().GetWeaponDefaultPosition();
        transform.position = transform.parent.position;
        transform.localPosition = transform.localPosition + offset;

        if (attributes.weaponType == WeaponType.Gun)
            transform.rotation = transform.parent.transform.rotation;
    }

    public void Shoot()
    {
        // Used by animation to kick off shoot effect

        StartCoroutine(ShootEffect());
    }

    public int GetAnimationLayer()
    { return attributes.animationLayer; }

    public WeaponImpact GetImpact()
    { return attributes.weaponImpact; }

    public int GetDamage()
    { return stats.damage; }

    public float GetAreaOfEffect()
    { return stats.areaOfEffect; }

    public int GetMinimumRange()
    { return stats.minRangeNoPenalty; }

    public int GetMaximumRange()
    { return stats.maxRangeNoPenalty; }

    public float GetAccuracyPenalty(int range)
    {
        int maxRangeDiff = range - GetMaximumRange();
        int minRangeDiff = -(range - GetMinimumRange());

        float penalty = 0;

        if (maxRangeDiff > 0) 
        {
            penalty = maxRangeDiff * stats.overRangeAccuracyPenalty;
        }
        else if (minRangeDiff > 0)
        {
            penalty = minRangeDiff * stats.underRangeAccuracyPenalty;
        }

        return penalty;
    }

    public IEnumerator ShootEffect()
    {
        // Display gun flash
        foreach (Light gunLight in gunLights)
            gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        foreach (ParticleSystem gunParticle in gunParticles)
            gunParticle.Stop();

        foreach (ParticleSystem gunParticle in gunParticles)
            gunParticle.Play();

        PlaySound(WeaponSound.FIRE);

        if (shellEject.gameObject.activeSelf) EjectShell();

        yield return new WaitForSecondsRealtime(gunParticles[0].main.duration);
        foreach (Light gunLight in gunLights)
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
        }

        // Play the sound at the desired audio source
        audioSource.PlayOneShot(audioClip);
    }

    void EjectShell()
    {
        // Generate a shell as an effect and eject it with force

        float randomForce = (Random.Range(3, 5));
        GameObject shell = GlobalManager.ActiveMap.CreateEffect(shellPrefab, shellEject.position, Quaternion.Euler(Random.Range(5, 15), 0, Random.Range(-10, -15)));
        shell.GetComponent<Rigidbody>().AddForce(shellEject.forward * randomForce, ForceMode.VelocityChange);
        shell.GetComponent<Rigidbody>().AddForce(-shellEject.right * randomForce/2, ForceMode.VelocityChange);

    }

    public void SpendAmmo(int amount = 1)
    {
        // Spends a shot from the current ammo, defaults to 1 shot

        stats.ammoCurrent -= amount;
    }
}

public enum WeaponType { Gun, Melee, Shield }

public enum WeaponFamily { MELEE, PISTOL, SMG, SHOTGUN, RIFLE, AR, LMG, SHIELD, LAUNCHER }

public enum WeaponImpact { LIGHT, MEDIUM, HEAVY }

public enum WeaponSound { FIRE, RELOAD };