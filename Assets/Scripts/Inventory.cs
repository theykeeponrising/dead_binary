using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    Character character;
    public List<Weapon> weapons;
    public List<Item> items;

    public Weapon equippedWeapon;

    // TO DO -- Add armor

    [SerializeField]
    [Range(0, 10)]
    int weaponsMax = 2; // If we eventually want to allow some characters to equip more weapons

    // [SerializeField]
    // [Range(0, 10)]
    // int itemsMax = 2;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<Character>();

        // Init starting weapons from inspector
        for (int index = 0; index < weapons.Count; index++)
        {
            weapons[index] = Instantiate(weapons[index]);
            weapons[index].DefaultPosition(character);
            weapons[index].gameObject.SetActive(false);
        }

        // Equip first slotted weapon
        equippedWeapon = weapons[0];
        equippedWeapon.gameObject.SetActive(true);
        character.GetComponent<Animator>().SetLayerWeight(equippedWeapon.weaponLayer, 1);
        character.GetComponent<Animator>().SetFloat("animSpeed", equippedWeapon.attributes.animSpeed);
    }

    public bool SpawnWeapon(Weapon weapon)
    {
        // Instantiates weapon and adds it to the weapon list
        // Returns true/false if weapon successfully added

        if (weapons.Count >= weaponsMax)
        {
            Debug.Log("Too many weapons!");
            return false;
        }

        weapon = Instantiate(weapon);
        weapons.Add(weapon);
        weapon.DefaultPosition(character);
        weapon.gameObject.SetActive(false);

        if (!equippedWeapon && weapons.IndexOf(weapon) == 0)
        {
            equippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            character.GetComponent<Animator>().SetLayerWeight(weapon.weaponLayer, 1);
            character.GetComponent<Animator>().SetFloat("animSpeed", weapon.attributes.animSpeed);
        }
        return true;
    }

    public bool PickupWeapon(Weapon weapon)
    {
        // Pickups weapon and adds it to the weapon list
        // Returns true/false if weapon successfully added

        if (weapons.Count >= weaponsMax)
        {
            Debug.Log("Too many weapons!");
            return false;
        }

        weapons.Add(weapon);
        weapon.DefaultPosition(character);
        weapon.gameObject.SetActive(false);

        if (!equippedWeapon && weapons.IndexOf(weapon) == 0)
        {
            equippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            character.GetComponent<Animator>().SetLayerWeight(weapon.weaponLayer, 1);
            character.GetComponent<Animator>().SetFloat("animSpeed", weapon.attributes.animSpeed);
        }
        return true;
    }

    public Weapon CycleWeapon()
    {
        // Returns the next weapon from the weapons list

        int index = weapons.IndexOf(equippedWeapon) + 1;

        // Loop back to first item if at end of list
        if (index >= weapons.Count)
            index = 0;

        return weapons[index];
    }
}
