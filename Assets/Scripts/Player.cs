using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Faction _faction;
    private List<Unit> _units = new();

    private void Start()
    {
        GetPlayerUnits();
    }

    private void GetPlayerUnits()
    {
        _units = Map.FindUnits(_faction);
    }

    public bool InCombat()
    {
        foreach (Unit unit in _units)
            if (unit.InCombat) return true;
        return false;
    }
}
