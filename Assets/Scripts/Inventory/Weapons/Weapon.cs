using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Main class used for weapon objects
    private Unit _unit;
    private Transform _barrelEnd;
    private Transform _shellEject;
    private ParticleSystem[] _gunParticles;
    private Light[] _gunLights;
    private Renderer[] _renderers;
    private readonly List<Material> _materials = new();

    private readonly Dictionary<WeaponMuzzleFlash, Color32> _muzzleFlashColors = new() 
    {
        { WeaponMuzzleFlash.CONVENTIONAL, new Color32(255, 255, 0, 255) },
        { WeaponMuzzleFlash.LASER_RED, new Color32(255, 0, 5, 255) },
        { WeaponMuzzleFlash.LASER_PINK, new Color32(255, 0, 255, 255) }
    };

    [SerializeField] private Vector3 _offset;
    [SerializeField] private bool _useFactionColors;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private Shell _shellPrefab;
    [SerializeField] private UnitActionShoot _weaponAction;
    [SerializeField] private AudioClip[] _fireSound;
    [SerializeField] private AudioClip[] _reloadSound;

    private Color FactionColor { get { return _unit.Attributes.Faction.FactionColor; } }
    
    public WeaponStats Stats;
    public WeaponAttributes Attributes;

    public UnitAction WeaponAction { get { return _weaponAction; } }

    private void Awake()
    {
        _gunParticles = GetComponentsInChildren<ParticleSystem>();
        _barrelEnd = _gunParticles[0].transform;
        _shellEject = transform.Find("ShellEject");
    }

    private void Start()
    {
        Reload();
        SetFactionColors();
        SetMuzzleFlash();
    }

    private void SetFactionColors()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in _renderers)
            _materials.Add(renderer.material);

        if (!_useFactionColors)
            return;

        foreach (Material material in _materials)
            material.SetColor("_Emissive", FactionColor * 6f);
    }

    private void SetMuzzleFlash()
    {
        _gunLights = GetComponentsInChildren<Light>();

        foreach (Light light in _gunLights)
            light.color = (Color)_muzzleFlashColors[Attributes.MuzzleFlash];
    }

    public void SetParent(Unit unit)
    {
        // Used to place newly-created weapon objects into the default position

        _unit = unit;

        transform.parent = _unit.GetAttachPoint(Attributes.AttachPoint);
        transform.position = transform.parent.position;
        transform.localPosition = transform.localPosition + _offset;
        transform.rotation = transform.parent.transform.rotation;
    }

    public void Shoot()
    {
        // Used by animation to kick off shoot effect

        StartCoroutine(MuzzleFlash());
        PlaySound(WeaponSound.FIRE);
        SpawnProjectile();
        EjectShell();
    }

    public int GetAnimationLayer()
    { return Attributes.AnimationLayer; }

    public WeaponImpact GetImpact()
    { return Attributes.Impact; }

    public int GetDamage()
    { return Stats.Damage; }

    public float GetAreaOfEffect()
    { return Stats.AreaOfEffect; }

    public int GetMinimumRange()
    { return Stats.RangeMin; }

    public int GetMaximumRange()
    { return Stats.RangeMax; }

    public float GetAccuracyPenalty(int range)
    {
        int maxRangeDiff = range - GetMaximumRange();
        int minRangeDiff = -(range - GetMinimumRange());

        float penalty = 0;

        if (maxRangeDiff > 0) 
        {
            penalty = maxRangeDiff * Stats.OverRangeAccuracyPenalty;
        }
        else if (minRangeDiff > 0)
        {
            penalty = minRangeDiff * Stats.UnderRangeAccuracyPenalty;
        }

        return penalty;
    }

    private void SpawnProjectile()
    {
        // Spawns projectile and sets its trajectory

        System.Type actionType = _weaponAction.GetType();
        UnitActionShoot action = (UnitActionShoot)_unit.FindActionOfType(actionType);
        action.SpawnProjectile(_projectilePrefab, _barrelEnd, Attributes.ProjectileSpeed);
    }

    public void Reload()
    {
        // Sets current ammo to weapon's maximum

        Stats.AmmoCurrent = Stats.AmmoMax;
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
                audioClip = _fireSound[Random.Range(0, _fireSound.Length)];
                break;
            case (WeaponSound.RELOAD):
                audioClip = _reloadSound[Random.Range(0, _reloadSound.Length)];
                break;
        }

        // Play the sound at the desired audio source
        audioSource.PlayOneShot(audioClip);
    }

    private void EjectShell()
    {
        // Generate a shell as an effect and eject it with force

        if (!_shellEject.gameObject.activeSelf)
            return;

        float randomForce = (Random.Range(3, 5));
        GameObject shell = GlobalManager.ActiveMap.CreateEffect(_shellPrefab.gameObject, _shellEject.position, Quaternion.Euler(Random.Range(5, 15), 0, Random.Range(-10, -15)));
        Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
        rigidbody.AddForce(_shellEject.forward * randomForce, ForceMode.VelocityChange);
        rigidbody.AddForce(-_shellEject.right * randomForce/2, ForceMode.VelocityChange);
    }

    private IEnumerator MuzzleFlash()
    {
        // Display gun flash
        foreach (Light gunLight in _gunLights)
            gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        foreach (ParticleSystem gunParticle in _gunParticles)
            gunParticle.Stop();

        foreach (ParticleSystem gunParticle in _gunParticles)
            gunParticle.Play();

        yield return new WaitForSecondsRealtime(_gunParticles[0].main.duration);
        foreach (Light gunLight in _gunLights)
            gunLight.enabled = false;
    }

    public void SpendAmmo(int amount = 1)
    {
        // Spends a shot from the current ammo, defaults to 1 shot

        Stats.AmmoCurrent -= amount;
    }
}

public enum WeaponImpact { LIGHT, MEDIUM, HEAVY }

public enum WeaponSound { FIRE, RELOAD };

public enum WeaponMuzzleFlash { CONVENTIONAL, LASER_RED, LASER_PINK };

public enum WeaponAttachPoint { NONE, HAND_RIGHT, HAND_LEFT }

[System.Serializable]
public struct WeaponAttributes
{
    public int AnimationLayer;
    public float AnimationSpeed;
    public float ProjectileSpeed;
    public WeaponImpact Impact;
    public WeaponMuzzleFlash MuzzleFlash;
    public WeaponAttachPoint AttachPoint;
}

[System.Serializable]
public struct WeaponStats
{
    public int Damage;
    public float AreaOfEffect;
    public int RangeMin;
    public int RangeMax;
    public int AmmoMax;
    public int AmmoCurrent;
    //Note: Not capped at 1.
    public float BaseAccuracyModifier;
    public float OverRangeAccuracyPenalty;
    public float UnderRangeAccuracyPenalty;
}
