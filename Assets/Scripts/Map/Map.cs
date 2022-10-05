using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    MapEffects mapEffects;
    public MapGrid mapGrid;
    public Tilemap detailMap;
    public Tilemap tileMap;
    public Tilemap coverMap;
    public Tilemap unitMap;

    void Awake()
    {
        mapEffects = GetComponent<MapEffects>();
        mapGrid = GetComponentInChildren<MapGrid>();
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();

        detailMap = tilemaps[0];
        tileMap = tilemaps[1];
        coverMap = tilemaps[2];
        unitMap = tilemaps[3];
    }

    public List<Unit> FindUnits(Faction faction = Faction.Any)
    {
        // Returns units on the active map filtered by faction
        // If no faction is provided, returns all units

        List<Unit> unitsFound = new List<Unit>();

        if (faction != Faction.Any)
        {
            foreach (Unit unit in unitMap.GetComponentsInChildren<Unit>())
                if (unit.attributes.faction == faction)
                    unitsFound.Add(unit);
        }
        else
            unitsFound = unitMap.GetComponentsInChildren<Unit>().ToList();

        return unitsFound;
    }

    public List<Tile> FindTiles()
    {
        // Returns all tiles on the active map

        return tileMap.GetComponentsInChildren<Tile>().ToList();
    }

    public GameObject CreateEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return mapEffects.CreateEffect(efxPrefab, efxPosition, efxRotation);
    }

    public GameObject CreateTimedEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation, float timer)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return mapEffects.CreateTimedEffect(efxPrefab, efxPosition, efxRotation, timer);
    }

    public void AddEffect(GameObject efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(Transform efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(ItemProp efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }
}
