using System.Collections.Generic;
using UnityEngine;

public class UnitActionMove : UnitAction
{
    private List<Tile> _movePath;
    private int _moveCount;
    private MoveData MoveData => Unit.MoveData;

    private readonly Timer _animationBuffer = new();

    public override void UseAction(Tile tile)
    {
        // Sets units move data and updates as unit progresses

        if (Unit.IsActing())
            return;

        _movePath = Unit.MoveData.Path;
        _moveCount = 0;

        // If tile is unreachable while patrolling, find nearby tile instead
        if (Unit.IsPatrolling() && !IsTilePathable(tile))
        {
            tile = tile.GetNearestOpenTile();
        }

        // If tile is unreachable, abort move action
        else if (!IsTilePathable(tile))
        {
            Debug.Log(string.Format("Tile is unreachable! {0}", tile.name));
            return;
        }

        _movePath = Unit.GetMovePath(tile);
        
        if (_movePath.Count > 0)
        {
            Unit.SpendActionPoints(ActionCost);
            _movePath[^1].Occupant = Unit;
            if (Unit.IsCrouching()) Unit.ToggleCrouch();
            StartPerformance();
        }
        else
        {
            Debug.Log("Move path is zero!");
        }
    }

    public override void CheckAction()
    {
        // Waits until unit stands
        while (Unit.IsCrouching())
            return;

        // Stage 0 -- Set the immediate destination and animation flag
        if (IsActionStage(0))
        {
            MoveData.SetDestination(_movePath[^1]);
            Unit.objectTile.Occupant = null;
            Unit.SetAnimatorBool("moving", true);
            NextActionStage();
        }

        // Stage 1 -- Progress through tiles in path until destination is reached
        else if (IsActionStage(1))
        {
            MoveData.Immediate = _movePath[_moveCount];
            CheckForObstacle();
            CheckAllUnitSight();

            // If we've arrived as the next tile in path, proceed to the next time
            if (IsDestinationReached(MoveData.Immediate))
                _moveCount += 1;

            // If we are at destination, set occupant and wrap-up the action
            if (IsDestinationReached(_movePath[^1]))
            {
                _movePath[^1].Occupant = Unit;
                NextActionStage();
            }
        }

        // Stage 2 -- Allow unit to reach stand position, and then set tile attribute
        else if (IsActionStage(2))
        {
            if (Vector3.Distance(Unit.transform.position, MoveData.Immediate.StandPoint) <= 0.01)
            {
                Unit.objectTile = MoveData.Immediate;
                NextActionStage();
            }
        }

        // Stage 3 -- Clean-up and end performance
        else if (IsActionStage(3))
        {
            MoveData.Immediate = null;
            MoveData.Destination.HighlightTile(showHighlight: false);
            MoveData.SetDestination(null);
            Unit.CheckSight();
            Unit.SetAnimatorBool("moving", false);
            EndPerformance();
        }
    }

    public bool IsTilePathable(Tile tile)
    {
        // Returns true/false is destination is pathable

        List<Tile> movePath = Unit.objectTile.GetMovementCost(tile);

        // If tile is unreachable, return false
        if (movePath.Count == 0 || !tile.IsTraversable || tile.Occupant)
        {
            _movePath = null;
            return false;
        }

        return true;
    }

    public bool IsDestinationReached(Tile tile)
    {
        return Map.MapGrid.GetTile(Unit.transform.position) == tile;
    }

    private void CheckForObstacle()
    {
        // Checks a short distance in front of character for objects in the "VaultOver" layer

        if (Unit.IsVaulting())
            return;

        Vector3 direction = (MoveData.Immediate.transform.position - Unit.transform.position);
        Ray ray = new(Unit.transform.position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));
        float distance = 0.75f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude * distance, layerMask))
        {
            if (hit.collider.GetComponentInParent<CoverObject>().IsVaultable)
            {
                if (_animationBuffer.CheckTimer())
                {
                    Unit.SetAnimatorTrigger("vaulting");
                    _animationBuffer.SetTimer(0.25f);
                }
                return;
            }
        }
    }

    private void CheckAllUnitSight()
    {
        List<Unit> units = Map.FindUnits();
        foreach (Unit unit in units)
        {
            if (unit == Unit) continue;
            unit.CheckSight();
        }
    }
}
