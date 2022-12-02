using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPod : MonoBehaviour
{
    [SerializeField] private List<EnemyUnit> _units = new();
    [SerializeField] private EnemyUnit _leader;
    [SerializeField] private EnemyPatrol _enemyPatrol;
    private bool _inCombat;

    public bool IsProcessingTurn { get { return _units.Any(x => x.IsProcessingTurn); } }
    public bool IsInCombat { get { return _inCombat; } }
    public EnemyUnit Leader { get { return _leader; } }

    private void Awake()
    {
        GetUnits();
        GetLeader();
    }

    private void GetUnits()
    {
        if (_units.Count > 0)
            return;

        _units = GetComponentsInChildren<EnemyUnit>().ToList();

        foreach (EnemyUnit unit in _units)
        {
            if (_enemyPatrol)
                unit.SetPod(this, _enemyPatrol);
            else
                unit.SetPod(this);
        }
    }

    private void GetLeader()
    {
        if (_leader)
            return;

        _leader = _units[0];
    }

    public bool IsLeader(EnemyUnit unit)
    {
        return _leader == unit;
    }

    public void ProcessTurn()
    {
        if (_inCombat)
            return;

        foreach (EnemyUnit unit in _units)
            unit.ProcessTurn();
    }

    public void EnterCombat()
    {
        if (_inCombat)
            return;

        _inCombat = true;

        foreach (EnemyUnit unit in _units)
            unit.EnterCombat();
    }
}
