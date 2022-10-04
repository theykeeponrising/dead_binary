using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateUseItem : StateTarget
{
    Item item;

    public StateUseItem(StateMachine<InCombatPlayerAction> machine, Item useItem) : base(machine) { Machine = machine; item = useItem; }

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

    public void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        if (typeof(TargetType) == typeof(Unit))
        {
            List<Unit> units = t.activeMap.FindUnits(item.GetAffinity(t.selectedCharacter));

            foreach (Unit unit in units)
                if (unit.stats.healthCurrent > 0 && ((DamageItem)item).isTargetInRange(t.selectedCharacter, unit))
                    targets.Add(unit);
        }

        //Find closest Target
        if (targets.Count > 0)
        {
            targets.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
            });

            target = targets[0];
        }
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
                if (targets.Contains(hit.collider.GetComponent<Unit>()))
                {
                    if (target != hit.collider.GetComponent<Unit>())
                    {
                        target = hit.collider.GetComponent<Unit>();
                        targetedTile = null;
                        tileSelectionCircle.SetActive(false);
                    }
                }
                else if (hit.collider.gameObject.GetComponent<Unit>())
                {
                    Debug.Log("Not a target but don't want to revert to idle. Do nothing.");
                }
                else if (hit.collider.gameObject.GetComponent<Tile>())
                {                    
                    if (((DamageItem)item).isTargetInRange(t.selectedCharacter, hit.collider.gameObject.GetComponent<Tile>()))
                    {
                        target = null;
                        targetedTile = hit.collider.gameObject.GetComponent<Tile>();

                        tileSelectionCircle.transform.position = targetedTile.transform.position;
                        tileSelectionCircle.SetActive(true);
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
}
