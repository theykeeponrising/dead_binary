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
    public Weapon.WeaponImpact itemImpact;
    [Tooltip("Item prop displayed when item is used.")]
    public ItemProp itemProp;
    [Tooltip("Effect shown when item is triggered.")]
    public ParticleSystem itemEffect;

    [HideInInspector] public Unit sourceUnit;
    [HideInInspector] public Unit targetedUnit;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public Vector3 triggerPosition;

    public override void TriggerItem()
    {
        // Callback function for props, initiates the item effect

        triggerPosition = transform.position;
        if (itemType == ItemType.CONSUMABLE) itemUsesCurrent -= 1;

        if (itemEffect)
        {
            GameObject spawnEffect = GlobalManager.Instance.activeMap.CreateTimedEffect(itemEffect.gameObject, triggerPosition, itemEffect.transform.rotation, 3f);
            spawnEffect.transform.localScale = Vector3.one * (areaOfEffect / 2);
        }
        
        // Use on unit if possible, otherwise on empty tile
        Tile targetedTile = targetedUnit ? targetedUnit.currentTile : sourceUnit.grid.GetTile(targetPosition);

        foreach (Unit unit in Tile.GetTileOccupants(Tile.AreaOfEffect(targetedTile, areaOfEffect)))
            ItemEffect(sourceUnit, unit);

        itemAction.EndPerformance();
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        // Callback function for props, initiates the item effect

        triggerPosition = setTriggerPosition;
        if (itemType == ItemType.CONSUMABLE) itemUsesCurrent -= 1;

        if (itemEffect)
        {
            GameObject spawnEffect = GlobalManager.Instance.activeMap.CreateTimedEffect(itemEffect.gameObject, triggerPosition, itemEffect.transform.rotation, 3f);
            spawnEffect.transform.localScale = Vector3.one * (areaOfEffect / 2);
        }   

        foreach (Unit unit in Tile.GetTileOccupants(Tile.AreaOfEffect(sourceUnit.grid.GetTile(targetPosition), areaOfEffect)))
            ItemEffect(sourceUnit, unit);

        itemAction.EndPerformance();
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
}
