using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    // Manifest of prefab assets for use in function calls
    public static AssetManager Instance = null;

    void Start()
    {
        Instance = this;
    }

    public Transform mapObjects; // TO DO -- Map handler script

    [System.Serializable]
    public class WeaponPrefabs
    {
        public Weapon noWeapon;
        public Weapon fist;
        public Weapon fistBig;
        public Weapon pipe;
        public Weapon pistol;
        public Weapon autoPistol;
        public Weapon sawedOff;
        public Weapon smg;
        public Weapon shotgun;
        public Weapon ar;
        public Weapon rifle;
        public Weapon carbine;
        public Weapon revolver;
        public Weapon lmg;
        public Weapon wasp;
        public Weapon amigo;
        public Weapon volt;
        public Weapon riotShield;
        public Weapon grenadeLauncher;
    }
    public WeaponPrefabs weapon;

    [System.Serializable]
    public class CoverPrefabs
    {
        public CoverObject halfWall;
        public CoverObject fullWall;
        public CoverObject concreteBarrier1;
        public CoverObject concreteBarrier2;
        public CoverObject metalRail;
        public CoverObject metalRailLarge;
    }
    public CoverPrefabs cover;
}
