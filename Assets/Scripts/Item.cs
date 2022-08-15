using UnityEngine;
using System;

/*
 * USES CUSTOM EDITOR
 */

[Serializable]
public abstract class Item : MonoBehaviour
{
    // [Header("Item")]
    
    public Statics statics;
    [Serializable]
    public class Statics
    {
        [Tooltip("The name of the item.")]
        [SerializeField] public string itemName;
        [Tooltip("The type of item. This is derived from the Class itself.")]
        [SerializeField] public ItemType itemType;
        [Tooltip("The AP required to use this item.")]
        [SerializeField] public int itemCost;
        [Tooltip("Does the item target Characters, or something else?")]
        [SerializeField] public TargetType targetType;
        [Tooltip("The icon associated with this item.")]
        [SerializeField] public Sprite icon;
        [Tooltip("Is the item able to be used more than once per match?")]
        [SerializeField] public bool isReusable;
        [Tooltip("How many turns to wait before being available again.")]
        [SerializeField] public int cooldownMax;
        [Tooltip("Does the item target allies, enemies, or combination?")]
        [SerializeField] public Affinity affinity;
    }

    public string Name => statics.itemName;
    public ItemType ItemType => statics.itemType;
    public int ItemCost => statics.itemCost;
    public TargetType TargetType => statics.targetType;
    public Sprite Icon => statics.icon;
    public bool IsReusable => statics.isReusable;
    public int CooldownMax => statics.cooldownMax;
    public Affinity Affinity => statics.affinity;

    public int CurrentCooldown;

    public bool CheckAffinity(Faction ownFac, Faction oppFac)
    {
        bool b = false;

        if (Affinity.HasFlag(Affinity.Ally))
            if (oppFac == ownFac) b = true;
        if (Affinity.HasFlag(Affinity.Enemy))
        {
            if (ownFac == Faction.Good && oppFac == Faction.Bad) b = true;
            if (ownFac == Faction.Bad && oppFac == Faction.Good) b = true;
        }
        if(Affinity == Affinity.Neutral)
            if (oppFac == Faction.Neutral) b = true;

        return b;
    }

    public void TryUseItem(Character owner, GameObject go)
    {
        if (ItemType == ItemType.CONSUMABLE)
        {
            if (TargetType == TargetType.Character)
            {
                if (go.TryGetComponent(out Character c))
                {
                    if (CheckAffinity(owner.faction, c.faction))
                        UseItem(owner, c);
                    else
                        Debug.Log("Cannot use " + Name + " on '" + go.name + "'. \n" +
                            Name + " can only be used on " + Affinity);
                }
                else
                {
                    Debug.Log("Must target a character!");
                }
            }
            else
            {
                Debug.Log("Can only use Character-Targeted items for now. Change TargetType to Character.");
            }
        }
        else
        {
            Debug.Log("Invalid Item Type. Can only use CONSUMABLE.");
        }
    }
    protected abstract void UseItem(Character owner, Character charTarget = null, CoverObject covTarg = null);
}

[Flags]
public enum Affinity
{
    Ally = 1 << 0,
    Enemy = 1 << 1,
    Neutral = 1 << 2
}
public enum ItemType { ATTACHMENT, EQUIPMENT, CONSUMABLE, QUEST }
public enum TargetType { Character, Cover, Weapon }
// ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
// EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
// CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
// QUEST - Not usable ... ex supplies, intel, macguffin
