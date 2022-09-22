using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DamageItem : Item
{
    // Base item class for all items that change HP amount.
    // Healing items will use a negative amount.

    [Tooltip("HP amount change from use. Positive amounts will heal.")]
    public int hpAmount;
    [Tooltip("Circular effect area in tiles.")]
    public float areaOfEffect;
    [Tooltip("Usable tile range for the item.")]
    public int range;
    [Tooltip("Impact effect of the item.")]
    public Weapon.WeaponImpact itemImpact;
    [Tooltip("Item prop displayed when item is used.")]
    public ItemProp itemProp;

    [HideInInspector] public Unit sourceUnit;
    [HideInInspector] public Unit targetedUnit;
    [HideInInspector] public Vector3 triggerPosition;

    public override void UseItem(Unit newSourceUnit, Unit newTargetedUnit)
    {
        (sourceUnit, targetedUnit) = (newSourceUnit, newTargetedUnit);

        if (itemType == ItemType.CONSUMABLE) itemUsesCurrent -= 1;
        Debug.Log(triggerPosition);

        foreach (Unit unit in GetImpactedUnits(AreaOfEffect()))
            ItemEffect(sourceUnit, unit);
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        triggerPosition = setTriggerPosition;
        UseItem(sourceUnit, targetedUnit);
    }

    public override void ItemEffect(Unit sourceUnit, Unit targetedUnit)
    {
        // Triggers item effect
        // Positive HP change is considered healing
        // Negative HP change is considered damage

        if (hpAmount >= 0)
        {
            Debug.Log(string.Format("Healed {0} for {1} health!", targetedUnit.attributes.name, Mathf.Abs(hpAmount)));
            targetedUnit.RestoreHealth(Mathf.Abs(hpAmount));
        }
        else
        {
            Debug.Log(string.Format("Damaged {0} for {1} health!", targetedUnit.attributes.name, Mathf.Abs(hpAmount)));
            targetedUnit.TakeDamage(sourceUnit, Mathf.Abs(hpAmount), triggerPosition);
            targetedUnit.GetAnimator().TakeDamageEffect(item: this);
        }
    }

    List<Tile> AreaOfEffect()
    {
        // Gets affected tiles from target position based on "areaOfEffect" stat
        // Every odd number of range adds horizontal and vertical neighbor tiles
        // Every even number of range adds diagonal neighbor tiles

        Tile[] tiles = GameObject.FindObjectsOfType<Tile>();
        List<Tile> impactedTiles = new List<Tile>();

        impactedTiles.Add(targetedUnit.currentTile);
        
        foreach (Tile tile in tiles)
        {
            float distance = Vector3.Distance(targetedUnit.transform.position, tile.gameObject.transform.position);
            if (distance <= areaOfEffect && !impactedTiles.Contains(tile)) impactedTiles.Add(tile);
        }

        return impactedTiles;
    }

    List<Unit> GetImpactedUnits(List<Tile> areaOfEffect)
    {
        List<Unit> impactedUnits = new List<Unit>();

        foreach (Tile tile in areaOfEffect)
        {
            if (!tile.occupant) continue;
            if (!tile.occupant.GetComponent<Unit>()) continue;
            impactedUnits.Add(tile.occupant.GetComponent<Unit>());
        }

        return impactedUnits;
    }
}
