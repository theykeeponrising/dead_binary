using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateNoSelection : FiniteState<InCombatPlayerAction>
{
    public StateNoSelection(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

    /*
    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);
    }
    */
    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter)
            ChangeState(new StateIdle(Machine));
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        if (!IsPointerOverUIElement(t))
            t.SelectUnit();
    }
}