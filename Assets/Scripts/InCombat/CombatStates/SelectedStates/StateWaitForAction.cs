using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWaitForAction : FiniteState<InCombatPlayerAction>
{
    UnitAction storedAction;
    public StateWaitForAction(StateMachine<InCombatPlayerAction> machine, UnitAction unitAction) : base(machine) { Machine = machine; storedAction = unitAction; }


    public override void Execute(InCombatPlayerAction t)
    {
        if (!storedAction.Performing())
            ChangeState(new StateIdle(Machine));
    }
}
