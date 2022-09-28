using UnityEngine;
using System;

public abstract class Item : MonoBehaviour
{
    public ItemType itemType;
    public TargetType targetType;
    public TargetFaction targetFaction;

    public string itemName;
    public ActionList itemAction;
    public int itemUsesCurrent;
    public int itemUsesMax;

    public bool CheckAffinity(Unit sourceUnit, Unit targetedUnit)
    {
        // Compares target factions to expected targets

        Faction sourceFaction = sourceUnit.attributes.faction;
        Faction targetedFaction = targetedUnit.attributes.faction;

        switch (targetFaction) {
            case TargetFaction.FRIENDLY:
                if (sourceFaction == targetedFaction) return true;
                break;
            case TargetFaction.ENEMY:
                if (sourceFaction == Faction.Good && targetedFaction == Faction.Bad) return true;
                if (sourceFaction == Faction.Bad && targetedFaction == Faction.Good) return true;
                break;
            case TargetFaction.NEUTRAL:
                if (targetedFaction == Faction.Neutral) return true;
                break;
        }
        return false;
    }

    public virtual void UseItem()
    {
        Debug.Log("No item use found for item! (No parameters)");
    }

    public virtual void UseItem(Unit target)
    {
        Debug.Log("No item use found for item! (Target parameter)");
    }

    public virtual void UseItem(Unit sourceUnit, Unit targetedUnit)
    {
        Debug.Log("No item use found for item! (Source and target parameters)");
    }

    public virtual void TriggerItem()
    {
        Debug.Log("No item trigger found for item!");
    }

    public virtual void TriggerItem(Vector3 triggerPosition)
    {
        Debug.Log("No item trigger found for item! (Trigger position parameter)");
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
public enum TargetType { CHARACTER, COVER, WEAPON }
public enum TargetFaction { FRIENDLY, ENEMY, NEUTRAL, ANY, NONE }
// ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
// EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
// CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
// QUEST - Not usable ... ex supplies, intel, macguffin