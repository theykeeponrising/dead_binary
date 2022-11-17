using System.Collections.Generic;
using UnityEngine;

public class UnitActionMove : UnitAction
{
    private List<Tile> _movePath;
    private int _moveCount;
    private MoveData MoveData => unit.MoveData;

    private readonly Timer _animationBuffer = new();

    public override void UseAction(Tile tile)
    {
        // Sets units move data and updates as unit progresses

        if (unit.IsActing())
            return;

        _movePath = unit.MoveData.Path;
        _moveCount = 0;

        // If tile is unreachable, abort move action
        if (!CheckTileMove(tile))
            return;

        if ((CheckTileMove(tile)))
        {
            // If tile is occupied, we can't move there
            if (tile.Occupant)
                _movePath = null;
            else _movePath = unit.Tile.GetMovementCost(tile, unit.Stats.movement);

            if (_movePath.Count > 0)
            {
                unit.SpendActionPoints(actionCost);
                if (unit.IsCrouching()) unit.ToggleCrouch();
                StartPerformance();
            }
        }
    }

    public override void CheckAction()
    {
        // Waits until unit stands
        while (unit.IsCrouching())
            return;

        // Stage 0 -- Set the immediate destination and animation flag
        if (ActionStage(0))
        {
            MoveData.SetDestination(_movePath[^1]);
            unit.Tile.Occupant = null;
            unit.SetAnimatorBool("moving", true);
            NextStage();
        }

        // Stage 1 -- Progress through tiles in path until destination is reached
        else if (ActionStage(1))
        {
            MoveData.immediate = _movePath[_moveCount];
            CheckForObstacle();

            // If we've arrived as the next tile in path, proceed to the next time
            if (Map.MapGrid.GetTile(unit.transform.position) == MoveData.immediate)
                _moveCount += 1;

            // If we are at destination, set occupant and wrap-up the action
            if (Map.MapGrid.GetTile(unit.transform.position) == _movePath[^1])
            {
                _movePath[^1].Occupant = unit;
                NextStage();
            }
        }

        // Stage 2 -- Allow unit to reach stand position, and then set tile attribute
        else if (ActionStage(2))
        {
            if (Vector3.Distance(unit.transform.position, MoveData.immediate.StandPoint) <= 0.01)
            {
                unit.Tile = MoveData.immediate;
                NextStage();
            }
        }

        // Stage 4 -- Clean-up and end performance
        else if (ActionStage(3))
        {
            MoveData.immediate = null;
            MoveData.Destination.HighlightTile(showHighlight: false);
            MoveData.SetDestination(null);
            unit.SetAnimatorBool("moving", false);
            EndPerformance();
        }
    }

    public bool CheckTileMove(Tile newTile)
    {
        // Gets the shortest tile distance to target and compares to maximum allowed moves
        // If destination is too far, abort move action

        _movePath = unit.Tile.GetMovementCost(newTile);
        if (_movePath.Count == 0 || !newTile.IsTraversable)
        {
            Debug.Log("No move path."); // Replace this with UI eventually
            return false;
        }
        if (_movePath.Count > unit.Stats.movement)
        {
            Debug.Log(string.Format("Destination Too Far! \nDistance: {0}, Max Moves: {1}", _movePath.Count, unit.Stats.movement)); // This will eventually be shown visually instead of told
            return false;
        }
        return true;
    }

    public void CheckForObstacle()
    {
        // Checks a short distance in front of character for objects in the "VaultOver" layer

        if (unit.IsVaulting())
            return;

        Vector3 direction = (MoveData.immediate.transform.position - unit.transform.position);
        Ray ray = new(unit.transform.position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));
        float distance = 0.75f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude * distance, layerMask))
        {
            if (hit.collider.GetComponentInParent<CoverObject>().IsVaultable)
            {
                if (_animationBuffer.CheckTimer())
                {
                    unit.SetAnimatorTrigger("vaulting");
                    _animationBuffer.SetTimer(0.25f);
                }
                return;
            }
        }
    }
}
