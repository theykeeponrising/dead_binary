using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    Character character;
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
        [Tooltip("Does the item target Characters, or something else?")]
        [ReadOnly] public TargetType targetType;
        [Tooltip("The icon associated with this item.")]
        [ReadOnly] public Sprite icon;
        [Tooltip("Is the item able to be used more than once per match?")]
        [ReadOnly] public bool isReusable;
        [Tooltip("How many turns to wait before being available again.")]
        [ReadOnly] public int cooldownMax;
        [Tooltip("Does the item target allies, enemies, or combination?")]
        [ReadOnly] public Affinity affinity;
    }
    
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
        character.animator.SetLayerWeight(equippedWeapon.weaponLayer, 1);


        InitializeItems();
    }

    private void Update()
    {
        for (int i = 0; i < itemStats.Count; i++)
        {
            itemStats[i].currentCooldown = items[0].CurrentCooldown;
        }
    }

    public Item GetItem(int index)
    {
        if (itemStats.Count > 0)
        {
            if(items[index])
            return items[index];
            else
            {
                Debug.Log("No item in that slot! (Is this an error?)");
                return null;
            }    
        }
        else
        {
            Debug.Log("No items in inventory!");
            return null;
        }
    }

    private void InitializeItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            ItemStats s = new ItemStats();
            s.itemName = items[i].Name;
            s.itemType = items[i].ItemType;
            s.itemCost = items[i].ItemCost;
            s.targetType = items[i].TargetType;
            s.icon = items[i].Icon;
            s.isReusable = items[i].IsReusable;
            s.cooldownMax = items[i].CooldownMax;
            s.affinity = items[i].Affinity;
            s.currentCooldown = items[i].CurrentCooldown;
            itemStats.Add(s);
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
