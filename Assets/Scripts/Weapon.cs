using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum WeaponType { Gun, Melee, Shield }
    public WeaponType weaponType;

    public enum WeaponHeld { Onehand, Offhand, Twohand }
    public WeaponHeld weaponHeld;
    public int weaponLayer;

    public enum WeaponFamily { MELEE, PISTOL, SMG, SHOTGUN, RIFLE, AR, LMG, SHIELD, LAUNCHER }
    public WeaponFamily weaponFamily;

    AudioSource audioSource;
    public AudioClip[] fireSound;
    public AudioClip reloadSound;
    public AudioClip noAmmoSound;

    GameObject parentObj;
    Transform handAttach;
    public Vector3 offset;

    ParticleSystem gunParticles;
    Light gunLight;
    
    [System.Serializable]
    public class Stats
    {
        public int damage;
        public int range;
    }
    public Stats stats;

    private void Awake()
    {
        if (weaponHeld == WeaponHeld.Onehand)
            weaponLayer = 1;
        else if (weaponHeld == WeaponHeld.Twohand)
            weaponLayer = 2;
    }

    public void DefaultPosition(Character parent)
    {
        parentObj = parent.gameObject;
        handAttach = parent.body.handRight;
        audioSource = GetComponent<AudioSource>();
        gunParticles = GetComponentInChildren<ParticleSystem>();
        gunLight = GetComponentInChildren<Light>();

        //CREATE WEAPON AND ATTACH TO APPROPRIATE PARENT HAND
        //int cnt = (!secondary) ? 0 : 1;
        transform.parent = handAttach;

        transform.position = transform.parent.position;
        transform.localPosition = transform.localPosition + offset;

        if (weaponType == WeaponType.Gun)
        {
            transform.rotation = transform.parent.transform.rotation;

            //if (!isPlayer) return;
            //reloadSlider = parentObj.GetComponent<Character>().reloadSlider[cnt];
            //floatingText = parentObj.GetComponent<Character>().floatingText.GetComponent<Text>();
        }
        //if (weaponType == WeaponType.Melee)
        //{
        //    transform.rotation = transform.parent.transform.rotation;
        //    if (altWeaponType == 0)
        //        transform.Rotate(new Vector3(0, 0, 0));
        //    else
        //        transform.Rotate(new Vector3(-90, 0, 0));
        //}
        //if (weaponType == WeaponType.Shield)
        //{
        //    transform.rotation = transform.parent.transform.rotation;
        //    transform.Rotate(new Vector3(0, -90, 0));
        //}
    }

    public void Shoot()
    {
        StartCoroutine(ShootEffect());
    }

    public IEnumerator ShootEffect()
    {
        //DISPLAY GUN FLASH
        gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        gunParticles.Stop();
        gunParticles.Play();
        AudioClip audioClip = fireSound[Random.Range(0, fireSound.Length)];
        audioSource.PlayOneShot(audioClip);
        yield return new WaitForSecondsRealtime(gunParticles.main.duration);
        gunLight.enabled = false;
    }

    public void Reload()
    {
        audioSource.PlayOneShot(reloadSound);
    }
}
