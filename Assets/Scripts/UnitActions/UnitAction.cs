using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitAction : MonoBehaviour
{
    // Action attributes
    public string actionName;
    public string actionDescription;
    public int actionCost;

    public int actionCooldown;
    public int actionCooldownRemaining;
    public bool OnCooldown() => actionCooldownRemaining > 0;

    public UnitActionRequirement[] actionRequirements;
    public UnitActionEnum actionSprite;
    public bool HasSprite() => actionSprite != UnitActionEnum.NONE;

    // Gamestate values
    [HideInInspector] public FiniteState<InCombatPlayerAction> currentState;
    [HideInInspector] public Unit unit;
    [HideInInspector] public Item item;
    [HideInInspector] public int actionStage = 0;

    public float bufferStart;
    public float bufferEnd;

    bool actionPerforming;
    bool actionPerformed;
    public bool Performing() => actionPerforming;
    public bool Performed() => actionPerformed;

    private void Awake()
    {
        unit = GetComponentInParent<Unit>();
        item = GetComponentInParent<Item>();
    }

    private void LateUpdate()
    {
        if (Performing()) CheckAction();
    }

    public bool IsType(System.Type type)
    {
        return GetType() == type;
    }

    public void OnTurnStart()
    {
        // Called at the start of the unit's turn
        // Reduce cooldown (if any) and reset the performed flag

        if (OnCooldown()) actionCooldownRemaining -= 1;
        actionPerformed = false;
    }

    public bool CheckRequirements(bool printDebug = false)
    {
        // Checks each requirement item in list
        // If any requirement fails, returns false, otherwise return true
        if (actionRequirements.Length > 0)
            foreach (UnitActionRequirement requirement in actionRequirements)
            {
                // Check that unit meets AP cost
                if (requirement == UnitActionRequirement.AP)
                    if (!unit || unit.Stats.actionPointsCurrent < actionCost)
                    {
                        if (printDebug) Debug.Log("failed requirement ap");
                        return false;
                    }

                // Check that unit meets Ammo cost
                if (requirement == UnitActionRequirement.AMMO)
                    if (!unit || unit.EquippedWeapon.Stats.AmmoCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement ammo");
                        return false;
                    }

                // Check that unit's ammo isn't full
                if (requirement == UnitActionRequirement.RELOAD)
                    if (!unit || unit.EquippedWeapon.Stats.AmmoCurrent >= unit.EquippedWeapon.Stats.AmmoMax)
                    {
                        if (printDebug) Debug.Log("failed requirement reload");
                        return false;
                    }

                if (requirement == UnitActionRequirement.QUANTITY)
                    if (item.itemUsesCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement quantity");
                        return false;
                    }
            }

        return true;
    }

    public void SetPerformed(bool performed)
    {
        actionPerformed = performed;
    }

    public void StartPerformance()
    {
        // Starts action's performing flag

        actionPerforming = true;
        SetPerformed(true);
    }

    public void StartPerformance(string animation)
    {
        // Starts action's performing flag with an animation

        actionPerforming = true;
        SetPerformed(true);
        unit.PlayAnimation(animation);
    }

    public void EndPerformance()
    {
        // Ends action's performing flag
        // Returns target to cover if applicable

        actionPerforming = false;
        actionStage = 0;
        unit.CoverCrouch();
        unit.PlayerAction.CheckTurnEnd();
    }

    public void NextStage()
    {
        // Progresses action to the next stage

        actionStage += 1;
    }

    public bool ActionStage(int stageCheck)
    {
        // Quick check if current action stage is value

        return actionStage == stageCheck;
    }

    public virtual void UseAction()
    {
        Debug.Log(string.Format("No action use found for {0}", actionName));
    }

    public virtual void UseAction(Unit unit)
    {
        Debug.Log(string.Format("No action use found for {0} (unit)", actionName));
    }

    public virtual void UseAction(Vector3 position)
    {
        Debug.Log(string.Format("No action use found for {0} (vector3)", actionName));
    }

    public virtual void UseAction(Tile tile)
    {
        Debug.Log(string.Format("No action use found for {0} (tile)", actionName));
    }

    public virtual void UseAction(Tile tile, List<Tile> path)
    {
        Debug.Log(string.Format("No action use found for {0} (tile, path)", actionName));
    }

    public virtual void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        Debug.Log(string.Format("No action use found for {0} (finite state)", actionName));
    }

    public virtual void UseAction(FiniteState<InCombatPlayerAction> setState, Item item)
    {
        Debug.Log(string.Format("No action use found for {0} (finite state, item)", actionName));
    }

    public virtual void CheckAction()
    {
        Debug.Log(string.Format("No action check found for {0}", actionName));
    }
}

public enum UnitActionRequirement { NONE, AP, AMMO, RELOAD, QUANTITY };