using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTarget : StateCancel
{
    protected List<System.Type> compatibleActions = new List<System.Type>() { typeof(UnitActionSwap) };
    protected UnitTargetAction storedAction;
    public StateTarget(StateMachine<InCombatPlayerAction> machine, UnitTargetAction storedAction) : base(machine) { Machine = machine; this.storedAction = storedAction; }

    List<Unit> targets;
    TargetType targetType = TargetType.CHARACTER;
    Faction targetFaction;

    protected Unit targetedUnit;
    protected Tile targetedTile;

    float targetRange = 50f;
    float areaOfEffect = 1f;

    GameObject indicatorAOE;

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);

        // Display info panel
        infoPanel.gameObject.SetActive(true);
        infoPanel.UpdateAction(storedAction);
        infoPanel.UpdateHit(-1);

        targets = new List<Unit>();
        targetFaction = t.selectedCharacter.attributes.faction.GetFactionsByRelation(storedAction.targetFaction)[0];

        if (storedAction.GetType().IsSubclassOf(typeof(UnitActionItem)))
        {
            targetFaction = t.selectedCharacter.attributes.faction.GetFactionsByRelation(storedAction.item.targetFaction)[0];
            targetRange = storedAction.item.range;
            areaOfEffect = storedAction.item.areaOfEffect;
            targetType = storedAction.item.targetType;

            if (storedAction.item.GetType().BaseType == typeof(DamageItem))
            {
                var itemType = (DamageItem)storedAction.item;
                infoPanel.UpdateDamage(itemType.hpAmount);
            }
        }

        // Instantiate tile selection circle
        indicatorAOE = GlobalManager.ActiveMap.mapGrid.InstantiateIndicatorAOE(Vector3.zero);

        //Find Targets
        switch (targetType)
        {
            case TargetType.CHARACTER:
                FindTargets<Unit>(t);
                break;
            default:
                Debug.Log("Types other than Character are not yet implemented");
                break;
        }
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

            // Show healthbar
            v.healthbar.gameObject.SetActive(true);
        }

        GlobalManager.ActiveMap.mapGrid.DestroyIndicatorAOE(indicatorAOE);

        base.Exit(t);
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter == null) ChangeState(new StateNoSelection(Machine));

        foreach (var v in targets)
        {
            if (v.GetComponent<Unit>() == true)
            {
                Unit c = v.GetComponent<Unit>();

                if (v == targetedUnit)
                    c.GetActor().IsTargetUX(true, true);
                else
                    c.GetActor().IsTargetUX(false, true);
            }
        }
    }

    public virtual void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        if (typeof(TargetType) == typeof(Unit))
        {
            List<Unit> units = Map.FindUnits(targetFaction);

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

            if (!targetedUnit) ChangeTarget(t, targets[0], initialTargets: true);
            else ChangeTarget(t, targetedUnit, initialTargets: true);
        }
    }

    public bool TargetInRange(Unit sourceUnit, Unit targetUnit)
    {
        // Returns true if target unit is within range of the action

        return Mathf.Round(Vector3.Distance(sourceUnit.transform.position, targetUnit.transform.position) / GlobalManager.tileSpacing) <= targetRange;
    }

    public bool TargetInRange(Unit sourceUnit, Tile targetTile)
    {
        // Returns true if target tile is within range of the action

        return Mathf.Round(Vector3.Distance(sourceUnit.transform.position, targetTile.transform.position) / GlobalManager.tileSpacing) <= targetRange;
    }

    public virtual void ChangeTarget(InCombatPlayerAction t, Unit targetUnit, bool initialTargets = false)
    {
        // Changes target to provided unit, clears tile target

        targetedUnit = targetUnit;
        targetedTile = null;
        t.selectedCharacter.GetActor().targetCharacter = targetedUnit;
        t.selectedCharacter.GetActor().UpdateHitStats();
        if (initialTargets) infoPanel.CreateTargetButtons(targets);
        else infoPanel.UpdateTargetButtons();
        ShowSelectionCircle(targetedUnit.transform.position);
        ShowHealtbar(targetedUnit);
    }

    public virtual void ChangeTarget(InCombatPlayerAction t, Tile targetTile, bool initialTargets = false)
    {
        // Changes target to provided tile, clears unit target

        targetedUnit = null;
        targetedTile = targetTile;
        t.selectedCharacter.GetActor().targetCharacter = null;
        if (initialTargets) infoPanel.CreateTargetButtons(targets);
        else infoPanel.UpdateTargetButtons();
        ShowSelectionCircle(targetedTile.transform.position);
    }

    public void ShowSelectionCircle(Vector3 position)
    {
        // Only show selection circle for aoe items
        if (areaOfEffect <= 1) return;

        indicatorAOE.transform.position = position;
        float itemAreaOfEffect = areaOfEffect * GlobalManager.tileSpacing;
        indicatorAOE.transform.localScale = new Vector3(itemAreaOfEffect, itemAreaOfEffect, itemAreaOfEffect);
        indicatorAOE.SetActive(true);
    }

    public void ShowHealtbar(Unit targetedUnit)
    {
        foreach (Unit target in targets) target.healthbar.gameObject.SetActive(false);
        targetedUnit.healthbar.gameObject.SetActive(true);
    }

    public void SetStoredAction(UnitTargetAction newAction)
    {
        storedAction = newAction;
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        if (!IsPointerOverUIElement(t))
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Directly targeting a valid unit with mouse primary
                if (targets.Contains(hit.collider.GetComponent<Unit>()))
                {
                    if (targetedUnit != hit.collider.GetComponent<Unit>())
                    {
                        ChangeTarget(t, hit.collider.GetComponent<Unit>());
                    }
                }

                // Directly targeting an invalid unit with mouse primary
                else if (hit.collider.gameObject.GetComponent<Unit>())
                {
                    Debug.Log("Not a target but don't want to revert to idle. Do nothing.");
                }

                // Directly targeting a tile with mouse primary
                else if (hit.collider.gameObject.GetComponent<Tile>())
                {
                    if (TargetInRange(t.selectedCharacter, hit.collider.gameObject.GetComponent<Tile>()))
                    {
                        ChangeTarget(t, hit.collider.gameObject.GetComponent<Tile>());
                    }
                    else
                    {
                        Debug.Log("Target out of range but don't want to revert to idle. Do nothing.");
                    }
                }
                else
                {
                    indicatorAOE.SetActive(false);
                    ChangeState(new StateIdle(Machine));
                }
            }
        }
    }

    public override void InputSpacebar(InCombatPlayerAction t)
    {
        if (targetedUnit)
        {
            storedAction.UseAction(targetedUnit);
        }
        else if (targetedTile)
        {
            storedAction.UseAction(targetedTile);
        }
        else
            storedAction.UseAction();

        ChangeState(new StateWaitForAction(Machine, storedAction));
    }

    public override void InputTab(InCombatPlayerAction t, bool shift)
    {
        int index = targets.IndexOf(targetedUnit);
        int n = shift ? index - 1 : index + 1;

        if (n < 0) n = targets.Count - 1;
        if (n > targets.Count - 1) n = 0;
        if (targets.Count == 0) return;

        ChangeTarget(t, targets[n]);
    }
}
