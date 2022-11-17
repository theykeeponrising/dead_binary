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

        if (unit.IsActing())
            return;

        if (!setTarget)
        {
            Debug.Log(string.Format("{0} was called with no target by {1}", this, unit.gameObject));
            return;
        }

        TargetUnit = setTarget;
        unit.TargetUnit = setTarget;
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
            CheckTargetHit();
            PerformShot();
            NextStage();
        }

        if (ActionStage(1))
        {
            CheckDodge();
            NextStage();
        }

        // Waits until shoot animation completes
        while (unit.IsPlayingAnimation("Shoot"))
            return;

        // Wait for the end buffer
        while (!_bufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        unit.ClearTarget();
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
            TargetUnit.TakeDamageEffect(unit.EquippedWeapon);
    }

    protected virtual void DamageTargets()
    {
        if (!_targetHit)
            return;

        if (!_targetDamaged)
        {
            if (TargetUnit.Attributes.faction == FactionManager.ACS) UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.DMG_CONVENTIONAL);
            TargetUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage());
            _targetDamaged = true;
        }
    }

    protected void PerformShot()
    {
        unit.PlayAnimation("Shoot");
        unit.EquippedWeapon.SpendAmmo();
    }

    private void CheckTargetHit()
    {
        int distanceToTarget = unit.Tile.GetMovementCost(TargetUnit.Tile, 15).Count;
        _targetHit = unit.RollForHit(distanceToTarget);
    }

    private void CheckDodge()
    {
        if (_targetHit)
            return;

        else if (!TargetUnit.IsDodging())
            TargetUnit.DodgeAttack(unit);
    }

    private Vector3 ProjectileTrajectory()
    {
        int lowRange = _targetHit ? 0 : 3;
        int highRange = _targetHit ? lowRange + 2 : lowRange + 5;

        Vector3 trajectory = new Vector3(GetRandomInt(lowRange, highRange), GetRandomInt(lowRange, highRange), GetRandomInt(lowRange, highRange)) * 0.1f;
        Vector3 direction =  TargetUnit.transform.position - unit.transform.position;

        if (!_targetHit)
            trajectory += direction * 1.5f;

        return trajectory;
    }

    private int GetRandomInt(int lowRange, int highRange)
    {
        int randomBool = Random.Range(0, 100);
        bool negative = (randomBool % 2 == 0);
        int modifier = negative ? -1 : 1;
        int randomNumber = Random.Range(lowRange, highRange);

        return randomNumber * modifier;
    }

    public virtual void SpawnProjectile(Projectile projectile, Transform barrelEnd, float speed)
    {
        if (!projectile)
            TriggerAction();

        Vector3 destination = TargetUnit.GetBoneTransform(HumanBodyBones.Chest).transform.position;
        Vector3 trajectory = ProjectileTrajectory();

        projectile = Map.MapEffects.CreateEffect(projectile, barrelEnd.position, barrelEnd.rotation);     
        projectile.Init(this, destination + trajectory, speed);
    }
}
