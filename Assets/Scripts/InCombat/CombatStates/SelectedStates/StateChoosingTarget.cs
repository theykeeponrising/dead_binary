using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChoosingTarget : StateTarget
{
    public StateChoosingTarget(StateMachine<InCombatPlayerAction> machine, UnitTargetAction storedAction) : base(machine, storedAction) { Machine = machine; this.storedAction = storedAction; }

    public override void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        base.FindTargets<TargetType>(t);
        t.selectedCharacter.GetActor().GetTarget();
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

        // An action that will not interrupt aiming is pressed
        else if (CompatibleActions.Contains(action.GetType()))
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
            StateIdle stateIdle = new StateIdle(Machine);
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
        storedAction.UseAction(t.selectedCharacter.GetActor().targetCharacter);
        ChangeState(new StateWaitForAction(Machine, storedAction));
    }
}