using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStateAction : UnitAction
{
    public override void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        Debug.Log(string.Format("No action use found for {0} (finite state)", ActionName));
    }
}
