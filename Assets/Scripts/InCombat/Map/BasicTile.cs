using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BasicTile : Tile, IPointerEnterHandler, IPointerExitHandler
{
    // Visual and sound effects
    protected Material _tileGlowMaterial;
    protected Color _tileDefaultColor;
    protected const float _colorIncrement = 1.0f / 255.0f;
    protected const float _selectionCircleColorIntensity = 120.0f;
    
    protected override void Start()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        _playerAction = playerTurnState.GetPlayerAction();
        
        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
        _tileGlowMaterial = renderer.materials[1];
        _tileDefaultColor = _tileGlowMaterial.color;

        FindAdjacentTiles();
        FindCoverObjects();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlight targeted tile on mouse over

        _playerAction.targetTile = this;

        //HighlightTile(TileHighlightType.PREVIEW, true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears tile highlight on mouse leave if currently target

        if (_playerAction.targetTile == this)
            _playerAction.targetTile = null;

        //HighlightTile(TileHighlightType.PREVIEW, false);
    }

    public override bool CheckIfTileOccupant(GridObject gridObj)
    {
        // True/False if tile is currently occupied by a character
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            Collider gridObjColl = gridObj.GetComponent<Collider>();
            if (gridObjColl && gridObjColl.bounds.Intersects(collider.bounds)) return true;
        }
        return false;
    }

    

    public override void HighlightTile(TileHighlightType highlightType = TileHighlightType.ERROR, bool showHighlight = true)
    {
        // Changes tile highlight based on context

        Color newColor = _tileDefaultColor;

        if (showHighlight)
        {
            switch (highlightType)
            {
                case (TileHighlightType.PREVIEW):
                    if (!_occupant)
                        newColor = new Color(0, 0, _colorIncrement, 1.0f) * _selectionCircleColorIntensity;
                    else
                        newColor = new Color(_colorIncrement, 0, 0, 1.0f) * _selectionCircleColorIntensity;
                    break;
                case (TileHighlightType.MOVEMENT):
                    newColor = new Color(0, _colorIncrement, 0, 1.0f) * _selectionCircleColorIntensity;
                    break;
                case (TileHighlightType.ERROR):
                    newColor = Color.red;
                    break;
            }
        }
        
        _tileGlowMaterial.color = newColor;
    }

    public override List<Tile> GetMovementCost(Tile findTile, int maxDist = 100)
    {
        // Finds the nearest path to the destination tile
        // Returns path of tiles in ordered list format

        // Reset path for tiles
        List<Tile> tiles = Map.FindTiles();
        foreach (Tile tile in tiles)
            tile.NearestTile = null;

        // If selecting the current tile, abort move action
        if (this == findTile)
            return new List<Tile>();

        // Prevent finding our current tile again
        NearestTile = this;

        // How many search iterations to perform
        int currentIteration = 0;

        // Create a list to keep track of all tiles found during iteration
        List<Tile> foundTiles = new() { this };

        // To randomize path movement
        //var rnd = new System.Random();

        // Begin iterating through nearby tiles
        while (currentIteration < maxDist)
        {
            currentIteration++;

            List<Tile> nextTiles = new();
            foreach (Tile tile in new List<Tile>(foundTiles))
            {
                // Ensures tiles only use the closest path if found by multiple tiles
                if (tile.NearestTile == null)
                {
                    tile.NearestTile = this;
                }

                // If tile path is obstructed, remove it from the list and allow it to be found by alternative paths
                if (tile.NearestTile == this && _grid.IsTilePathObstructed(this, tile))
                {
                    tile.NearestTile = null;
                    continue;
                }

                // Expands to next row of neighboring tiles, and returns path if destination tile is found

                foreach (Tile tile2 in tile.AdjacentTiles)
                {
                    if (!tile2.IsTraversable && tile2 != findTile)
                        continue;
                    if (_grid.IsTilePathObstructed(tile, tile2))
                        continue;
                    if (tile2.NearestTile == null)
                        tile2.NearestTile = tile;
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
            reverseTile = reverseTile.NearestTile;
        }
        return movePath;
    }
}