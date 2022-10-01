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
    public ActionRequirement[] actionRequirements;
    public UnitActionEnum actionSprite;

    // Gamestate values
    [HideInInspector] public FiniteState<InCombatPlayerAction> currentState;
    [HideInInspector] public Unit unit;
    [HideInInspector] public Item item;
    [HideInInspector] public int actionStage = 0;
    bool actionPerforming;
    public float bufferStart;
    public float bufferEnd;

    public bool Performing() => actionPerforming;

    private void Awake()
    {
        unit = GetComponentInParent<Unit>();
        item = GetComponentInParent<Item>();
    }

    private void LateUpdate()
    {
        if (Performing()) CheckAction();
    }

    public bool HasSprite()
    {
        // Used to check if action should show up on the action bar
        // Returns false if actionSprite is the NONE choice

        return actionSprite != UnitActionEnum.NONE;
    }

    public bool CheckRequirements(bool printDebug = false)
    {
        // Checks each requirement item in list
        // If any requirement fails, returns false, otherwise return true
        if (actionRequirements.Length > 0)
            foreach (ActionRequirement requirement in actionRequirements)
            {
                // Check that unit meets AP cost
                if (requirement == ActionRequirement.AP)
                    if (!unit || unit.stats.actionPointsCurrent < actionCost)
                    {
                        if (printDebug) Debug.Log("failed requirement ap");
                        return false;
                    }

                // Check that unit meets Ammo cost
                if (requirement == ActionRequirement.AMMO)
                    if (!unit || unit.inventory.equippedWeapon.stats.ammoCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement ammo");
                        return false;
                    }

                // Check that unit's ammo isn't full
                if (requirement == ActionRequirement.RELOAD)
                    if (!unit || unit.inventory.equippedWeapon.stats.ammoCurrent >= unit.inventory.equippedWeapon.stats.ammoMax)
                    {
                        if (printDebug) Debug.Log("failed requirement reload");
                        return false;
                    }

                if (requirement == ActionRequirement.QUANTITY)
                    if (item.itemUsesCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement quantity");
                        return false;
                    }
            }

        return true;
    }

    public void StartPerformance()
    {
        // Starts action's performing flag

        actionPerforming = true;
    }

    public void StartPerformance(string animation)
    {
        // Starts action's performing flag with an animation

        actionPerforming = true;
        unit.GetAnimator().Play(animation);
    }

    public void EndPerformance()
    {
        // Ends action's performing flag
        // Returns target to cover if applicable

        actionPerforming = false;
        actionStage = 0;
        unit.GetAnimator().CoverCrouch();
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
