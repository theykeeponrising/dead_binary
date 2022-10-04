using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionDoNothing : UnitAction
{
    public override void UseAction()
    {
        // Flags the unit as "doing nothing"
        // Displays waiting icon above character and flags them as ending turn

        if (unit.GetActor().IsActing())
            return;

        SetPerformed(true);
        unit.GetActor().GetPlayerAction().SelectAction();
        unit.healthbar.DoNothingIndicator(true);
        unit.GetActor().playerAction.CheckTurnEnd();
    }
}
