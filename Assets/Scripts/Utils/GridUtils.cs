using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class GridUtils
{
    public enum GridDirection { LEFT, RIGHT, UP, DOWN };
    public static List<Tile> GetStraightPath(Tile startTile, Tile endTile)
    {
        Vector3 centerOffset = new Vector3(MapGrid.TileSpacing / 2.0f, 0.0f, MapGrid.TileSpacing / -2.0f);
        Vector3 startPos = startTile.Position + centerOffset;
        Vector3 endPos = endTile.Position + centerOffset;
        
        if (startTile == endTile) return new List<Tile>();
        Vector3 straightLineDirection = endPos - startPos;
        // Split the straight line into x and z components
        int x = (int) straightLineDirection.x;
        int z = (int) straightLineDirection.z;

        Vector2 lineNormal = Vector2.Perpendicular(new Vector2(straightLineDirection.x, straightLineDirection.z));
        lineNormal.Normalize();

        GridDirection xDirection = x > 0 ? GridDirection.RIGHT : GridDirection.LEFT;
        GridDirection zDirection = z > 0 ? GridDirection.UP : GridDirection.DOWN;

        Tile currentTile = startTile;

        //Update the lastLinePosition at each iteration, to ensure that long paths don't have too much floating point divergeance
        Vector3 lastLinePosition = startPos;

        List<Tile> path = new List<Tile> { currentTile };

        //Choose the next tile that minimizes the straight-line divergence
        for (int i = 0; i < Mathf.Abs(x / MapGrid.TileSpacing) + Mathf.Abs(z / MapGrid.TileSpacing); i++)
        {
            Tile zDirTile = GetTileInDirection(currentTile, zDirection);
            Tile xDirTile = GetTileInDirection(currentTile, xDirection);
            Vector3 xDirTilePos = xDirTile.Position + centerOffset;
            Vector3 zDirTilePos = zDirTile.Position + centerOffset;

            if (zDirTile == null || xDirTile == null) 
            {
                throw new System.Exception("No tile found when checking straight line path!");
            }

            // Compare divergence from line normal of x and z-dir tiles
            Vector2 xDirTileGridPos = new Vector2(xDirTilePos.x - lastLinePosition.x, xDirTilePos.z - lastLinePosition.z);
            float xDivergence = Vector2.Dot(xDirTileGridPos, lineNormal);

            Vector2 zDirTileGridPos = new Vector2(zDirTilePos.x - lastLinePosition.x, zDirTilePos.z - lastLinePosition.z);
            float zDivergence = Vector2.Dot(zDirTileGridPos, lineNormal);

            currentTile = Mathf.Abs(xDivergence) < Mathf.Abs(zDivergence) ? xDirTile : zDirTile;
            path.Add(currentTile);
            if (currentTile == endTile) 
            {
                break;
            }
        }
        return path;
    }

    public static GridDirection GetNeighborDirection(Tile tile, Tile neighborTile)
    {
        Vector3 direction = neighborTile.Position - tile.Position;

        if (direction.x > 0) return GridDirection.RIGHT;
        else if (direction.x < 0) return GridDirection.LEFT;
        else if (direction.z > 0) return GridDirection.UP;
        else return GridDirection.DOWN;
    }

    public static Tile GetTileInDirection(Tile tile, GridDirection direction)
    {
        List<Tile> neighbors = tile.AdjacentTiles;
        foreach (Tile neighborTile in neighbors)
        {
            GridDirection neighborDirection = GetNeighborDirection(tile, neighborTile);
            if (neighborDirection == direction) return neighborTile;
        }
        return null;
    }
}