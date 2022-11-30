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

    private void GetHostileUnits()
    {
        List<Faction> hostileFactions = _faction.GetFactionsByRelation(FactionAffinity.ENEMY);

        foreach (Faction faction in hostileFactions)
            _hostileUnits.AddRange(Map.FindUnits(faction));
    }

    public bool InCombat()
    {
        foreach (Unit unit in _hostileUnits)
            if (unit.InCombat && unit.IsAlive()) return true;
        return false;
    }
}
