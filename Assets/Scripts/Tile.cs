using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Main script for Tile behavior, such as pathing and cover objects

    public List<Tile> neighbours;
    public BoxCollider[] boxColliders;
    public Tile nearestTile;
    Color tileColor;
    [HideInInspector]
    public Material tileGlow;
    public GridObject occupant;
    public Cover cover;
    [HideInInspector]
    public Vector3 standPoint;
    InCombatPlayerAction playerAction;

    // Use this for initialization
    void Start()
    {
        playerAction = GameObject.FindGameObjectWithTag("Player").GetComponent<InCombatPlayerAction>();
        Renderer renderer = this.gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
        tileGlow = renderer.materials[1];
        tileColor = tileGlow.color;
        boxColliders = gameObject.GetComponents<BoxCollider>();
        cover = GetComponentInChildren<Cover>();
        if (cover)
            standPoint = cover.standPoint;
        else
            standPoint = transform.position;
        FindNeighbours();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Updates targeted tile on mouse over
        playerAction.targetTile = this;
        Highlighted(true, "preview");
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears targeted tile on mouse leave if currently target
        if (playerAction.targetTile == this)
            playerAction.targetTile = null;
        Highlighted(false, "preview");
    }

    public void Highlighted(bool highlighted = true, string highlightType = "error", string eval = "movement")
    {
        // Changes tile highlight based on context

        if (highlighted)
        {
            if (highlightType == "preview")
            {
                if (eval == "movement" && !occupant)
                    tileGlow.color = new Color(0, 0, 150, 0.50f);
                else
                    tileGlow.color = new Color(150, 0, 0, 0.50f);
            }
            else if (highlightType == "moving")
                tileGlow.color = new Color(0, 150, 0, 0.50f);
            else if (highlightType == "error")
                tileGlow.color = Color.red;
        }
        else
            tileGlow.color = tileColor;
    }

    void FindNeighbours()
    {
        // Gets all immediate vertical and horizontal neighbors

        Tile[] tiles = FindObjectsOfType<Tile>();

        foreach (Tile tile in tiles)
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, tile.gameObject.transform.position);
                if (distance <= GlobalManager.tileSpacing) neighbours.Add(tile);
            }
    }

    Tile GetNeighbor(bool north = false, bool south = false, bool east = false, bool west = false)
    {
        // Used to find specific directional tiles

        foreach (Tile tile in neighbours)
        {
            if ((tile.transform.position.z > transform.position.z) == north && (tile.transform.position.z < transform.position.z) == south && (tile.transform.position.x > transform.position.x) == east && (tile.transform.position.x < transform.position.x) == west)
                return tile;
        }
        return null;
    }

    //TODO: Simplify this to just calculate distance using GlobalManager.tileSpacing and the transform.positions of the tiles
    public List<Tile> FindCost(Tile findTile, int maxDist = 10)
    {
        // Finds the nearest path to the destination tile
        // Returns path of tiles in ordered list format

        // Reset path for tiles
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
            tile.nearestTile = null;

        // If selecting the current tile, abort move action
        if (this == findTile)
            return new List<Tile>();

        // Prevent finding our current tile again
        nearestTile = this;

        // How many search iterations to perform
        int currentIteration = 1;

        // Create a list to keep track of all tiles found during iteration
        List<Tile> foundTiles = new List<Tile>(neighbours);

        // To randomize path movement
        //var rnd = new System.Random();

        // If destination is adjacent to current tile, skip tile find
        if (foundTiles.Contains(findTile))
        {
            findTile.nearestTile = this;
            return FindPath(findTile);
        }

        // Begin iterating through nearby tiles
        while (currentIteration < maxDist)
        {
            currentIteration++;

            List<Tile> nextTiles = new List<Tile>();
            foreach (Tile tile in new List<Tile>(foundTiles))
            {
                // Ensures tiles only use the closest path if found by multiple tiles
                if (tile.nearestTile == null)
                    tile.nearestTile = this;

                // Expands to next row of neighboring tiles, and returns path if destination tile is found
                //foreach (Tile tile2 in tile.neighbours.OrderBy(item => rnd.Next()))
                foreach (Tile tile2 in tile.neighbours)
                {
                    if (!tile2.isTileTraversable() && tile2 != findTile)
                        continue;
                    if (tile2.nearestTile == null)
                        tile2.nearestTile = tile;
                    if (!nextTiles.Contains(tile2))
                        nextTiles.Add(tile2);
                    if (tile2 == findTile)
                        return FindPath(tile2);
                }
            }
            foundTiles = nextTiles;
        }
        return new List<Tile>();

    }

    public bool isTileTraversable()
    {
        return !this.occupant || this.occupant.isTraversable;
    }

    private List<Tile> FindPath(Tile reverseTile)
    {
        // Finds the nearest path to tile by checking tiles in reverse
        // Returns path when this script is found

        List<Tile> movePath = new List<Tile>();
        while (reverseTile != this)
        {
            movePath.Insert(0, reverseTile);
            reverseTile = reverseTile.nearestTile;
        }
        return movePath;
    }

    public void ChangeTileOccupant(GridObject gridObject, bool occupied = true)
    {
        // Used to change tile occupant.

        if (occupied)
        {
            occupant = gridObject;
        }
        else
            occupant = null;
    }

    public bool CheckIfTileOccupant(GridObject gridObject)
    {
        // True/False if tile is currently occupied by a character
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            Collider gridObjColl = gridObject.GetComponent<Collider>();
            if (gridObjColl && gridObjColl.bounds.Intersects(collider.bounds)) return true;
        }
        return false;
    }
}