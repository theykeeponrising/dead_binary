using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionShoot : UnitTargetAction
{
    int minWeaponRange;
    int distanceToTarget;
    Timer bufferStartTimer;
    Timer bufferEndTimer;

    public override void UseAction(Unit setTarget)
    {
        // Kicks off unit and weapon's reload methods
        // Sets action to "performing" state

        targetUnit = setTarget;
        minWeaponRange = unit.GetEquippedWeapon().GetMinimumRange();
        distanceToTarget = unit.currentTile.FindCost(targetUnit.currentTile, 15).Count;

        if (distanceToTarget < minWeaponRange) // TO DO -- rework minimum range to inflict accuracy penality instead
        {
            Debug.Log(string.Format("Target is too close! \nDistance: {0}, Weapon Range: {1}", distanceToTarget, minWeaponRange)); // This will eventually be shown visually instead of told
            return;
        }

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
            if (targetUnit) targetUnit.TakeDamage(unit, unit.inventory.equippedWeapon.stats.damage, distanceToTarget);
            unit.SpendActionPoints(actionCost);
            unit.GetEquippedWeapon().SpendAmmo();
            NextStage();
        }

        // Waits until shoot animation completes
        while (unit.GetAnimator().AnimatorIsPlaying("Shoot"))
            return;

        // Wait for the end buffer
        while (!bufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        unit.GetAnimator().SetBool("aiming", false);
        unit.RemoveFlag("aiming");
        unit.GetAnimator().SetUpdateMode();
        EndPerformance();
    }
}
