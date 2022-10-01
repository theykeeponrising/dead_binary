using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateShootTarget : FiniteState<InCombatPlayerAction>
{
    UnitAction unitAction;
    public StateShootTarget(StateMachine<InCombatPlayerAction> machine, UnitAction setAction) : base(machine) { Machine = machine; unitAction = setAction; }

    float timer;

    public override void Enter(InCombatPlayerAction t)
    {
        unitAction.UseAction(t.selectedCharacter.GetActor().targetCharacter);
        timer = Time.time + 1;
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (timer < Time.time - 1.5f)
        {
            t.selectedCharacter.GetActor().ClearTarget();
            ChangeState(new StateIdle(Machine));
        }
    }
}
