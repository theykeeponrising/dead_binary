using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitTargetAction : UnitAction
{
    [HideInInspector] public Unit targetUnit;
    [HideInInspector] public Vector3 targetPosition;

    public override void UseAction(Unit unit)
    {
        base.UseAction();
    }
}
