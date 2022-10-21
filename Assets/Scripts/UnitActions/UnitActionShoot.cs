using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionShoot : UnitTargetAction
{
    int distanceToTarget;
    protected Timer bufferStartTimer;
    protected Timer bufferEndTimer;

    public override void UseAction(Unit setTarget)
    {
        // Locks in target information and begins shoot sequence
        // Sets action to "performing" state

        if (unit.GetActor().IsActing())
            return;

        if (!setTarget)
        {
            Debug.Log(string.Format("{0} was called with no target by {1}", this, unit.gameObject));
            return;
        }

        targetUnit = setTarget;
        distanceToTarget = unit.currentTile.GetMovementCost(targetUnit.currentTile, 15).Count;
        unit.AddFlag(FlagType.AIM);

        unit.GetActor().targetCharacter = setTarget;
        unit.SpendActionPoints(actionCost);

        bufferStartTimer = new Timer(bufferStart);
        bufferEndTimer = new Timer(bufferEnd);

        StartPerformance();
    }

    public override void CheckAction()
    {
        // Wait for the start buffer
        while (!bufferStartTimer.CheckTimer())
            return;

        // Perform shoot animation, inflict damage, spend ammo and AP
        if (ActionStage(0))
        {
            unit.GetAnimator().Play("Shoot");
            if (targetUnit) targetUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage(), distanceToTarget, MessageType.DMG_CONVENTIONAL);
            unit.EquippedWeapon.SpendAmmo();
            NextStage();
        }

        // Waits until shoot animation completes
        while (unit.GetAnimator().AnimatorIsPlaying("Shoot"))
            return;

        // Wait for the end buffer
        while (!bufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        unit.GetActor().ClearTarget();
        EndPerformance();
    }
}
