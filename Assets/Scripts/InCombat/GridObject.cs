using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridObject : MonoBehaviour
{
    [Header("-GridObject Attributes")]
    public List<Tile> objectTiles;
    public bool isTraversable = false;
    public AudioManager.ImpactType impactType;

    protected virtual void Awake()
    {
        this.objectTiles = FindCurrentTiles();
        foreach (Tile tile in objectTiles) tile.ChangeTileOccupant(this, true);
    }

    protected virtual List<Tile> FindCurrentTiles()
    {
        // Finds the tile the character is currently standing on
        // Called during start

        Tile[] tiles = FindObjectsOfType<Tile>();
        List<Tile> foundTiles = new List<Tile>();

        foreach (Tile tile in tiles)
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                if (tile.CheckIfTileOccupant(this))
                {
                    foundTiles.Add(tile);
                }
        return foundTiles;
    }
}