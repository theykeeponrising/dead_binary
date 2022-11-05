using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public MapGrid mapGrid;

    public static MapEffects MapEffects;
    public static Tilemap DetailMap;
    public static Tilemap TileMap;
    public static Tilemap CoverMap;
    public static Tilemap UnitMap;

    private static List<Tile> _tileMapResults;
    private static List<CoverObject> _coverMapResults;

    private void Awake()
    {
        MapEffects = GetComponent<MapEffects>();
        mapGrid = GetComponentInChildren<MapGrid>();
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();

        DetailMap = tilemaps[0];
        TileMap = tilemaps[1];
        CoverMap = tilemaps[2];
        UnitMap = tilemaps[3];

        _tileMapResults = TileMap.GetComponentsInChildren<Tile>().ToList();
        _coverMapResults = CoverMap.GetComponentsInChildren<CoverObject>().ToList();
    }

    public static List<Unit> FindUnits(Faction faction = null)
    {
        // Returns units on the active map filtered by faction
        // If no faction is provided, returns all units

        List<Unit> unitsFound = new List<Unit>();

        if (faction != null)
        {
            foreach (Unit unit in UnitMap.GetComponentsInChildren<Unit>())
                if (unit.attributes.faction == faction)
                    unitsFound.Add(unit);
        }
        else
            unitsFound = UnitMap.GetComponentsInChildren<Unit>().ToList();

        return unitsFound;
    }

    public static List<Tile> FindTiles()
    {
        // Returns all tiles on the active map

        return _tileMapResults;
    }

    public static List<CoverObject> FindCoverObjects()
    {
        // Returns all tiles on the active map

        return _coverMapResults;
    }

    public GameObject CreateEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return MapEffects.CreateEffect(efxPrefab, efxPosition, efxRotation);
    }

    public GameObject CreateTimedEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation, float timer)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return MapEffects.CreateTimedEffect(efxPrefab, efxPosition, efxRotation, timer);
    }

    public void AddEffect(GameObject efxPrefab)
    {
        // Add an existing effect to the effects list

        MapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(Transform efxPrefab)
    {
        // Add an existing effect to the effects list

        MapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(ItemProp efxPrefab)
    {
        // Add an existing effect to the effects list

        MapEffects.AddEffect(efxPrefab.transform);
    }

    public static void ClearTileHighlights()
    {
        // Removes all tile highlights

        foreach (Tile tile in _tileMapResults)
            tile.HighlightTile(showHighlight:false);
    }
}
