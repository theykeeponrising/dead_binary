using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionItem : UnitTargetAction
{
    public override void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        setState.ChangeState(new StateUseItem(setState.Machine, this));
    }

    public override void CheckAction()
    {
        
    }
}
