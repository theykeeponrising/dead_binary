using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const float colorIncrement = 1.0f/255.0f;

    // Main script for Tile behavior, such as pathing and cover objects

    private InCombatPlayerAction _playerAction;
    private MapGrid _grid;

    // Grid objects
    private GridObject _occupant;
    private CoverObject _cover;
    private Vector3 _standPoint;

    // Pathing
    private List<Tile> _adjacentTiles = new();
    private Tile _nearestTile;

    // Visual and sound effects
    private Material _tileGlowMaterial;
    private Color _tileDefaultColor;
    [SerializeField] private ImpactTypes _impactType;


    public MapGrid Grid { get { return _grid; } set { _grid = value; } }
    public List<Tile> AdjacentTiles { get { return _adjacentTiles; } }
    public Tile NearestTile { get { return _nearestTile; } set { _nearestTile = value; } }
    public GridObject Occupant { get { return _occupant; } set { _occupant = value; } }
    public CoverObject Cover { get { return _cover; } set { _cover = value; } }
    public Vector3 StandPoint { get { return _standPoint; } }
    public ImpactTypes ImpactType { get { return _impactType; } }
    public bool IsTraversable => !_occupant || _occupant.isTraversable;

    
    public float selectionCircleColorIntensity = 120.0f;
   
    // Use this for initialization
    void Start()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        _playerAction = playerTurnState.GetPlayerAction();
        
        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
        _tileGlowMaterial = renderer.materials[1];
        _tileDefaultColor = _tileGlowMaterial.color;

        FindAdjacentTiles();
        FindCoverObjects();
        GetStandPoint();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Updates targeted tile on mouse over

        _playerAction.targetTile = this;

        HighlightTile(TileHighlightType.PREVIEW, true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears targeted tile on mouse leave if currently target

        if (_playerAction.targetTile == this)
            _playerAction.targetTile = null;

        HighlightTile(TileHighlightType.PREVIEW, false);
    }

    public void HighlightTile(TileHighlightType highlightType = TileHighlightType.ERROR, bool showHighlight = true, string eval = "movement")
    {
        // Changes tile highlight based on context

        if (showHighlight)
        {
            switch (highlightType)
            {
                case (TileHighlightType.PREVIEW):
                    if (eval == "movement" && !_occupant)
                        _tileGlowMaterial.color = new Color(0, 0, colorIncrement, 1.0f) * selectionCircleColorIntensity;
                    else
                        _tileGlowMaterial.color = new Color(colorIncrement, 0, 0, 1.0f) * selectionCircleColorIntensity;
                    break;
                case (TileHighlightType.MOVEMENT):
                    _tileGlowMaterial.color = new Color(0, colorIncrement, 0, 1.0f) * selectionCircleColorIntensity;
                    break;
                case (TileHighlightType.ERROR):
                    _tileGlowMaterial.color = Color.red;
                    break;
            }
        }
        else
            _tileGlowMaterial.color = _tileDefaultColor;
    }

    void FindAdjacentTiles()
    {
        // Gets all immediate vertical and horizontal neighbors

        List<Tile> tiles = Map.FindTiles();

        foreach (Tile tile in tiles)
            if (tile != this)
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, tile.gameObject.transform.position);
                if (distance <= MapGrid.tileSpacing) _adjacentTiles.Add(tile);
            }
    }

    //TODO: Move this to grid.cs, and simplify
    public List<Tile> GetMovementCost(Tile findTile, int maxDist = 10)
    {
        // Finds the nearest path to the destination tile
        // Returns path of tiles in ordered list format

        // Reset path for tiles
        List<Tile> tiles = Map.FindTiles();
        foreach (Tile tile in tiles)
            tile._nearestTile = null;

        // If selecting the current tile, abort move action
        if (this == findTile)
            return new List<Tile>();

        // Prevent finding our current tile again
        _nearestTile = this;

        // How many search iterations to perform
        int currentIteration = 0;

        // Create a list to keep track of all tiles found during iteration
        List<Tile> foundTiles = new List<Tile> { this };

        // To randomize path movement
        //var rnd = new System.Random();

        // Begin iterating through nearby tiles
        while (currentIteration < maxDist)
        {
            currentIteration++;

            List<Tile> nextTiles = new List<Tile>();
            foreach (Tile tile in new List<Tile>(foundTiles))
            {
                // Ensures tiles only use the closest path if found by multiple tiles
                if (tile._nearestTile == null)
                {
                    tile._nearestTile = this;
                }

                // If tile path is obstructed, remove it from the list and allow it to be found by alternative paths
                if (tile._nearestTile == this && _grid.IsTilePathObstructed(this, tile))
                {
                    tile._nearestTile = null;
                    continue;
                }

                // Expands to next row of neighboring tiles, and returns path if destination tile is found

                foreach (Tile tile2 in tile.AdjacentTiles)
                {
                    if (!tile2.IsTraversable && tile2 != findTile)
                        continue;
                    if (_grid.IsTilePathObstructed(tile, tile2))
                        continue;
                    if (tile2._nearestTile == null)
                        tile2._nearestTile = tile;
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

    private List<Tile> FindPath(Tile reverseTile)
    {
        // Finds the nearest path to tile by checking tiles in reverse
        // Returns path when this script is found

        List<Tile> movePath = new();
        while (reverseTile != this)
        {
            movePath.Insert(0, reverseTile);
            reverseTile = reverseTile._nearestTile;
        }
        return movePath;
    }

    public bool CheckIfTileOccupant(GridObject gridObj)
    {
        // True/False if tile is currently occupied by a character
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            Collider gridObjColl = gridObj.GetComponent<Collider>();
            if (gridObjColl && gridObjColl.bounds.Intersects(collider.bounds)) return true;
        }
        return false;
    }

    void FindCoverObjects()
    {
        List<CoverObject> coverObjs = Map.FindCoverObjects();

        foreach (CoverObject coverObj in coverObjs)
            if (coverObj.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, coverObj.gameObject.transform.position);
                if (distance <= MapGrid.tileSpacing) 
                {
                    Cover = coverObj;
                    Cover.RegisterTile(this);
                    break;
                }
            }
    }

    void GetStandPoint()
    {
        if (Cover)
            _standPoint = Cover.GetStandPoint(this);
        else
            _standPoint = transform.position;
    }

    public static List<Tile> AreaOfEffect(Tile targetTile, float areaOfEffect)
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
            impactedUnits.Add(tile.Occupant.GetComponent<Unit>());
        }

        return impactedUnits;
    }
}

public enum TileHighlightType { PREVIEW, MOVEMENT, ERROR }
