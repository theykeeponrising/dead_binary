using UnityEngine;

public class UnitActionShoot : UnitTargetAction
{
    protected Timer BufferStartTimer;
    protected Timer BufferEndTimer;
    protected bool TargetDamaged;
    protected bool TargetHit;

    public override void UseAction(Unit setTarget)
    {
        // Locks in target information and begins shoot sequence
        // Sets action to "performing" state

        if (Unit.IsActing())
            return;

        if (!setTarget)
        {
            Debug.Log(string.Format("{0} was called with no target by {1}", this, Unit.gameObject));
            return;
        }

        TargetUnit = setTarget;
        Unit.TargetUnit = setTarget;
        Unit.SpendActionPoints(ActionCost);

        TargetDamaged = false;
        TargetHit = false;
        BufferStartTimer = new(BufferStart);
        BufferEndTimer = new(BufferEnd);

        StartPerformance();
    }

    public override void CheckAction()
    {
        // Wait for the start buffer
        while (!BufferStartTimer.CheckTimer())
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
        while (Unit.IsPlayingAnimation("Shoot"))
            return;

        // Wait for the end buffer
        while (!BufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        Unit.ClearTarget();
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
        if (TargetHit)
            TargetUnit.TakeDamageEffect(Unit.EquippedWeapon);
    }

    protected virtual void DamageTargets()
    {
        if (!TargetHit)
            return;

        if (!TargetDamaged)
        {
            if (TargetUnit.Attributes.faction == FactionManager.ACS) UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.DMG_CONVENTIONAL);
            TargetUnit.TakeDamage(Unit, Unit.EquippedWeapon.GetDamage());
            TargetDamaged = true;
        }
    }

    protected void PerformShot()
    {
        Unit.PlayAnimation("Shoot");
        Unit.EquippedWeapon.SpendAmmo();
    }

    private void CheckTargetHit()
    {
        int distanceToTarget = Unit.Tile.GetMovementCost(TargetUnit.Tile, 15).Count;
        TargetHit = Unit.RollForHit(distanceToTarget);
    }

    private void CheckDodge()
    {
        if (TargetHit)
            return;

        else if (!TargetUnit.IsDodging())
            TargetUnit.DodgeAttack(Unit);
    }

    private Vector3 ProjectileTrajectory()
    {
        int lowRange = TargetHit ? 0 : 3;
        int highRange = TargetHit ? lowRange + 2 : lowRange + 5;

        Vector3 trajectory = new Vector3(GetRandomInt(lowRange, highRange), GetRandomInt(lowRange, highRange), GetRandomInt(lowRange, highRange)) * 0.1f;
        Vector3 direction =  TargetUnit.transform.position - Unit.transform.position;

        if (!TargetHit)
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
