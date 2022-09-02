using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid : MonoBehaviour
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
    public List<Tile> GetTilesInRange(Vector3 startPos, int tileDist)
    {
        List<Tile> tilesInRange = new List<Tile>();
        startPos = NormalizePositionToGrid(startPos);
        for (int i = 0; i < tileDist; i++)
        {
            for (int j = i; j < tileDist; j++)
            {
                Vector3 nextPos = startPos + new Vector3((float) i, 0.0f, (float) j);
                int flattened_index = GetFlattenedIndex(nextPos);

                //Ignore any indices that would be out of bounds
                if (flattened_index >= 0)
                {
                    Tile nextTile = grid[flattened_index];
                    tilesInRange.Add(nextTile);
                }
            }
        }
        return tilesInRange;
    }

    //Path distance between two tiles (Note: Not straight-line distance!)
    public int GetTileDistance(Tile a, Tile b)
    {
        Vector3 pos_a = a.gameObject.transform.position;
        Vector3 pos_b = b.gameObject.transform.position;
        Vector3 diff = pos_a - pos_b;

        //Note: Not straight line distance
        float dist = diff.x + diff.z;
        return Mathf.RoundToInt(dist / tileSpacing);
    }
}