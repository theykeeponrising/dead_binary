using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Process for handling enemy turns. Will basically iterate through each unit one at a time and have them act.
public class EnemyTurnProcess
{
    private readonly List<EnemyUnit> _enemyUnits;
    private readonly EnemyTurnState _enemyTurnState;
    public EnemyTurnProcess(EnemyTurnState enemyTurnState)
    {
        _enemyTurnState = enemyTurnState;
        _enemyUnits = new ();
        List<Unit> units = Map.FindUnits();
        foreach (Unit unit in units)
        {
            EnemyUnit enemyUnit = unit.GetComponent<EnemyUnit>();
            if (enemyUnit) _enemyUnits.Add(enemyUnit);
        }
    }

    public void ProcessTurn()
    {
        GlobalManager.Instance.StartCoroutine(ProcessEnemyUnits());
    }

    public IEnumerator ProcessEnemyUnits()
    {
        //TODO: Get enemy units here and pass these along
        foreach (EnemyUnit enemyUnit in _enemyUnits)
        {
            enemyUnit.ProcessUnitTurn();
            //Ensure units act one at a time
            while (enemyUnit.IsProcessingTurn()) yield return new WaitForSeconds(0.1f);
        }

        //End turn when we're done processing the units
        EnemyUnitsProcessedCallback();
    }

    public void EnemyUnitsProcessedCallback()
    {
        _enemyTurnState.EndTurn();
    }
}