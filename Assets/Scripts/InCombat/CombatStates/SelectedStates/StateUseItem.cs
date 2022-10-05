using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateUseItem : StateTarget
{
    Item item;

    public StateUseItem(StateMachine<InCombatPlayerAction> machine, Item useItem) : base(machine, useItem.GetAffinity(useItem.unit)) { Machine = machine; item = useItem; }

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);
        t.PathPreviewClear();

        // Display info panel
        infoPanel.gameObject.SetActive(true);
        infoPanel.UpdateAction(item.itemAction);
        infoPanel.UpdateHit(-1);

        if (item.GetType().BaseType == typeof(DamageItem))
        {
            var itemType = (DamageItem)item;
            infoPanel.UpdateDamage(itemType.hpAmount);
        }

        //Find Targets
        switch (item.targetType)
        {
            case TargetType.CHARACTER:
                FindTargets<Unit>(t);
                break;
            default:
                Debug.Log("Types other than Character are not yet implemented");
                break;
        }
    }

    public override void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        if (typeof(TargetType) == typeof(Unit))
        {
            List<Unit> units = t.activeMap.FindUnits(item.GetAffinity(t.selectedCharacter));

            foreach (Unit unit in units)
                if (unit.stats.healthCurrent > 0 && ((DamageItem)item).isTargetInRange(t.selectedCharacter, unit) && !item.immuneUnitTypes.Contains(unit.attributes.unitType))
                    targets.Add(unit);
        }

        //Find closest Target
        if (targets.Count > 0)
        {
            targets.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
            });

            ChangeTarget(t, targets[0]);
        }
    }

    public override void ChangeTarget(InCombatPlayerAction t, Unit targetUnit)
    {
        target = targetUnit;
        t.selectedCharacter.GetActor().targetCharacter = target;
        infoPanel.CreateTargetButtons(targets);
        ShowSelectionCircle(target.transform.position);
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter == null) ChangeState(new StateNoSelection(Machine));

        foreach (var v in targets)
        {
            if (v.GetComponent<Unit>() == true)
            {
                Unit c = v.GetComponent<Unit>();

                if (v == target)
                    c.GetActor().IsTargetUX(true, true);
                else
                    c.GetActor().IsTargetUX(false, true);
            }
        }
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        if (!IsPointerOverUIElement(t))
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Directly targeting a valid unit with mouse primary
                if (targets.Contains(hit.collider.GetComponent<Unit>()))
                {
                    if (target != hit.collider.GetComponent<Unit>())
                    {
                        target = hit.collider.GetComponent<Unit>();
                        targetedTile = null;

                        t.selectedCharacter.GetActor().targetCharacter = target;
                        ShowSelectionCircle(target.transform.position);
                        infoPanel.CreateTargetButtons(targets);
                    }
                }

                // Directly targeting an invalid unit with mouse primary
                else if (hit.collider.gameObject.GetComponent<Unit>())
                {
                    Debug.Log("Not a target but don't want to revert to idle. Do nothing.");
                }

                // Directly targeting a tile with mouse primary
                else if (hit.collider.gameObject.GetComponent<Tile>())
                {                    
                    if (((DamageItem)item).isTargetInRange(t.selectedCharacter, hit.collider.gameObject.GetComponent<Tile>()))
                    {
                        target = null;
                        targetedTile = hit.collider.gameObject.GetComponent<Tile>();
                        t.selectedCharacter.GetActor().targetCharacter = null;
                        ShowSelectionCircle(targetedTile.transform.position);
                        infoPanel.CreateTargetButtons(targets);
                    }
                    else
                    {
                        Debug.Log("Target out of range but don't want to revert to idle. Do nothing.");
                    }
                }
                else
                {
                    tileSelectionCircle.SetActive(false);
                    ChangeState(new StateIdle(Machine));
                }
            }
        }
    }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        if (target)
        {
            t.selectedCharacter.GetActor().ItemAction(item, target);

        }
        else if (targetedTile)
        {
            t.selectedCharacter.GetActor().ItemAction(item, targetedTile);
        }
        else
            Debug.Log("No Target to Use Item. But how. Reverting to idle.");

        ChangeState(new StateWaitForAction(Machine, item.itemAction));
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        UnitAction action = t.GetBindings(index);

        if (action.GetType() == typeof(UnitActionInventory))
        {
            ChangeState(new StateIdle(Machine));
        }

        // If requirements aren't met, ignore button press
        bool requirementsMet = action.CheckRequirements();
        if (!requirementsMet) return;

        ButtonPress(index);
        action.UseAction();
        ChangeState(new StateWaitForAction(Machine, action));
    }

    public void ShowSelectionCircle(Vector3 position)
    {
        // Only show selection circle for aoe items
        if (((DamageItem)item).areaOfEffect <= 1) return;

        tileSelectionCircle.transform.position = position;
        float itemAreaOfEffect = ((DamageItem)item).areaOfEffect * GlobalManager.tileSpacing;
        tileSelectionCircle.transform.localScale = new Vector3(itemAreaOfEffect, itemAreaOfEffect, itemAreaOfEffect);
        tileSelectionCircle.SetActive(true);
    }
}
