using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : DamageItem
{
    public override void UseItem(Unit setSourceUnit, Unit setTargetedUnit)
    {
        // Gets unit information, creates grenade prop, and plays throwing animation

        sourceUnit = setSourceUnit;
        targetedUnit = setTargetedUnit;

        sourceUnit.GetActor().ClearTarget();

        base.TriggerItem();
    }

    // TO-DO : Medkit SFX, animations
}
