using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTarget : StateCancel
{
    public List<System.Type> CompatibleActions = new List<System.Type>() { typeof(UnitActionSwap) };
    public Faction targetFaction;
    public float targetRange;
    public StateTarget(StateMachine<InCombatPlayerAction> machine, Faction targetFaction, float targetRange = 50f) : base(machine) { Machine = machine; this.targetFaction = targetFaction; this.targetRange = targetRange; }

    public List<Unit> targets = new List<Unit>();
    public Unit target;
    public Tile targetedTile;

    public GameObject tileSelectionCircle;

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);

        // Instantiate tile selection circle
        tileSelectionCircle = t.selectedCharacter.grid.InstantiateTileSelectionCircle(Vector3.zero);
    }

    public override void Exit(InCombatPlayerAction t)
    {
        // Disable UI
        inventoryPanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(false);

        foreach (var v in targets)
        {
            v.TryGetComponent(out Unit c);
            c.GetActor().IsTargetUX(false, false);
        }

        t.selectedCharacter.grid.DestroyTileSelectionCircle(tileSelectionCircle);

        base.Exit(t);
    }

    public virtual void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        if (typeof(TargetType) == typeof(Unit))
        {
            List<Unit> units = t.activeMap.FindUnits(targetFaction);

            foreach (Unit unit in units)
                if (unit.stats.healthCurrent > 0 && TargetInRange(t.selectedCharacter, unit))
                    targets.Add(unit);
        }

        //Find closest Target
        if (targets.Count > 0)
        {
            targets.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
            });

            target = targets[0];
            t.selectedCharacter.GetActor().targetCharacter = target;
            infoPanel.CreateTargetButtons(targets);
        }
    }

    public bool TargetInRange(Unit sourceUnit, Unit targetedUnit)
    {
        // Returns true if target is within range of the item

        return (sourceUnit.transform.position - targetedUnit.transform.position).magnitude / GlobalManager.tileSpacing <= targetRange;
    }

    public virtual void ChangeTarget(InCombatPlayerAction t, Unit targetUnit)
    {
        t.selectedCharacter.GetActor().targetCharacter = targetUnit;
        infoPanel.CreateTargetButtons(targets);
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
                    {
                        t.selectedCharacter.GetActor().targetCharacter = c;
                        infoPanel.UpdateTargetButtons();
                    }
                }
            }
        }
    }

    public override void InputTab(InCombatPlayerAction t, bool shift)
    {
        int index = targets.IndexOf(target);
        int n = shift ? index - 1 : index + 1;

        if (n < 0) n = targets.Count - 1;
        if (n > targets.Count - 1) n = 0;

        ChangeTarget(t, targets[n]);
    }
}
