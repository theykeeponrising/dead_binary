using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    [Header("-GridObject Attributes")]
    [HideInInspector] public List<Tile> objectTiles;
    public bool isTraversable = false;
    public ImpactTypes impactType;
    public Tile Tile;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        this.objectTiles = FindCurrentTiles();
        foreach (Tile tile in objectTiles) tile.Occupant = this;
    }

    protected virtual List<Tile> FindCurrentTiles()
    {
        // Finds the tile the character is currently standing on
        // Called during start

        List<Tile> tiles = Map.FindTiles();
        List<Tile> foundTiles = new();

        foreach (Tile tile in tiles)
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                if (tile.CheckIfTileOccupant(this))
                {
                    foundTiles.Add(tile);
                }
        return foundTiles;
    }

    protected virtual Tile FindCurrentTile()
    {
        // Finds the tile the character is currently standing on
        // Called during start

        List<Tile> tiles = Map.FindTiles();

        foreach (Tile tile in tiles)
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                if (tile.CheckIfTileOccupant(this))
                {
                    return tile;
                }
        return null;
    }
}