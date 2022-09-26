using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class will automatically determine what tiles exist on the board (NOTE: Assumes rectangular map!)
//All cross-tile functions should go here
public class MapGrid : MonoBehaviour
{
    //Grid is a 2d array indexed to the tiles, flattened to 1d, assumes square map
    public Tile[] grid;
    public int width;
    public int height;
    public float gridOffsetX;
    public float gridOffsetZ;

    // Distance between neighboring tiles
    public static float tileSpacing = 2.0f;
    
    void Awake() 
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Tile");
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        if (gameObjects.Length == 0) Debug.LogError("No objects with tag 'Tile' found!");

        foreach (GameObject go in gameObjects)
        {
            Tile tile = go.GetComponent<Tile>();
            //Get position of tile
            Vector3 pos = go.transform.position;
            float x = pos.x;
            float z = pos.z;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }
        width = Mathf.RoundToInt((maxX - minX) / tileSpacing) + 1;
        height = Mathf.RoundToInt((maxZ - minZ) / tileSpacing) + 1;
        Debug.Log(string.Format("Min X: {0}, Max X: {1}, Min Y: {2}, Max Y: {3}", minX, maxX, minZ, maxZ));

        gridOffsetX = minX / tileSpacing;
        gridOffsetZ = minZ / tileSpacing;

        Debug.Log(string.Format("Grid Width: {0}, Grid Height: {1}", width, height));
        grid = new Tile[width * height];

        foreach (GameObject go in gameObjects)
        {
            Tile tile = go.GetComponent<Tile>();
            AddTile(tile);
        }

    }

    public void AddTile(Tile tile) 
    {
        //Get position of tile
        Vector3 pos = NormalizePositionToGrid(tile.gameObject.transform.position);
        int flattened_xz = GetFlattenedIndex(pos);
        if (grid[flattened_xz] != null)
        {
            string s = string.Format("Tile at position {0}, {1} already exists! Index: {2}", pos.x, pos.z, flattened_xz);
            Debug.LogError(s, tile);
        } else {
            grid[flattened_xz] = tile;
            tile.SetGrid(this);
        }
    }

    //Convert the 2-D index to a 1-D flattened array index
    public int GetFlattenedIndex(Vector3 pos)
    {
        int zLoc = Mathf.RoundToInt(pos.z - gridOffsetZ);
        int xLoc = Mathf.RoundToInt(pos.x - gridOffsetX);
        
        //Bounds check
        if (xLoc < 0 || xLoc >= width || zLoc < 0 || zLoc >= height) return -1;
        
        return zLoc * width + xLoc;
    }

    public Vector3 NormalizePositionToGrid(Vector3 pos)
    {
        return pos / tileSpacing;
    } 

    //Get the tile at a particular position
    public Tile GetTile(Vector3 pos)
    {
        pos = NormalizePositionToGrid(pos);
        return grid[GetFlattenedIndex(pos)];
    }

    //Get all tiles within a certain distance of the start tile
    // public List<Tile> GetTilesInRange(Vector3 startPos, int tileDist)
    // {
    //     List<Tile> tilesInRange = new List<Tile>();
    //     startPos = NormalizePositionToGrid(startPos);
    //     for (int i = -tileDist; i <= tileDist; i++)
    //     {
    //         for (int j = -tileDist; j <= tileDist; j++)
    //         {
                
    //             if (Mathf.Abs(i) + Math.Abs(j) > tileDist) continue;
    //             Vector3 nextPos = startPos + new Vector3((float) i, 0.0f, (float) j);
    //             int flattened_index = GetFlattenedIndex(nextPos);
                

    //             //Ignore any indices that would be out of bounds
    //             if (flattened_index >= 0)
    //             {
    //                 Tile nextTile = grid[flattened_index];
    //                 if (nextTile.isTileTraversable()) tilesInRange.Add(nextTile);
    //             }
    //         }
    //     }
    //     return tilesInRange;
    // }

    //Get all tiles within a certain distance of the start tile
    public List<Tile> GetTilesInRange(Vector3 startPos, int tileDist)
    {
        List<Tile> tilesInRange = new List<Tile>();
        tilesInRange.Add(GetTile(startPos));

        // Reset path for tiles
        foreach (Tile tile in grid)
            tile.nearestTile = null;

        // Prevent finding our current tile again
        Tile currentTile = tilesInRange[0];
        List<Tile> foundTiles = tilesInRange;

        // Begin iterating through nearby tiles
        for (int currentIteration = 0; currentIteration < tileDist; currentIteration++)
        {
            List<Tile> nextTiles = new List<Tile>();
            foreach (Tile tile in foundTiles)
            {
                // Ensures tiles only use the closest path if found by multiple tiles
                if (tile.nearestTile == null)
                {
                    tile.nearestTile = currentTile;
                }

                // If tile path is obstructed, remove it from the list and allow it to be found by alternative paths
                if (tile.nearestTile == currentTile && isTilePathObstructed(currentTile, tile))
                {
                    tile.nearestTile = null;
                    continue;
                }

                // Expands to next row of neighboring tiles, and returns path if destination tile is found
                //foreach (Tile tile2 in tile.neighbours.OrderBy(item => rnd.Next()))
                foreach (Tile tile2 in tile.neighbours)
                {
                    if (!tile2.isTileTraversable())
                        continue;
                    if (isTilePathObstructed(tile, tile2))
                        continue;
                    if (tile2.nearestTile == null)
                        tile2.nearestTile = tile;
                    if (!nextTiles.Contains(tile2))
                        nextTiles.Add(tile2);
                }
            }
            tilesInRange.AddRange(nextTiles);
            foundTiles = nextTiles;
        }
        return tilesInRange;
    }

    public bool isTilePathObstructed(Tile tileStart, Tile tileDest)
    {
        // Returns True/False if any obstructions are blocking the path

        // First, check if both tiles have any cover object
        if (!tileStart.cover || !tileDest.cover)
            return false;

        // Then, check if the cover object is full sized or vaultable
        if (tileDest.cover.coverSize != CoverObject.CoverSize.full && tileDest.cover.canVaultOver)
            return false;

        // Lastly, check if both tiles share the same cover object
        return tileStart.cover == tileDest.cover;
    }

    

    //Path distance between two tiles (Note: Not straight-line distance!)
    public int GetTileDistance(Tile a, Tile b)
    {
        Vector3 pos_a = a.gameObject.transform.position;
        Vector3 pos_b = b.gameObject.transform.position;
        Vector3 diff = pos_a - pos_b;

        //Note: Not straight line distance
        float dist = Mathf.Abs(diff.x) + Mathf.Abs(diff.z);
        return Mathf.RoundToInt(dist / tileSpacing);
    }

    public Vector3 GetAverageTileLocation(List<Tile> tiles)
    {
        Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        foreach (Tile tile in tiles)
        {
            pos += tile.gameObject.transform.position / tiles.Count;
        }
        return pos;
    }
}