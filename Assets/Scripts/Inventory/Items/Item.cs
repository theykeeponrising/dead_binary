using UnityEngine;
using System.Collections.Generic;

public abstract class Item : MonoBehaviour
{
    protected Unit unit;
    public ItemType itemType;
    public TargetType targetType;
    public FactionAffinity targetFaction;
    public List<UnitType> immuneUnitTypes;

    public UnitAction itemAction;
    public int itemUsesCurrent;
    public int itemUsesMax;

    public string itemInfoName;
    [Tooltip("Action displayed on the Info Panel action button.")]
    public string itemInfoDescription;
    [Tooltip("Description displayed on the Info Panel action button.")]

    public float areaOfEffect;
    [Tooltip("Usable tile range for the item.")]
    public int range;
    [Tooltip("Impact effect of the item.")]

    [SerializeField] protected ItemEffectType itemSFX;

    public bool CheckRequirements() => itemAction.CheckRequirements();

    private void Awake()
    {
        unit = GetComponentInParent<Unit>();

        if (itemAction)
        {
            itemAction = Instantiate(itemAction, transform);
            if (itemInfoName != "") itemAction.SetName(itemInfoName);
            if (itemInfoDescription != "") itemAction.SetDescription(itemInfoDescription);
        }
    }

    public virtual void UseItem()
    {
        Debug.Log("No item use found for item! (No parameters)");
    }

    public virtual void UseItem(Unit target)
    {
        Debug.Log("No item use found for item! (Target parameter)");
    }

    public virtual void UseItem(Unit sourceUnit, Unit targetUnit)
    {
        Debug.Log("No item use found for item! (Source and target parameters)");
    }

    public virtual void UseItem(Unit sourceUnit, Vector3 setTargetPosition)
    {
        Debug.Log("No item use found for item! (Source and target position parameters)");
    }

    public virtual void TriggerItem()
    {
        Debug.Log("No item trigger found for item!");
    }

    public virtual void TriggerItem(Vector3 triggerPosition)
    {
        Debug.Log("No item trigger found for item! (Trigger position parameter)");
    }

    public virtual void TriggerItem(Unit triggerTarget)
    {
        Debug.Log("No item trigger found for item! (Trigger unit parameter)");
    }

    public virtual void ItemEffect()
    {
        Debug.Log("No item effect found for item!");
    }

    public virtual void ItemEffect(Unit sourceUnit, Unit targetedUnit)
    {
        Debug.Log("No item effect found for item! (Source and target parameters)");
    }

}

public enum ItemType { ATTACHMENT, EQUIPMENT, CONSUMABLE, QUEST }
public enum TargetType { CHARACTER, COVER, TILE }

// ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
// EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
// CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
// QUEST - Not usable ... ex supplies, intel, macguffin
