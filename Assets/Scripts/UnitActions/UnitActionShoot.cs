using UnityEngine;

public class UnitActionShoot : UnitTargetAction
{
    private int _distanceToTarget;
    protected Timer _bufferStartTimer;
    protected Timer _bufferEndTimer;

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

        TargetUnit = setTarget;
        _distanceToTarget = unit.currentTile.GetMovementCost(TargetUnit.currentTile, 15).Count;
        unit.AddFlag(FlagType.AIM);

        unit.GetActor().targetCharacter = setTarget;
        unit.SpendActionPoints(actionCost);

        _bufferStartTimer = new(bufferStart);
        _bufferEndTimer = new(bufferEnd);

        StartPerformance();
    }

    public override void CheckAction()
    {
        // Wait for the start buffer
        while (!_bufferStartTimer.CheckTimer())
            return;

        // Perform shoot animation, inflict damage, spend ammo and AP
        if (ActionStage(0))
        {
            PerformShot();
            DamageTarget();
            NextStage();
        }

        // Waits until shoot animation completes
        while (unit.GetAnimator().AnimatorIsPlaying("Shoot"))
            return;

        // Wait for the end buffer
        while (!_bufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        unit.GetActor().ClearTarget();
        EndPerformance();
    }

    private void DamageTarget()
    {
        if (!TargetUnit)
            return;

        TargetUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage(), _distanceToTarget, MessageType.DMG_CONVENTIONAL);
    }

    private void PerformShot()
    {
        unit.GetAnimator().Play("Shoot");
        unit.EquippedWeapon.SpendAmmo();
    }
}
