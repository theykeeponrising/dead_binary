using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    // Manifest of prefab assets for use in function calls
    
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
}