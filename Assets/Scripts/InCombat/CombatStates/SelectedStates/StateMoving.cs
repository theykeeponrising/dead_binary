using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMoving : FiniteState<InCombatPlayerAction>
{
    public StateMoving(StateMachine<InCombatPlayerAction> machine, Tile destination) : base(machine) { Machine = machine; _destination = destination; }

    Tile _destination;
    float timer = 5; //Failsafe

    public override void Enter(InCombatPlayerAction t)
    {
        timer += Time.time;
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter)
            if (t.selectedCharacter.currentTile == _destination)
                ChangeState(new StateIdle(Machine));

        if (Time.time > timer)
        {
            Debug.LogWarning("Character timed out of Moving. Reverting to Idle state.");
            ChangeState(new StateIdle(Machine));
        }
    }
}
