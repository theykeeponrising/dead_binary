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
    public WeaponImpact itemImpact;
    [Tooltip("Item prop displayed when item is used.")]
    public ItemProp itemProp;
    [Tooltip("Effect shown when item is triggered.")]
    public ParticleSystem itemEffect;

    protected Unit sourceUnit;
    protected Unit targetedUnit;
    protected Vector3 targetPosition;
    protected Vector3 triggerPosition;

    public override void TriggerItem()
    {
        // Callback function for props, initiates the item effect

        if (triggerPosition == Vector3.zero)
            triggerPosition = transform.position;

        if (itemType == ItemType.CONSUMABLE) itemUsesCurrent -= 1;

        CreateItemEffect();
        
        // Use on unit if possible, otherwise on empty tile
        Tile targetedTile = targetedUnit ? targetedUnit.Tile : Map.MapGrid.GetTile(targetPosition);

        foreach (Unit unit in Tile.GetTileOccupants(Tile.GetAreaOfEffect(targetedTile, areaOfEffect)))
        {
            if (!immuneUnitTypes.Contains(unit.attributes.unitType)) 
                ItemEffect(sourceUnit, unit);
        }

        itemAction.EndPerformance();
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        triggerPosition = setTriggerPosition;
        TriggerItem();
    }

    public override void TriggerItem(Unit triggerTarget)
    {
        triggerPosition = triggerTarget.transform.position;
        TriggerItem();
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
            targetedUnit.TakeDamage(sourceUnit, Mathf.Abs(hpAmount), triggerPosition, MessageType.DMG_EXPLOSIVE);
            targetedUnit.TakeDamageEffect(item: this);
        }
    }

    void CreateItemEffect()
    {
        // Creates the item effect object at the trigger position

        if (!itemEffect) 
            return;

        GameObject spawnEffect = GlobalManager.ActiveMap.CreateTimedEffect(itemEffect.gameObject, triggerPosition, itemEffect.transform.rotation, 3f);
        spawnEffect.transform.localScale = Vector3.one * (areaOfEffect / 2);
        PlayItemSFX(spawnEffect);
    }

    void PlayItemSFX(GameObject itemEffect)
    {
        // Plays the item effect sound (if any)

        if (itemSFX == ItemEffectType.NONE)
            return;

        AudioSource audioSource = itemEffect.GetComponent<AudioSource>();
        AudioClip audioClip = AudioManager.GetSound(itemSFX);
        audioSource.PlayOneShot(audioClip);
    }
}
