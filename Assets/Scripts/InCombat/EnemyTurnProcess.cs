using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnProcess
{
    List<EnemyUnit> enemyUnits;
    public EnemyTurnProcess()
    {
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
        //TODO: Get enemy units
        foreach (EnemyUnit enemyUnit in enemyUnits)
        {
            enemyUnit.ProcessUnitTurn();
        }
    }
}