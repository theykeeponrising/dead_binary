using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    Unit unit;
    public List<Weapon> weapons;
    [SerializeField] public List<Item> items;
    [SerializeField] private List<ItemStats> itemStats;

    [System.Serializable] public class ItemStats
    {
        [Tooltip("How many turns remain until the item can be used again.")]
        [ReadOnly] public int currentCooldown;
        [Tooltip("The name of the item.")]
        [ReadOnly] public string itemName;
        [Tooltip("The type of item. This is derived from the Class itself.")]
        [ReadOnly] public ItemType itemType;
        [Tooltip("The AP required to use this item.")]
        [ReadOnly] public int itemCost;
        [Tooltip("The amount of uses before depletion.")]
        [ReadOnly] public int itemCharges;
        [Tooltip("Does the item target Characters, or something else?")]
        [ReadOnly] public TargetType targetType;
        [Tooltip("The icon associated with this item.")]
        [ReadOnly] public Sprite icon;
        [Tooltip("Is the item able to be used more than once per match?")]
        [ReadOnly] public bool isReusable;
        [Tooltip("How many turns to wait before being available again.")]
        [ReadOnly] public int cooldownMax;
    }
    
    public Weapon equippedWeapon;

    // TO DO -- Add armor

    [SerializeField]
    [Range(0, 10)]
    int weaponsMax = 2; // If we eventually want to allow some characters to equip more weapons

    public void Init(Unit unit)
    {
        this.unit = unit;

        // Init starting weapons from inspector
        for (int index = 0; index < weapons.Count; index++)
        {
            weapons[index] = Instantiate(weapons[index]);
            weapons[index].DefaultPosition(unit);
            weapons[index].gameObject.SetActive(false);
        }

        // Equip first slotted weapon
        equippedWeapon = weapons[0];
        equippedWeapon.gameObject.SetActive(true);
        unit.GetComponent<Animator>().SetLayerWeight(equippedWeapon.GetAnimationLayer(), 1);
        unit.GetComponent<Animator>().SetFloat("animSpeed", equippedWeapon.attributes.animationSpeed);

        // Init starting items from inspector
        for (int index = 0; index < items.Count; index++)
        {
            items[index] = Instantiate(items[index], transform);
        }
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
        weapon.DefaultPosition(unit);
        weapon.gameObject.SetActive(false);

        if (!equippedWeapon && weapons.IndexOf(weapon) == 0)
        {
            equippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            unit.GetComponent<Animator>().SetLayerWeight(weapon.GetAnimationLayer(), 1);
            unit.GetComponent<Animator>().SetFloat("animSpeed", weapon.attributes.animationSpeed);
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
        weapon.DefaultPosition(unit);
        weapon.gameObject.SetActive(false);

        if (!equippedWeapon && weapons.IndexOf(weapon) == 0)
        {
            equippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            unit.GetComponent<Animator>().SetLayerWeight(weapon.GetAnimationLayer(), 1);
            unit.GetComponent<Animator>().SetFloat("animSpeed", weapon.attributes.animationSpeed);
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

    public void RemoveChargeFromItem(int index, int amount)
    {
        itemStats[index].itemCharges -= amount;

        if (itemStats[index].itemCharges <= 0)
        {
            items.Remove(items[index]);
            itemStats.Remove(itemStats[index]);
        }
    }
}
