using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Process for handling enemy turns. Will basically iterate through each unit one at a time and have them act.
public class EnemyTurnProcess
{
    private readonly List<EnemyUnit> _enemyUnits;
    private readonly List<EnemyPod> _enemyPods;
    private readonly EnemyTurnState _enemyTurnState;

    public EnemyTurnProcess(EnemyTurnState enemyTurnState)
    {
        _enemyTurnState = enemyTurnState;
        _enemyUnits = Map.FindEnemyUnits();
        _enemyPods = Map.FindPods();
    }

    public void ProcessTurn()
    {
        GlobalManager.Instance.StartCoroutine(ProcessEnemyUnits());
    }

    public IEnumerator ProcessEnemyUnits()
    {
        foreach (EnemyUnit enemyUnit in _enemyUnits)
        {
            if (enemyUnit.IsFollowingPod())
                continue;

            Camera.main.GetComponent<CameraHandler>().SetCameraFollow(enemyUnit);
            enemyUnit.ProcessTurn();
            //Ensure units act one at a time
            while (enemyUnit.IsProcessingTurn) yield return new WaitForSeconds(0.1f);
        }

        foreach (EnemyPod enemyPod in _enemyPods)
        {
            Camera.main.GetComponent<CameraHandler>().SetCameraFollow(enemyPod.Leader);
            enemyPod.ProcessTurn();
            //Ensure one pod acts at a time
            while (enemyPod.IsProcessingTurn) yield return new WaitForSeconds(0.1f);
        }

        //End turn when we're done processing the units
        EnemyUnitsProcessedCallback();
    }

    public void EnemyUnitsProcessedCallback()
    {
        Camera.main.GetComponent<CameraHandler>().SetCameraFollow(null);
        _enemyTurnState.EndTurn();
    }
}