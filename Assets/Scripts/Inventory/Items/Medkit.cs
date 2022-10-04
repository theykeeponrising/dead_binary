using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : DamageItem
{
    public override void UseItem(Unit setSourceUnit, Unit setTargetedUnit)
    {
        // Gets unit information, heals unit

        sourceUnit = setSourceUnit;
        targetedUnit = setTargetedUnit;

        sourceUnit.GetActor().ClearTarget();

        base.TriggerItem();
    }

    // TO-DO : Medkit SFX, animations
}
