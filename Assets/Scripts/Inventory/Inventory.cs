using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Unit _unit;
    [SerializeField] private List<ItemStats> _itemStats;
    [SerializeField] [Range(0, 10)] private int _weaponsMax = 2;
    [SerializeField] private bool _useAltWeapon;

    public Weapon EquippedWeapon;
    public Weapon AltWeapon;
    public List<Weapon> Weapons;
    public List<Item> Items;

    // TO DO -- Add armor

    public void Init(Unit unit)
    {
        _unit = unit;

        // Init starting weapons from inspector
        for (int index = 0; index < Weapons.Count; index++)
        {
            Weapons[index] = Instantiate(Weapons[index]);
            Weapons[index].SetParent(unit);
            Weapons[index].gameObject.SetActive(false);
        }

        // Equip first slotted weapon
        EquippedWeapon = Weapons[0];
        EquippedWeapon.gameObject.SetActive(true);
        unit.GetComponent<Animator>().SetLayerWeight(EquippedWeapon.GetAnimationLayer(), 1);
        unit.GetComponent<Animator>().SetFloat("animSpeed", EquippedWeapon.Attributes.AnimationSpeed);

        // Alt weapons for drones
        if (_useAltWeapon)
        {
            AltWeapon = Weapons[1];
            AltWeapon.Attributes.AttachPoint = WeaponAttachPoint.HAND_LEFT;
            AltWeapon.SetParent(unit);
            AltWeapon.gameObject.SetActive(true);
        }

        // Init starting items from inspector
        for (int index = 0; index < Items.Count; index++)
        {
            Items[index] = Instantiate(Items[index], transform);
        }
    }

    public bool SpawnWeapon(Weapon weapon)
    {
        // Instantiates weapon and adds it to the weapon list
        // Returns true/false if weapon successfully added

        if (Weapons.Count >= _weaponsMax)
        {
            Debug.Log("Too many weapons!");
            return false;
        }

        weapon = Instantiate(weapon);
        Weapons.Add(weapon);
        weapon.SetParent(_unit);
        weapon.gameObject.SetActive(false);

        if (!EquippedWeapon && Weapons.IndexOf(weapon) == 0)
        {
            EquippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            _unit.GetComponent<Animator>().SetLayerWeight(weapon.GetAnimationLayer(), 1);
            _unit.GetComponent<Animator>().SetFloat("animSpeed", weapon.Attributes.AnimationSpeed);
        }
        return true;
    }

    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
        weapon.gameObject.SetActive(true);
        weapon.SetParent(_unit);
        _unit.SetAnimationLayerWeight(weapon.GetAnimationLayer(), 1);
        _unit.SetAnimationSpeed(weapon.Attributes.AnimationSpeed);
    }

    public bool PickupWeapon(Weapon weapon)
    {
        // Pickups weapon and adds it to the weapon list
        // Returns true/false if weapon successfully added

        if (Weapons.Count >= _weaponsMax)
        {
            Debug.Log("Too many weapons!");
            return false;
        }

        Weapons.Add(weapon);
        weapon.SetParent(_unit);
        weapon.gameObject.SetActive(false);

        if (!EquippedWeapon && Weapons.IndexOf(weapon) == 0)
        {
            EquippedWeapon = weapon;
            weapon.gameObject.SetActive(true);
            _unit.GetComponent<Animator>().SetLayerWeight(weapon.GetAnimationLayer(), 1);
            _unit.GetComponent<Animator>().SetFloat("animSpeed", weapon.Attributes.AnimationSpeed);
        }
        return true;
    }

    public Weapon CycleWeapon()
    {
        // Returns the next weapon from the weapons list

        int index = Weapons.IndexOf(EquippedWeapon) + 1;

        // Loop back to first item if at end of list
        if (index >= Weapons.Count)
            index = 0;

        return Weapons[index];
    }

    public void RemoveChargeFromItem(int index, int amount)
    {
        _itemStats[index].itemCharges -= amount;

        if (_itemStats[index].itemCharges <= 0)
        {
            Items.Remove(Items[index]);
            _itemStats.Remove(_itemStats[index]);
        }
    }
}

[System.Serializable]
public class ItemStats
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
