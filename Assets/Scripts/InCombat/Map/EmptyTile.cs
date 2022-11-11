using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyTile : Tile
{
    protected override void Start() {}

    public override bool CheckIfTileOccupant(GridObject gridObj)
    {
        return false;
    }

    public override List<Tile> GetMovementCost(Tile findTile, int maxDist = 10)
    {
        return new List<Tile>();   
    }

    public override void HighlightTile(TileHighlightType highlightType = TileHighlightType.ERROR, bool showHighlight = true)
    {
        return;
    }
    
}