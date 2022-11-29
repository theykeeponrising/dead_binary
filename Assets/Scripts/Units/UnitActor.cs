using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Class if focused on implementing the characters actions
//I.e. the logic for moving, shooting, jumping, etc.
//May be worth splitting up further
public class UnitActor
{
    private readonly Unit _unit;
    private readonly SelectionCircle _selectionCircle;

    public UnitStats Stats { get { return _unit.Stats; } }
    public MoveData MoveData = new();

    public UnitActor(Unit unit)
    {
        _unit = unit;
        _selectionCircle = unit.GetComponentInChildren<SelectionCircle>();
    }

    public void Update()
    {
        Movement();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlights unit on mouse over
        SelectUnit(SelectionType.HIGHLIGHT);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave
        SelectUnit(SelectionType.CLEAR);
    }

    public void SelectUnit(SelectionType selectionType = SelectionType.CLEAR)
    {
        // Changes selection circle based on selection type
        _selectionCircle.ChangeSelection(selectionType);
    }

    public UnitAction FindActionOfType(System.Type actionType, bool printDebug = false)
    {
        // Finds an action by type from the unit's current action list

        foreach (UnitAction unitAction in _unit.GetUnitActions())
        {
            if (printDebug) Debug.Log(unitAction.GetType());
            if (unitAction.GetType() == actionType)
                return unitAction;
        }
        return null;
    }

    public bool IsActing()
    {
        // Checks all unit actions to see if any are currently performing
        // Returns True/False if any action is currently performing

        foreach (UnitAction action in _unit.GetUnitActions())
            if (action.Performing())
                return true;

        foreach (Item item in _unit.GetItems())
            if (item.itemAction.Performing())
                return true;

        return false;
    }

    public void SetWaiting(bool isWaiting)
    {
        // Toggles the unit's "waiting" state

        UnitAction waitAction = FindActionOfType(typeof(UnitActionWait));
        waitAction?.SetPerformed(performed: isWaiting);
        _unit.Healthbar.ShowWaitIndicator(showSprites: isWaiting);
    }

    private void Movement()
    {
        // Function that actually moves target towards destination
        // If there is no move target, then this action is skipped
        
        // If we have a move target, begin moving
        if (MoveData.Immediate)
        {
            Vector3 relativePos;
            Vector3 moveTargetPoint = MoveData.Destination.StandPoint;
            float distance = Vector3.Distance(_unit.transform.position, moveTargetPoint);
            MoveData.Velocity.z = distance / 2;

            // Slow down movement speed if character is vaulting
            float distanceDelta = (_unit.IsVaulting() || _unit.IsPatrolling()) ? 0.0125f : 0.03f;

            // If the final move target is also the most immediate one, slow down move speed as we approach
            if (MoveData.Destination == MoveData.Immediate)
            {
                _unit.transform.position = Vector3.MoveTowards(_unit.transform.position, moveTargetPoint, distanceDelta);
                relativePos = moveTargetPoint - _unit.transform.position;
            }
            else
            {
                _unit.transform.position = Vector3.MoveTowards(_unit.transform.position, MoveData.Immediate.transform.position, distanceDelta);
                relativePos = MoveData.Immediate.transform.position - _unit.transform.position;
            }

            // Gradually rotate character to face towards move target
            if (relativePos != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(relativePos);
                toRotation.x = _unit.transform.rotation.x;
                toRotation.z = _unit.transform.rotation.z;
                _unit.transform.rotation = Quaternion.Lerp(_unit.transform.rotation, toRotation, 10 * Time.time);
            }
        }

        // Gradually rotate character to expected look direction while behind cover
        else if (_unit.CurrentCover && !_unit.IsActing())
        {
            // Get which the direction the cover is relative to the tile
            Vector3 lookDirection = (_unit.CurrentCover.transform.position - _unit.Tile.transform.position);

            // Add the direction to the tile world space position to get a world space point to look at
            lookDirection += _unit.Tile.transform.position;

            // Remove vertical position for a flat lookat point
            lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);

            // Character look at position
            _unit.transform.rotation = Quaternion.Slerp(_unit.transform.rotation, Quaternion.LookRotation(lookDirection - _unit.transform.position), 3 * Time.time);
        }
    }

    public List<Tile> GetMovePath(Tile tile)
    {
        // Gets either the full or partial movement path to tile

        int maxDist = Stats.Movement * 2;
        List<Tile> movePath = _unit.Tile.GetMovementCost(tile, maxDist);
        UnitAction unitAction = FindActionOfType(typeof(UnitActionMove));

        // If we don't have enough AP to move, return null
        if (unitAction.ActionCost > Stats.ActionPointsCurrent)
            return null;

        // If path is too far for a single movement, return partial path
        if (movePath.Count > Stats.Movement)
            movePath = movePath.GetRange(0, Stats.Movement);

        return movePath;
    }

    public void ItemAction(Item item, Unit target)
    {
        if (item.immuneUnitTypes.Contains(target.Attributes.UnitType))
        {
            Debug.Log("Units of this type are immune to the effect of this item.");
            return;
        }

        item.UseItem(_unit, target);
        _unit.SpendActionPoints(item.itemAction.ActionCost);
        _unit.transform.LookAt(target.transform);
    }

    public void ItemAction(Item item, Tile targetTile)
    {
        _unit.SpendActionPoints(item.itemAction.ActionCost);
        item.UseItem(_unit, targetTile.transform.position);
        _unit.transform.LookAt(targetTile.transform);
    }

    public void Say(string dialog)
    {
        if (!_unit.Attributes.UnitPortrait)
        {
            Debug.Log("Unit has \"Talk\" action but no portrait assigned.");
            return;
        }

        UIManager.GetDialogPanel().UnitTalk(_unit, dialog);
    }
}

public class MoveData
{
    public List<Tile> Path;
    public Tile Immediate;
    public Tile Destination;
    public Vector3 Velocity = new();

    public void SetPath(List<Tile> path)
    { Path = path; }

    public void SetDestination(Tile destination)
    { Destination = destination; }
}
