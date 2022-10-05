using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : DamageItem
{
    public override bool UseItem(Unit setSourceUnit, Unit setTargetedUnit)
    {
        // Gets unit information, heals unit

        sourceUnit = setSourceUnit;
        targetedUnit = setTargetedUnit;

        if (targetedUnit.stats.unitType == UnitType.ROBOTIC)
        {
            Debug.Log("Units of type \"ROBOTIC\" cannot be healed by medkits");
            return false;
        }
        
        sourceUnit.GetActor().ClearTarget();

        base.TriggerItem();
        return true;
    }

    // TO-DO : Medkit SFX, animations
}
