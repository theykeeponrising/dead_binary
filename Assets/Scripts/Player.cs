using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Faction _faction;
    private List<Unit> _units = new();
    private List<Unit> _hostileUnits = new();

    private void Start()
    {
        GetPlayerUnits();
        GetHostileUnits();
    }

    private void GetPlayerUnits()
    {
        _units = Map.FindUnits(_faction);
    }

    public void GetHostileUnits()
    {
        // Finds all hostile units regardless of their faction
        // Useful for encounters where there may be multiple hostile factions at once

        List<Faction> hostileFactions = _faction.GetFactionsByRelation(FactionAffinity.ENEMY);
        List<Unit> hostileUnits = new();

        foreach (Faction faction in hostileFactions)
            hostileUnits.AddRange(Map.FindUnits(faction));

        _hostileUnits = hostileUnits;
    }

    public bool InCombat()
    {
        foreach (Unit unit in _hostileUnits)
        {
            if (unit.IsAlive() && unit.InCombat)
            {
                //Debug.Log(string.Format("{0} in combat", unit));
                return true;
            }
        }
        return false;
    }
}
