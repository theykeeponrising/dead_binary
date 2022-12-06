using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Tile : MonoBehaviour
{
    // Main script for Tile behavior, such as pathing and cover objects

    protected InCombatPlayerAction _playerAction;
    protected MapGrid _grid;

    // Grid objects
    [SerializeField] protected GridObject _occupant;
    [SerializeField] private CoverObject _cover;

    // Pathing
    protected readonly List<Tile> _adjacentTiles = new();
    protected Tile _nearestTile;

    [SerializeField] private ImpactTypes _impactType;

    public MapGrid Grid { get { return _grid; } set { _grid = value; } }
    public List<Tile> AdjacentTiles { get { return _adjacentTiles; } }
    public Tile NearestTile { get { return _nearestTile; } set { _nearestTile = value; } }
    public GridObject Occupant { get { return _occupant; } set { _occupant = value; } }
    public CoverObject Cover { get { return _cover; } set { _cover = value; } }
    public Vector3 StandPoint => (Cover) ? Cover.GetStandPoint(this) : transform.position;
    public ImpactTypes ImpactType { get { return _impactType; } }
    public bool IsTraversable => !_occupant || _occupant.isTraversable;
       
    protected abstract void Start();

    public abstract void HighlightTile(TileHighlightType highlightType = TileHighlightType.ERROR, bool showHighlight = true);

    protected virtual void FindAdjacentTiles()
    {
        // Gets all immediate vertical and horizontal neighbors

        List<Tile> tiles = Map.FindTiles();

        foreach (Tile tile in tiles)
            if (tile != this)
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, tile.gameObject.transform.position);
                if (distance <= MapGrid.TileSpacing) _adjacentTiles.Add(tile);
            }
    }

    protected virtual void FindCoverObjects()
    {
        List<CoverObject> coverObjs = Map.FindCoverObjects();

        foreach (CoverObject coverObj in coverObjs)
            if (coverObj.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, coverObj.gameObject.transform.position);
                if (distance <= MapGrid.TileSpacing)
                {
                    Cover = coverObj;
                    Cover.RegisterTile(this);
                    break;
                }
            }
    }

    public abstract List<Tile> GetMovementCost(Tile findTile, int maxDist = 100);   
    public abstract bool CheckIfTileOccupant(GridObject gridObj);
    public static List<Tile> GetAreaOfEffect(Tile targetTile, float areaOfEffect)
    {
        // Gets affected tiles from target position based on "areaOfEffect" stat
        // Every odd number of range adds horizontal and vertical neighbor tiles
        // Every even number of range adds diagonal neighbor tiles

        List<Tile> tiles = Map.FindTiles();
        List<Tile> impactedTiles = new();

        impactedTiles.Add(targetTile);

        foreach (Tile tile in tiles)
        {
            float distance = Vector3.Distance(targetTile.transform.position, tile.transform.position);
            if (distance <= areaOfEffect && !impactedTiles.Contains(tile)) impactedTiles.Add(tile);
        }

        return impactedTiles;
    }

    public static List<Unit> GetTileOccupants(List<Tile> areaOfEffect)
    {
        // Gets all valid occupants of a tile list

        List<Unit> impactedUnits = new();

        foreach (Tile tile in areaOfEffect)
        {
            if (!tile.Occupant) continue;
            if (!tile.Occupant.GetComponent<Unit>()) continue;
            if (impactedUnits.Contains(tile.Occupant.GetComponent<Unit>())) continue;
            impactedUnits.Add(tile.Occupant.GetComponent<Unit>());
        }

        return impactedUnits;
    }

    public bool IsPathable(Unit unit)
    {
        // Returns true/false is destination is pathable

        int maxDist = unit.Stats.Movement * 2;
        List<Tile> movePath = unit.Tile.GetMovementCost(this, maxDist);

        // If tile is unreachable, return false
        if (movePath.Count == 0 || !IsTraversable || Occupant)
            return false;
        return true;
    }
}

public enum TileHighlightType { PREVIEW, MOVEMENT, ERROR }
