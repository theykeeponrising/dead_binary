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

    public void FindTargets<T>(InCombatPlayerAction t)
    {
        var x = typeof(T);

        if (x == typeof(Unit))
        {
            Unit[] chars = GameObject.FindObjectsOfType<Unit>();
            foreach (var v in chars)
            {
                if (v.GetComponent<IFaction>() != null && v.stats.healthCurrent > 0)
                {
                    if (item.CheckAffinity(t.selectedCharacter, v) == true)
                    {
                        targets.Add(v);
                    }
                }
            }
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
                    }
                }
                else if (hit.collider.gameObject.GetComponent<Unit>())
                {
                    Debug.Log("Not a target but don't want to revert to idle. Do nothing.");
                }
                else
                    ChangeState(new StateIdle(Machine));
            }
        }
    }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        if (target)
        {
            t.selectedCharacter.GetActor().ItemAction(item, target);
        }
        else
            Debug.Log("No Target to Use Item. But how. Reverting to idle.");
        ChangeState(new StateIdle(Machine));
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
