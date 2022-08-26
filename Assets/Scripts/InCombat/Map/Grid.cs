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
    
    void Start() 
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

        gridOffsetX = minX;
        gridOffsetZ = minZ;

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
        Vector3 pos = tile.gameObject.transform.position;
        int flattened_xz = GetFlattenedIndex(pos.x, pos.z);
        string fs = string.Format("Position {0}, {1}, Index: {2}", pos.x, pos.z, flattened_xz);
        Debug.Log(fs);
        if (grid[flattened_xz] != null)
        {
            string s = string.Format("Tile at position {0}, {1} already exists! Index: {2}", pos.x, pos.z, flattened_xz);
            Debug.LogError(s, tile);
            Tile otherTile = grid[flattened_xz];
            Vector3 pos2 = otherTile.gameObject.transform.position;
            int otherx = Mathf.RoundToInt(pos2.x);
            int otherz = Mathf.RoundToInt(pos2.z);
            string f = string.Format("Other tile is at position {0}, {1}", otherx, otherz);
            string f2 = string.Format("GridOffsetX: {0}, GridOffsetY: {1}", gridOffsetX, gridOffsetZ);
            Debug.Log(f);
            Debug.Log(f2);
        } else {
            grid[flattened_xz] = tile;
        }
    }

    //Convert the 2-D index to a 1-D flattened array index
    public int GetFlattenedIndex(float x, float z)
    {
        int zLoc = Mathf.RoundToInt(((float) (z - gridOffsetZ) / tileSpacing));
        int xLoc = Mathf.RoundToInt(((float) (x - gridOffsetX) / tileSpacing));
        return zLoc * width + xLoc;
    }

    //Get the tile at a particular index
    public Tile GetTile(float x, float z)
    {
        return grid[GetFlattenedIndex(x, z)];
    }
}