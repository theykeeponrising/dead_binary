using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : DamageItem
{
    public override void UseItem(Unit setSourceUnit, Vector3 setTargetPosition)
    {
        // Gets unit information, creates grenade prop, and plays throwing animation

        itemAction.StartPerformance();
        sourceUnit = setSourceUnit;
        targetPosition = setTargetPosition;

        float distance = (sourceUnit.transform.position - targetPosition).magnitude;

        if (distance > MapGrid.tileSpacing * 3)
            sourceUnit.GetComponent<Animator>().Play("Throw-Long");
        else
            sourceUnit.GetComponent<Animator>().Play("Throw-Short");

        ItemProp grenade = Instantiate(itemProp, sourceUnit.GetAnimator().body.handLeft);
        grenade.SetItemEffect(this);
        grenade.SetItemDestination(targetPosition);

        sourceUnit.GetActor().ClearTarget();
    }

    public override void TriggerItem()
    {
        // Callback function for props, initiates the item effect

        base.TriggerItem();
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        // Callback function for props, initiates the item effect

        base.TriggerItem(setTriggerPosition);
    }
}
