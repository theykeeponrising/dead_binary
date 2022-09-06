using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Process for handling enemy turns. Will basically iterate through each unit one at a time and have them act.
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
            //Ensure units act one at a time
            while (enemyUnit.isActing) yield return new WaitForSeconds(0.1f);
        }

        //End turn when we're done processing the units
        EnemyUnitsProcessedCallback();
    }

    public void EnemyUnitsProcessedCallback()
    {
        enemyTurnState.EndTurn();
    }
}