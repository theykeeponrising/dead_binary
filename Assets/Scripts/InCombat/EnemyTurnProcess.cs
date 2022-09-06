using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnProcess
{
    List<EnemyUnit> enemyUnits;
    EnemyTurnState enemyTurnState;
    public EnemyTurnProcess(EnemyTurnState enemyTurnState)
    {
        this.enemyTurnState = enemyTurnState;
        enemyUnits = new List<EnemyUnit>();
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject go in gameObjects)
        {
            EnemyUnit enemyUnit = go.GetComponent<EnemyUnit>();
            if (enemyUnit) enemyUnits.Add(enemyUnit);
        }
    }

    public void ProcessTurn()
    {
        GlobalManager.Instance.StartCoroutine(ProcessEnemyUnits());
    }

    public IEnumerator ProcessEnemyUnits()
    {
        //TODO: Get enemy units here and pass these along
        foreach (EnemyUnit enemyUnit in enemyUnits)
        {
            enemyUnit.ProcessUnitTurn();
            while (enemyUnit.isActing) yield return new WaitForSeconds(0.1f);
        }

        EnemyUnitsProcessedCallback();
    }

    public void EnemyUnitsProcessedCallback()
    {
        enemyTurnState.EndTurn();
    }
}