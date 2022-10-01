using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePostShootTarget : FiniteState<InCombatPlayerAction>
{
    public StatePostShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
}
