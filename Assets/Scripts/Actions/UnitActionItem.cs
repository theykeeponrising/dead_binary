using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionItem : UnitAction
{

    public override void UseAction(FiniteState<InCombatPlayerAction> setState, Item item)
    {
        setState.ChangeState(new StateUseItem(setState.Machine, item));
    }
}
