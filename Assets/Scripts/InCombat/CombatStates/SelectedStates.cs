using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedStates
{
    public class Idle : FiniteState<Character>
    {
        public Idle(StateMachine<Character> machine) : base(machine) { Machine = machine; }

        public override void Enter(Character t)
        {
            //Animate

            //Open AP HUD

            //Open Vitals HUD
        }

        public override void Execute(Character t)
        {
        }

        public override void Exit(Character t)
        {
        }
    }

    public class ChoosingMoveDestination : FiniteState<Character>
    {
        public ChoosingMoveDestination(StateMachine<Character> machine) : base(machine) { Machine = machine; }

        public override void Enter(Character t)
        {
        }

        public override void Execute(Character t)
        {
        }

        public override void Exit(Character t)
        {
        }
    }

    public class Moving : FiniteState<Character>
    {
        public Moving(StateMachine<Character> machine) : base(machine) { Machine = machine; }

        public override void Enter(Character t)
        {
        }

        public override void Execute(Character t)
        {
        }

        public override void Exit(Character t)
        {
        }
    }

    public class ChoosingShootTarget : FiniteState<Character>
    {
        public ChoosingShootTarget(StateMachine<Character> machine) : base(machine) { Machine = machine; }

        public override void Enter(Character t)
        {
        }

        public override void Execute(Character t)
        {
        }

        public override void Exit(Character t)
        {
        }
    }
}