using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionShootRocket : UnitActionShoot
{
    float _areaOfEffect;
    Tile _targetTile;
    Vector3 _triggerPosition => _targetTile.transform.position;
    Timer _impactTimer;
    [SerializeField] ParticleSystem rocketEffect;

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
        _areaOfEffect = unit.EquippedWeapon.GetAreaOfEffect();
        unit.AddFlag(FlagType.AIM);

        unit.GetActor().targetCharacter = setTarget;
        unit.SpendActionPoints(actionCost);

        bufferStartTimer = new Timer(bufferStart);
        bufferEndTimer = new Timer(bufferEnd);
        _impactTimer = new Timer(0.75f);

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
            NextStage();
        }

        while (!_impactTimer.CheckTimer())
        {
            return;
        }

        if (ActionStage(1))
        {
            DamageTargets();
            ShowRocketEffect();
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

    void DamageTargets()
    {
        // Use on unit if possible, otherwise on empty tile
        _targetTile = targetUnit ? targetUnit.currentTile : unit.grid.GetTile(targetPosition);

        foreach (Unit impactedUnit in Tile.GetTileOccupants(Tile.GetAreaOfEffect(_targetTile, _areaOfEffect)))
        {
            impactedUnit.GetAnimator().TakeDamageEffect(unit.EquippedWeapon);
            impactedUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage(), _triggerPosition);
        }
    }

    void ShowRocketEffect()
    {
        // Creates the item effect object at the trigger position

        if (!rocketEffect)
            return;

        GameObject spawnEffect = GlobalManager.ActiveMap.CreateTimedEffect(rocketEffect.gameObject, _triggerPosition, rocketEffect.transform.rotation, 3f);
        spawnEffect.transform.localScale = Vector3.one * (_areaOfEffect / 2);
        PlayRocketSFX(spawnEffect);
    }

    void PlayRocketSFX(GameObject spawnEffect)
    {
        // Plays the rocket effect sound
        
        AudioSource audioSource = spawnEffect.GetComponent<AudioSource>();
        AudioClip audioClip = AudioManager.GetSound(ItemEffectType.EXPLOSION);
        audioSource.PlayOneShot(audioClip);
    }
}
