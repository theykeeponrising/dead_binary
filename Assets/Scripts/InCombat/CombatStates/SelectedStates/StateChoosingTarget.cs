using UnityEngine;

public class StateChoosingTarget : StateTarget
{
    public StateChoosingTarget(StateMachine<InCombatPlayerAction> machine, UnitTargetAction storedAction) : base(machine, storedAction) { Machine = machine; this.storedAction = storedAction; }

    public override void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        base.FindTargets<TargetType>(t);
        t.selectedCharacter.GetActor().GetTarget(storedAction.UseCharacterCamera);
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        // Perform action based on which button was selected

        UnitAction action = t.GetBindings(index);

        // Same action pressed -- execute action
        if (storedAction == action)
        {
            InputSpacebar(t);
        }

        // If we are swapping weapons, change the stored action to the new shoot action
        else if (action.GetType() == (typeof(UnitActionSwap)))
        {
            ButtonPress(index);
            action.UseAction(this);
            ChangeState(new StateWaitForAction(Machine, action, this));
        }

        // An action that will not interrupt aiming is pressed
        else if (compatibleActions.Contains(action.GetType()))
        {
            ButtonPress(index);
            action.UseAction();
            ChangeState(new StateWaitForAction(Machine, action, this));
        }

        // If we are selecting a state-changing action, allow action to proceed
        else if (action.GetType().IsSubclassOf(typeof(UnitStateAction)))
        {
            ButtonPress(index);
            t.selectedCharacter.GetActor().ClearTarget();
            action.UseAction(this);
        }

        // Switching state
        else
        {
            t.selectedCharacter.GetActor().ClearTarget();
            StateIdle stateIdle = new(Machine);
            stateIdle.InputActionBtn(t, index);
            ChangeState(new StateWaitForAction(Machine, action, stateIdle));
        }
    }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        // Spacebar and shoot action will execute shoot while in targeting

        int index = t.GetIndex(storedAction);
        infoPanel.gameObject.SetActive(false);
        ButtonPress(index);

        if (targetedUnit)
            storedAction.UseAction(targetedUnit);
        else if (targetedTile)
            storedAction.UseAction(targetedTile);
        else
            Debug.Log("Action called with no target!");
        ChangeState(new StateWaitForAction(Machine, storedAction));
    }
}