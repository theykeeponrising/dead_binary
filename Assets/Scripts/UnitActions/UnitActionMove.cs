using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionMove : UnitAction
{
    List<Tile> movePath;
    int moveCount;

    public override void UseAction(Tile tile)
    {
        // Sets units move data and updates as unit progresses

        if (unit.GetActor().IsActing())
            return;

        movePath = unit.GetActor().moveData.path;
        moveCount = 0;

        // If tile is unreachable, abort move action
        if (!CheckTileMove(tile))
            return;

        if ((CheckTileMove(tile)))
        {
            // If tile is occupied, we can't move there
            if (tile.Occupant)
                movePath = null;
            else movePath = unit.currentTile.GetMovementCost(tile, unit.stats.movement);

            if (movePath.Count > 0)
            {
                unit.SpendActionPoints(actionCost);
                if (unit.GetAnimator().IsCrouching()) unit.GetAnimator().ToggleCrouch();
                StartPerformance();
            }
        }
    }

    public override void CheckAction()
    {
        // Waits until unit stands
        while (unit.GetAnimator().IsCrouching())
            return;

        // Stage 0 -- Set the immediate destination and animation flag
        if (ActionStage(0))
        {
            unit.GetActor().moveData.destination = movePath[movePath.Count - 1];
            unit.currentTile.Occupant = null;
            unit.GetAnimator().SetBool("moving", true);
            NextStage();
        }

        // Stage 1 -- Progress through tiles in path until destination is reached
        else if (ActionStage(1))
        {
            unit.GetActor().moveData.immediate = movePath[moveCount];
            unit.GetActor().CheckForObstacle();

            // If we've arrived as the next tile in path, proceed to the next time
            if (unit.grid.GetTile(unit.transform.position) == unit.GetActor().moveData.immediate)
                moveCount += 1;

            // If we are at destination, set occupant and wrap-up the action
            if (unit.grid.GetTile(unit.transform.position) == movePath[movePath.Count - 1])
            {
                movePath[movePath.Count - 1].Occupant = unit;
                NextStage();
            }
        }

        // Stage 2 -- Allow unit to reach stand position, and then set tile attribute
        else if (ActionStage(2))
        {
            if (Vector3.Distance(unit.transform.position, unit.GetActor().moveData.immediate.StandPoint) <= 0.01)
            {
                unit.currentTile = unit.GetActor().moveData.immediate;
                NextStage();
            }
        }

        // Stage 4 -- Clean-up and end performance
        else if (ActionStage(3))
        {
            unit.GetActor().moveData.immediate = null;
            unit.GetActor().moveData.destination.HighlightTile(showHighlight: false);
            unit.GetActor().moveData.destination = null;
            unit.GetAnimator().SetBool("moving", false);
            EndPerformance();
        }
    }

    public bool CheckTileMove(Tile newTile)
    {
        // Gets the shortest tile distance to target and compares to maximum allowed moves
        // If destination is too far, abort move action


        movePath = unit.currentTile.GetMovementCost(newTile);
        if (movePath.Count == 0 || !newTile.IsTraversable)
        {
            Debug.Log("No move path."); // Replace this with UI eventually
            return false;
        }
        if (movePath.Count > unit.stats.movement)
        {
            Debug.Log(string.Format("Destination Too Far! \nDistance: {0}, Max Moves: {1}", movePath.Count, unit.stats.movement)); // This will eventually be shown visually instead of told
            return false;
        }
        return true;
    }
}
