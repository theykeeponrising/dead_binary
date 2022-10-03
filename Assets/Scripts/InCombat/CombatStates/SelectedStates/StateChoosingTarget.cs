using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChoosingTarget : StateTarget
{
    UnitAction storedAction;

    public StateChoosingTarget(StateMachine<InCombatPlayerAction> machine, UnitAction setAction) : base(machine) { Machine = machine; storedAction = setAction; }

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);
        t.GetType();
        t.PathPreviewClear();
        infoPanel.UpdateAction(storedAction);

        //Make a list of Enemies
        Unit[] gos = GameObject.FindObjectsOfType<Unit>();
        foreach (var v in gos)
        {
            if (v.GetComponent<IFaction>() != null)
                if (t.selectedCharacter.attributes.faction != v.attributes.faction)
                    if (v.stats.healthCurrent > 0)
                        targets.Add(v);
        }

        //Find closest target
        if (targets.Count > 0)
        {
            targets.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
            });

            t.selectedCharacter.GetActor().potentialTargets = targets;
            t.selectedCharacter.GetActor().targetCharacter = t.selectedCharacter.GetActor().targetCharacter != null ? t.selectedCharacter.GetActor().targetCharacter : targets[0];
            t.selectedCharacter.GetActor().GetTarget();
            target = targets[0];
        }
        else
        {
            Debug.LogWarning("There are no nearby enemies. If there should be, check to see that their Faction is not the same as the Selected Character. Reverting to Idle");
            ChangeState(new StateIdle(Machine));
        }
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter == null) ChangeState(new StateNoSelection(Machine));

        foreach (var v in targets)
        {
            if (v == t.selectedCharacter.GetActor().targetCharacter)
                v.GetActor().IsTargetUX(true, true);
            else
                v.GetActor().IsTargetUX(false, true);
        }
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        // If valid target, make Target
        if (!IsPointerOverUIElement(t))
        {
            Camera raycastCamera = Camera.main;
            RaycastHit hit;
            Ray ray;

            if (t.selectedCharacter.GetComponentInChildren<Camera>())
                raycastCamera = t.selectedCharacter.GetComponentInChildren<Camera>();

            ray = raycastCamera.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

            int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                if (hit.collider.GetComponent<Unit>())
                {
                    var c = hit.collider.GetComponent<Unit>();

                    if (targets.Contains(c))
                        t.selectedCharacter.GetActor().targetCharacter = c;
                }
            }
        }
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        // Perform action based on which button was selected

        UnitAction action = t.GetBindings(index);

        // Same action pressed -- execute action
        if (storedAction == action)
        {
            InputSpacebar(t);
        }

        // An action that will not interrupt aiming is pressed
        else if (CompatibleActions.Contains(action.GetType()))
        {
            ButtonPress(index);
            action.UseAction();
        }

        // If we are selecting a state-changing action, allow action to proceed
        else if (action.GetType().IsSubclassOf(typeof(UnitStateAction)))
        {
            ButtonPress(index);
            t.selectedCharacter.GetActor().ClearTarget();
            action.UseAction(this);
        }

        // Switching state
        else
        {
            t.selectedCharacter.GetActor().ClearTarget();
            new StateIdle(Machine).InputActionBtn(t, index);
            ChangeState(new StateIdle(Machine));
        }
    }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        // Spacebar and shoot action will execute shoot while in targeting

        int index = t.GetIndex(storedAction);
        infoPanel.gameObject.SetActive(false);
        ButtonPress(index);
        storedAction.UseAction(t.selectedCharacter.GetActor().targetCharacter);
        ChangeState(new StateWaitForAction(Machine, storedAction));
        //ChangeState(new StateShootTarget(Machine, storedAction));
    }
}