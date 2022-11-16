using UnityEngine;

public class StateUseItem : StateTarget
{
    public StateUseItem(StateMachine<InCombatPlayerAction> machine, UnitTargetAction storedAction) : base(machine, storedAction) { Machine = machine; }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        if (targetedUnit)
        {
            t.selectedCharacter.UseItem(storedAction.item, targetedUnit);

        }
        else if (targetedTile)
        {
            t.selectedCharacter.UseItem(storedAction.item, targetedTile);
        }
        else
            Debug.Log("No Target to Use Item. But how. Reverting to idle.");

        ChangeState(new StateWaitForAction(Machine, storedAction));
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        UnitAction action = t.GetBindings(index);

        if (action.GetType() == typeof(UnitActionInventory))
        {
            ChangeState(new StateIdle(Machine));
        }

        else if (action.GetType().IsSubclassOf(typeof(UnitTargetAction)))
        {
            ButtonPress(index);
            ChangeState(new StateChoosingTarget(Machine, (UnitTargetAction)action));
        }

        else
        {
            // If requirements aren't met, ignore button press
            bool requirementsMet = action.CheckRequirements();
            if (!requirementsMet) return;

            ButtonPress(index);
            action.UseAction();
            ChangeState(new StateWaitForAction(Machine, action));
        }
    }
}
