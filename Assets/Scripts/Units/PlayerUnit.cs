using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Handles all unit logic that is player specific
public class PlayerUnit : Unit
{
    protected override void Awake()
    {
        base.Awake();
    }

    public static List<PlayerUnit> FindPlayerUnits(Faction faction = null, bool excludeDeadUnits = true)
    {
        // Returns enemy units on the active map filtered by faction
        // If no faction is provided, returns all units

        List<PlayerUnit> unitsFound = new();

        // Find by faction
        if (faction != null)
        {
            foreach (PlayerUnit unit in Map.UnitMap.GetComponentsInChildren<PlayerUnit>())
                if (unit.Attributes.Faction == faction)
                    unitsFound.Add(unit);
        }
        else
        {
            unitsFound = Map.UnitMap.GetComponentsInChildren<PlayerUnit>().ToList();
        }

        // Exclude dead units
        if (excludeDeadUnits)
        {
            List<PlayerUnit> iterateList = new(unitsFound);
            foreach (PlayerUnit unit in iterateList)
                if (unit.IsDead())
                    unitsFound.Remove(unit);
        }

        return unitsFound;
    }
}