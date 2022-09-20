using UnityEngine;
using System;

public abstract class Item : MonoBehaviour
{
    public ItemType itemType;
    public TargetType targetType;
    public TargetFaction targetFaction;

    public string itemName;
    public ActionList itemAction;

    public bool CheckAffinity(Faction ownFac, Faction oppFac)
    {
        // Compares target factions to expected targets

        switch (targetFaction) {
            case TargetFaction.FRIENDLY:
                if (oppFac == ownFac) return true;
                break;
            case TargetFaction.ENEMY:
                if (ownFac == Faction.Good && oppFac == Faction.Bad) return true;
                if (ownFac == Faction.Bad && oppFac == Faction.Good) return true;
                break;
            case TargetFaction.NEUTRAL:
                if (oppFac == Faction.Neutral) return true;
                break;
        }
        return false;
    }

    public virtual void UseItem()
    {
        Debug.Log("No item use found for item!");
    }

    public virtual void UseItem(Unit target)
    {
        Debug.Log("No item use found for item!");
    }
}

public enum ItemType { ATTACHMENT, EQUIPMENT, CONSUMABLE, QUEST }
public enum TargetType { CHARACTER, COVER, WEAPON }
public enum TargetFaction { FRIENDLY, ENEMY, NEUTRAL, ANY, NONE }
// ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
// EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
// CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
// QUEST - Not usable ... ex supplies, intel, macguffin
