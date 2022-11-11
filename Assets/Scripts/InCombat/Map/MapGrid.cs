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

    public GameObject indicatorAOEPrefab;
    public GameObject emptyTile;

    private GameObject emptyTileParentGO;
    
    void Awake() 
    {
        List<Tile> tiles = Map.FindTiles();
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        if (tiles.Count == 0) Debug.LogError("No objects with tag 'Tile' found!");

        foreach (Tile tile in tiles)
        {
            //Get position of tile
            Vector3 pos = tile.transform.position;
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

        foreach (Tile tile in tiles)
        {
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
            GameObject go = grid[flattened_xz].gameObject;
            grid[flattened_xz] = null;
            Destroy(go);
        }

        grid[flattened_xz] = tile;
        tile.Grid = this;
        
    }

    public Vector3 GetPositionFromIndex(int flattened_xz)
    {
        float z = Mathf.Floor(((float)flattened_xz) / width);
        float x = flattened_xz - (z * width);

        Vector3 pos = new Vector3((x + gridOffsetX) * tileSpacing, 0.0f, (z + gridOffsetZ) * tileSpacing);
        return pos;
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

    public List<Tile> GetTilesInRange(Vector3 startPos, int tileDist)
    {
        List<Tile> tilesInRange = new List<Tile>();
        tilesInRange.Add(GetTile(startPos));

        // Reset path for tiles
        foreach (Tile tile in grid)
            tile.NearestTile = null;

        // Prevent finding our current tile again
        Tile currentTile = tilesInRange[0];
        List<Tile> foundTiles = tilesInRange;

        // Begin iterating through nearby tiles
        for (int currentIteration = 0; currentIteration < tileDist; currentIteration++)
        {
            List<Tile> nextTiles = new List<Tile>();
            foreach (Tile tile in new List<Tile>(foundTiles))
            {
                // Ensures tiles only use the closest path if found by multiple tiles
                if (tile.NearestTile == null)
                {
                    tile.NearestTile = currentTile;
                }

                // If tile path is obstructed, remove it from the list and allow it to be found by alternative paths
                if (tile.NearestTile == currentTile && IsTilePathObstructed(currentTile, tile))
                {
                    tile.NearestTile = null;
                    continue;
                }

                // Expands to next row of neighboring tiles, and returns path if destination tile is found

                foreach (Tile tile2 in tile.AdjacentTiles)
                {
                    if (!tile2.IsTraversable)
                        continue;
                    if (IsTilePathObstructed(tile, tile2))
                        continue;
                    if (tile2.NearestTile == null)
                        tile2.NearestTile = tile;
                    if (!nextTiles.Contains(tile2) && !tilesInRange.Contains(tile2))
                        nextTiles.Add(tile2);
                }
            }
            tilesInRange.AddRange(nextTiles);
            foundTiles = nextTiles;
        }
        return tilesInRange;
    }

    public bool IsTilePathObstructed(Tile tileStart, Tile tileDest)
    {
        // Returns True/False if any obstructions are blocking the path

        // Trivial check
        if (tileStart == tileDest) return false;

        // First, check if both tiles have any cover object
        if (!tileStart.Cover || !tileDest.Cover)
            return false;

        // Then, check if the cover object is full sized or vaultable
        if (tileDest.Cover.CoverSize != CoverSizes.full && tileDest.Cover.IsVaultable)
            return false;

        // Check if both tiles share the same cover object
        if (tileStart.Cover != tileDest.Cover)
            return false;

        // Check if the obstructing cover object has been destroyed
        if (tileStart.Cover.IsDestroyed)
            return false;

        return true;
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

    public bool CheckIfCovered(Tile attackerTile, Tile defenderTile)
    {
        // Checks if any cover objects are between character and attacker
        // Does raycast from character to attacker in order to find closest potential cover object

        // NOTE -- We use the tiles for raycast, not the characters or weapons
        // This is to prevent animations or standpoints from impacting the calculation

        //TODO: Rework this to iterate through tiles, similar to weapon line of sight logic

        Vector3 defenderPosition = defenderTile.transform.position;
        Vector3 attackerPosition = attackerTile.transform.position;

        Vector3 direction = (attackerPosition - defenderPosition);
        Ray ray = new(defenderPosition, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));

        // If cover object detected, and is the target character's current cover, return true
        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude * Mathf.Infinity, layerMask))
        {
            CoverObject coverObject = hit.collider.GetComponentInParent<CoverObject>();

            if (!coverObject)
                return false;

            if (coverObject.IsCoverInUse(defenderTile.Cover))
                return true;
        }
        return false;
    }

    public GameObject InstantiateIndicatorAOE(Vector3 position)
    {
        return Instantiate(indicatorAOEPrefab, position, Quaternion.identity, transform);
    }

    public void DestroyIndicatorAOE(GameObject indicatorAOE)
    {
        Destroy(indicatorAOE);
    }
}