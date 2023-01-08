using System.Collections.Generic;
using UnityEngine;

public class StateTarget : StateCancel
{
    protected List<System.Type> compatibleActions = new() { typeof(UnitActionSwap) };
    protected UnitTargetAction storedAction;
    public StateTarget(StateMachine<InCombatPlayerAction> machine, UnitTargetAction storedAction) : base(machine) { Machine = machine; this.storedAction = storedAction; }

    List<Unit> targets;
    TargetType targetType = TargetType.CHARACTER;
    Faction targetFaction;

    protected Unit targetedUnit;
    protected Tile targetedTile;

    private float targetRange = 50f;
    private float areaOfEffect = 1f;

    private GameObject indicatorAOE;

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);

        // Display info panel
        infoPanel.gameObject.SetActive(true);
        infoPanel.UpdateAction(storedAction);
        infoPanel.UpdateHit(-1);

        targets = new List<Unit>();
        targetFaction = t.selectedCharacter.Attributes.Faction.GetFactionsByRelation(storedAction.TargetFaction)[0];

        if (storedAction.GetType().IsSubclassOf(typeof(UnitActionItem)))
        {
            targetFaction = t.selectedCharacter.Attributes.Faction.GetFactionsByRelation(storedAction.Item.targetFaction)[0];
            targetRange = storedAction.Item.range;
            areaOfEffect = storedAction.Item.areaOfEffect;
            targetType = storedAction.Item.targetType;

            if (storedAction.Item.GetType().BaseType == typeof(DamageItem))
            {
                var itemType = (DamageItem)storedAction.Item;
                infoPanel.UpdateDamage(itemType.hpAmount);
            }
        }
        else if (storedAction.GetType() == typeof(UnitActionShootRocket))
        {
            Weapon weapon = t.selectedCharacter.EquippedWeapon;
            targetFaction = t.selectedCharacter.Attributes.Faction.GetFactionsByRelation(storedAction.TargetFaction)[0];
            areaOfEffect = weapon.Stats.AreaOfEffect;
            targetType = TargetType.CHARACTER;
            infoPanel.UpdateDamage(weapon.GetDamage());
        }

        // Instantiate tile selection circle
        indicatorAOE = UIManager.InstantiateIndicatorAOE(Vector3.zero);

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
            c.SelectUnit(SelectionType.CLEAR);

            // Show healthbar
            v.Healthbar.gameObject.SetActive(true);
        }

        UIManager.DestroyIndicatorAOE(indicatorAOE);

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
                    c.SelectUnit(SelectionType.TARGET_MAIN);
                else
                    c.SelectUnit(SelectionType.TARGET_POTENTIAL);
            }
        }
    }

    public virtual void FindTargets<TargetType>(InCombatPlayerAction t)
    {
        if (typeof(TargetType) == typeof(Unit))
        {
            List<Unit> units = Map.FindUnits(targetFaction);

            // Iterates through enemy faction units, and adds them if they are
            // Alive, In Range, and within the line of sight
            foreach (Unit unit in units)
                if (unit.Stats.HealthCurrent > 0 &&
                    TargetInRange(t.selectedCharacter, unit) &&
                    t.selectedCharacter.IsTargetInLineOfSight(unit))
                {
                    targets.Add(unit);
                }
        }

        //Find closest Target
        if (targets.Count > 0)
        {
            targets.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
            });

            if (!targetedUnit) 
                ChangeTarget(t, targets[0], initialTargets: true);
            else 
                ChangeTarget(t, targetedUnit, initialTargets: true);
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
        t.selectedCharacter.TargetUnit = targetedUnit;
        t.selectedCharacter.UpdateHitStats();
        if (initialTargets) infoPanel.CreateTargetButtons(targets);
        else infoPanel.UpdateTargetButtons();
        ShowSelectionCircle(targetedUnit.transform.position);
        ShowHealtbar(targetedUnit);

        if (!storedAction.UseCharacterCamera)
            Camera.main.GetComponent<CameraHandler>().SetCameraSnap(targetedUnit);
    }

    public virtual void ChangeTarget(InCombatPlayerAction t, Tile targetTile, bool initialTargets = false)
    {
        // Changes target to provided tile, clears unit target

        targetedUnit = null;
        targetedTile = targetTile;
        t.selectedCharacter.TargetUnit = null;
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
        foreach (Unit target in targets) target.Healthbar.gameObject.SetActive(false);
        targetedUnit.Healthbar.gameObject.SetActive(true);
    }

    public void SetStoredAction(UnitTargetAction newAction, Weapon weapon)
    {
        storedAction = newAction;
        areaOfEffect = weapon.Stats.AreaOfEffect;
        infoPanel.UpdateDamage(weapon.GetDamage());
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        if (!IsPointerOverUIElement(t))
        {
            Camera camera = CameraHandler.ActiveCamera;
            Ray ray = camera.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Directly targeting a valid unit with mouse primary
                if (targets.Contains(hit.collider.GetComponent<Unit>()) && storedAction.TargetTypes.Contains(TargetType.CHARACTER))
                {
                    if (targetedUnit != hit.collider.GetComponent<Unit>())
                    {
                        ChangeTarget(t, hit.collider.GetComponent<Unit>());
                    }
                }

                // Directly targeting a tile with mouse primary
                else if (hit.collider.gameObject.GetComponent<Tile>() && storedAction.TargetTypes.Contains(TargetType.TILE))
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
                    return;
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
        {
            storedAction.UseAction();
        }

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
