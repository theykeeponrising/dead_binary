using UnityEngine;

public class UnitActionShoot : UnitTargetAction
{
    protected Timer _bufferStartTimer;
    protected Timer _bufferEndTimer;
    protected bool _targetDamaged;
    protected bool _targetHit;

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
        unit.AddFlag(FlagType.AIM);

        unit.GetActor().targetCharacter = setTarget;
        unit.SpendActionPoints(actionCost);

        _targetDamaged = false;
        _targetHit = false;
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

    public virtual void TriggerAction(Projectile projectile = null)
    {
        if (projectile)
            Map.MapEffects.DestroyEffect(projectile);

        if (!TargetUnit)
            return;

        DamageTargets();
        HitTargets();
    }

    protected virtual void HitTargets()
    {
        if (_targetHit)
            TargetUnit.GetAnimator().TakeDamageEffect(unit.EquippedWeapon);
    }

    protected virtual void DamageTargets()
    {
        if (!_targetDamaged)
        {
            int distanceToTarget = unit.currentTile.GetMovementCost(TargetUnit.currentTile, 15).Count;
            _targetHit = TargetUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage(), distanceToTarget, MessageType.DMG_CONVENTIONAL);
            _targetDamaged = true;
        }
    }

    protected void PerformShot()
    {
        unit.GetAnimator().Play("Shoot");
        unit.EquippedWeapon.SpendAmmo();
    }

    public virtual void SpawnProjectile(Projectile projectile, Transform barrelEnd, float speed)
    {
        if (!projectile)
            TriggerAction();

        Vector3 destination = TargetUnit.GetAnimator().GetBoneTransform(HumanBodyBones.Chest).transform.position;
        projectile = Map.MapEffects.CreateEffect(projectile, barrelEnd.position, barrelEnd.rotation);
        Vector3 variation = new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5)) * 0.1f;
        projectile.Init(this, destination + variation, speed);
    }
}
