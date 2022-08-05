using System.Collections.Generic;
using UnityEngine;

public class SelectedStates
{
    public class Idle : FiniteState<InCombatPlayerAction>
    {
        public Idle(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            if (t.selectedCharacter)
            {
                t.selectedCharacter.potentialTargets = null;
                t.selectedCharacter.targetCharacter = null;
            }
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter == null)
                ChangeState(new NoTargetSelected(Machine));
        }
        public override void InputPrimary(InCombatPlayerAction t)
        {
            t.SelectUnit();
        }
        public override void InputAction1(InCombatPlayerAction t)
        {
            ChangeState(new ChoosingMoveDestination(Machine));
        }
        public override void InputAction2(InCombatPlayerAction t)
        {
            ChangeState(new ChoosingShootTarget(Machine));
        }
        public override void InputAction3(InCombatPlayerAction t)
        {
            ChangeState(new Reloading(Machine));
        }
        public override void InputAction4(InCombatPlayerAction t)
        {
            ChangeState(new RefreshingAP(Machine));
        }
    }

    public class NoTargetSelected : FiniteState<InCombatPlayerAction>
    {
        public NoTargetSelected(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter)
                ChangeState(new Idle(Machine));
        }
        public override void InputPrimary(InCombatPlayerAction t)
        {
            t.SelectUnit();
        }
    }

    public class ChoosingMoveDestination : FiniteState<InCombatPlayerAction>
    {
        public ChoosingMoveDestination(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
        }
        public override void InputAction1(InCombatPlayerAction t)
        {
            Debug.Log("AP: " + t.selectedCharacter.stats.actionPointsCurrent);
            if (t.selectedCharacter.stats.actionPointsCurrent > 0)
            {
                t.MoveCharacter();
                ChangeState(new Moving(Machine));
            }
            else
                ChangeState(new Idle(Machine));
        }
        public override void InputSecndry(InCombatPlayerAction t)
        {
            if (t.selectedCharacter.stats.actionPointsCurrent > 0)
            {
                t.MoveCharacter();
                ChangeState(new Moving(Machine));
            }
            else
                ChangeState(new Idle(Machine));
        }
        public override void InputAction2(InCombatPlayerAction t)
        {
            ChangeState(new ChoosingShootTarget(Machine));
        }
        public override void InputAction3(InCombatPlayerAction t)
        {
            ChangeState(new Reloading(Machine));
        }
        public override void InputAction4(InCombatPlayerAction t)
        {
            ChangeState(new RefreshingAP(Machine));
        }
    }

    public class Moving : FiniteState<InCombatPlayerAction>
    {
        public Moving(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter)
                if (t.selectedCharacter.isAtDestination)
                    ChangeState(new Idle(Machine));
            else
            {
                t.PathPreviewClear();
                ChangeState(new NoTargetSelected(Machine));
            }
        }
    }

    public class ChoosingShootTarget : FiniteState<InCombatPlayerAction>
    {
        public ChoosingShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        List<Character> enemyList = new List<Character>();

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            t.PathPreviewClear();
            Character[] gos = GameObject.FindObjectsOfType<Character>();

            foreach (var v in gos)
            {
                if (v.GetComponent<IFaction>() != null)
                {
                    if (t.selectedCharacter.faction != v.faction)
                    {
                        enemyList.Add(v);
                    }
                }
            }

            // t.selectedCharacter.potentialTargets = new Character[enemyList.Count];
            //t.selectedCharacter.potentialTargets = enemyList.ToArray();

            t.selectedCharacter.potentialTargets = new List<Character>();
            t.selectedCharacter.potentialTargets = enemyList;

            if (t.selectedCharacter.potentialTargets.Count > 0)
                t.selectedCharacter.targetCharacter = t.selectedCharacter.potentialTargets[0];
            else
                Debug.LogWarning("There are no nearby enemies. If there should be, check to see that their Faction is not the same as the Selected Character.");
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter.targetCharacter)
            {
               // Place a small ball above the target.
               t.selectorBall.SetActive(true);
               t.selectorBall.transform.position = t.selectedCharacter.targetCharacter.transform.position + Vector3.up * 2;
            }
            else
            {
                t.selectorBall.transform.position = Vector3.zero;
                t.selectorBall.SetActive(false);
            }
        }
        public override void InputPrimary(InCombatPlayerAction t)
        {
            t.SelectUnit();
        }
        public override void InputSecndry(InCombatPlayerAction t)
        {
            // Check for Target (Should already have).
            if(t.selectedCharacter.targetCharacter)
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());
                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                // What are we hitting?
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.GetComponent<Character>())
                    {
                        var v = hit.collider.GetComponent<Character>();

                        // If Right Click on Target, shoot it.
                        if (v == t.selectedCharacter.targetCharacter)
                            ChangeState(new ShootTarget(Machine));

                        // If Potential Target, Switch Target
                        else if (t.selectedCharacter.potentialTargets.Contains(v))
                            t.selectedCharacter.targetCharacter = v;

                        // If character is not potential target, throw error
                        else if (!t.selectedCharacter.potentialTargets.Contains(v))
                            Debug.Log("Unit not Targetable.");
                    }
                    
                    // If no character is pressed, revert to Idle.
                    else
                        ChangeState(new Idle(Machine));
                }
            }
        }
        public override void InputAction1(InCombatPlayerAction t)
        {
            ChangeState(new ChoosingMoveDestination(Machine));
        }
        public override void InputAction2(InCombatPlayerAction t)
        {
            if (t.selectedCharacter.targetCharacter)
                ChangeState(new ShootTarget(Machine));
            else
                Debug.Log("No Target -- But how?");
        }
        public override void InputAction3(InCombatPlayerAction t)
        {
            ChangeState(new Reloading(Machine));
        }
        public override void InputAction4(InCombatPlayerAction t)
        {
            ChangeState(new RefreshingAP(Machine));
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
            if (timer < Time.time)
                ChangeState(new PostShootTarget(Machine));
        }
    }

    public class PostShootTarget : FiniteState<InCombatPlayerAction>
    {
        public PostShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            ChangeState(new Idle(Machine));
        }
        public override void Exit(InCombatPlayerAction t)
        {
            t.selectorBall.SetActive(false);
            t.selectedCharacter.targetCharacter = null;
        }
    }

    public class Reloading : FiniteState<InCombatPlayerAction>
    {
        public Reloading(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        float timer = 1;
        public override void Enter(InCombatPlayerAction t)
        {
            timer += Time.time;
            t.selectedCharacter.ProcessAction(Actions.action_reload);
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (Time.time > timer)
            {
                ChangeState(new Idle(Machine));
            }
        }
    }

    public class RefreshingAP : FiniteState<InCombatPlayerAction>
    {
        public RefreshingAP(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
        float timer = 1;
        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.RefreshActionPoints();
            timer += Time.time;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time)
                ChangeState(new Idle(Machine));
        }
    }
}