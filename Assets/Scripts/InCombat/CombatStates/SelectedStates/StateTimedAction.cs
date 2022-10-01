using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTimedAction : FiniteState<InCombatPlayerAction>
{
    UnitAction action;
    public StateTimedAction(StateMachine<InCombatPlayerAction> machine, UnitAction performAction) : base(machine) { Machine = machine; action = performAction; }

    float timer = 0.25f;
    public override void Enter(InCombatPlayerAction t)
    {
        t.selectedCharacter.GetActor().ProcessAction(action);
        timer += Time.time;
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (timer < Time.time)
            ChangeState(new StateIdle(Machine));
    }
}
