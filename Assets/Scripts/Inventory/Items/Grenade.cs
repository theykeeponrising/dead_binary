using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : DamageItem
{
    public override void UseItem(Unit setSourceUnit, Unit setTargetedUnit)
    {
        sourceUnit = setSourceUnit;
        targetedUnit = setTargetedUnit;

        sourceUnit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.THROW, true);
        ItemProp grenade = Instantiate(itemProp, sourceUnit.GetAnimator().body.handLeft);
        grenade.SetItemEffect(this);
        grenade.SetItemDestination(targetedUnit.transform.position);
    }

    public override void TriggerItem()
    {
        base.UseItem(sourceUnit, targetedUnit);
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        triggerPosition = setTriggerPosition;
        base.UseItem(sourceUnit, targetedUnit);
    }
}
