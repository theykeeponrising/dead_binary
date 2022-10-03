using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWaitForAction : FiniteState<InCombatPlayerAction>
{
    UnitAction storedAction;
    FiniteState<InCombatPlayerAction> NextState;
    public StateWaitForAction(StateMachine<InCombatPlayerAction> machine, UnitAction unitAction, FiniteState<InCombatPlayerAction> nextState = null) : base(machine) { Machine = machine; storedAction = unitAction; NextState = nextState; }


    public override void Execute(InCombatPlayerAction t)
    {
        // Waits until the storedAction completes, and then proceed to the next state
        // If no state was provided, default to StateIdle

        if (NextState == null) NextState = new StateIdle(Machine);
        if (!storedAction.Performing())
            ChangeState(NextState);
    }
}
