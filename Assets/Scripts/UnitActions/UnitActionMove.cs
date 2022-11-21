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

        // If tile is unreachable, abort move action
        if (!IsTilePathable(tile))
            return;

        _movePath = Unit.GetMovePath(tile);
        
        if (_movePath.Count > 0)
        {
            Unit.SpendActionPoints(ActionCost);
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
        if (ActionStage(0))
        {
            MoveData.SetDestination(_movePath[^1]);
            Unit.Tile.Occupant = null;
            Unit.SetAnimatorBool("moving", true);
            NextStage();
        }

        // Stage 1 -- Progress through tiles in path until destination is reached
        else if (ActionStage(1))
        {
            MoveData.Immediate = _movePath[_moveCount];
            CheckForObstacle();

            // If we've arrived as the next tile in path, proceed to the next time
            if (Map.MapGrid.GetTile(Unit.transform.position) == MoveData.Immediate)
                _moveCount += 1;

            // If we are at destination, set occupant and wrap-up the action
            if (Map.MapGrid.GetTile(Unit.transform.position) == _movePath[^1])
            {
                _movePath[^1].Occupant = Unit;
                NextStage();
            }
        }

        // Stage 2 -- Allow unit to reach stand position, and then set tile attribute
        else if (ActionStage(2))
        {
            if (Vector3.Distance(Unit.transform.position, MoveData.Immediate.StandPoint) <= 0.01)
            {
                Unit.Tile = MoveData.Immediate;
                NextStage();
            }
        }

        // Stage 4 -- Clean-up and end performance
        else if (ActionStage(3))
        {
            MoveData.Immediate = null;
            MoveData.Destination.HighlightTile(showHighlight: false);
            MoveData.SetDestination(null);
            Unit.SetAnimatorBool("moving", false);
            EndPerformance();
        }
    }

    public bool IsTilePathable(Tile tile)
    {
        // Returns true/false is destination is pathable

        List<Tile> movePath = Unit.Tile.GetMovementCost(tile);

        // If tile is unreachable, return false
        if (movePath.Count == 0 || !tile.IsTraversable || tile.Occupant)
        {
            _movePath = null;
            return false;
        }

        return true;
    }

    public void CheckForObstacle()
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
}
