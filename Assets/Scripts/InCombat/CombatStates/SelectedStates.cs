using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedStates
{
    public class Idle : FiniteState<InCombatPlayerAction>
    {
        public Idle(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            t.PathPreviewClear();
            if (t.selectedCharacter)
            {
                t.selectedCharacter.potentialTargets = null;
                t.selectedCharacter.targetCharacter = null;
            }

            //Animate

            //Open AP HUD

            //Open Vitals HUD
        }

        public override void Execute(InCombatPlayerAction t)
        {
            Debug.Log("- IDLING -");
        }

        public override void Exit(InCombatPlayerAction t)
        {
        }
    }

    public class ChoosingMoveDestination : FiniteState<InCombatPlayerAction>
    {
        public ChoosingMoveDestination(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.potentialTargets = null;
            t.selectedCharacter.targetCharacter = null;
        }

        public override void Execute(InCombatPlayerAction t)
        {
        }

        public override void Exit(InCombatPlayerAction t)
        {
        }
    }

    public class Moving : FiniteState<InCombatPlayerAction>
    {
        public Moving(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
        }

        public override void Execute(InCombatPlayerAction t)
        {
        }

        public override void Exit(InCombatPlayerAction t)
        {
        }
    }

    public class ChoosingShootTarget : FiniteState<InCombatPlayerAction>
    {
        public ChoosingShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        List<Character> enemyList = new List<Character>();
        public override void Enter(InCombatPlayerAction t)
        {
            t.PathPreviewClear();
            Character[] gos = GameObject.FindObjectsOfType<Character>();

            foreach(var v in gos)
            {
                if (v.GetComponent<IFaction>() != null)
                {
                    if(t.selectedCharacter.faction != v.faction)
                    {
                        enemyList.Add(v);
                    }
                }
            }

            t.selectedCharacter.potentialTargets = enemyList.ToArray();

            if(t.selectedCharacter.potentialTargets != null)
            {
                t.selectedCharacter.targetCharacter = t.selectedCharacter.potentialTargets[0];
            }
            
        }

        public override void Execute(InCombatPlayerAction t)
        {
            Debug.Log("- ChoosingShootTarget -");
            bool b = false;
            if (t.selectedCharacter.targetCharacter != null)
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Physics.Raycast(ray, out hit))
                        if (hit.collider.GetComponent<Character>())
                            if (hit.collider.GetComponent<Character>() == t.selectedCharacter.targetCharacter)
                                if (Input.GetMouseButtonDown(0))
                                    b = true;
                }

                if (Input.GetKeyDown(KeyCode.Space) || b == true)
                {
                    t.stateMachine.ChangeState(new ShootTarget(t.stateMachine));
                }
            }
        }

        public override void Exit(InCombatPlayerAction t)
        {
        }
    }

    public class ShootTarget : FiniteState<InCombatPlayerAction>
    {
        public ShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
        float timer;
        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.ProcessAction(Actions.action_shoot, contextCharacter: t.selectedCharacter.targetCharacter, contextString: "attack");

            timer = Time.time + 1;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            Debug.Log("- SHOOTING Target -");

            if (timer < Time.time)
                t.state.ChangeState(new PostShootTarget(t.stateMachine));
        }

        public override void Exit(InCombatPlayerAction t)
        {
        }
    }

    public class PostShootTarget : FiniteState<InCombatPlayerAction>
    {
        public PostShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            t.stateMachine.ChangeState(new Idle(t.stateMachine));
        }

        public override void Execute(InCombatPlayerAction t)
        {
            Debug.Log("- POSTSHOOT -");
        }

        public override void Exit(InCombatPlayerAction t)
        {
            t.selectedCharacter.targetCharacter = null;
        }
    }
}