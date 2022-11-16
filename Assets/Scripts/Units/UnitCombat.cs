using System.Collections.Generic;
using UnityEngine;

class UnitCombat
{
    private readonly Unit _unit;
    private Unit _targetUnit;
    private List<Unit> _potentialTargets;
    private readonly InfoPanelScript _infoPanel;

    private Weapon EquippedWeapon { get { return _unit.EquippedWeapon; } }
    private Tile UnitTile { get { return _unit.Tile; } }
    private Tile TargetUnitTile { get { return _targetUnit.Tile; } }
    public Unit TargetUnit { get { return _targetUnit; } set { _targetUnit = value; } }
    public List<Unit> PotentialTargets { get { return _potentialTargets; } set { _potentialTargets = value; } }

    public UnitCombat(Unit unit)
    {
        _unit = unit;
        _infoPanel = UIManager.GetInfoPanel();
    }

    public Vector3 GetTargetPosition(bool snapToTarget = false)
    {
        // Gets target's position relative to the tip of the gun

        Vector3 targetDirection = TargetUnit.GetBoneTransform(HumanBodyBones.Chest).position - _unit.EquippedWeapon.transform.position;
        Vector3 aimDirection = _unit.EquippedWeapon.transform.forward;
        float blendOut = 0.0f;
        float angleLimit = 90f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50f;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        if (snapToTarget) direction = targetDirection;
        return _unit.EquippedWeapon.transform.position + direction;
    }

    public void GetTarget(bool useCharacterCamera = false)
    {
        // Character it put into "targeting" mode
        // Target selected with left-click will have action done to it (such as attack action)

        _unit.GetComponentInChildren<UnitCamera>().enabled = useCharacterCamera;
        _unit.GetComponent<Collider>().enabled = !useCharacterCamera;
        _unit.ProcessAnimationEvent(AnimationEventContext.AIMING, true);
        if (_unit.IsCrouching()) _unit.ToggleCrouch();

        _infoPanel.gameObject.SetActive(true);
        UpdateHitStats();
    }

    public void UpdateHitStats()
    {
        _infoPanel.UpdateHit(_unit.GetHitChance());
        _infoPanel.UpdateDamage(-_unit.EquippedWeapon.GetDamage());
    }

    public void ClearTarget()
    {
        // Cleans up targeting-related objects

        _unit.GetComponentInChildren<UnitCamera>().enabled = false;
        _unit.GetComponent<Collider>().enabled = true;
        _infoPanel.gameObject.SetActive(false);

        _unit.SetAnimatorBool("aiming", false);
        _unit.SetAnimatorMode();
        _unit.RemoveFlag(AnimationFlag.AIM);
        TargetUnit = null;

        _unit.CoverCrouch();
    }

    public float CalculateHitChance()
    {
        // Calculate hit chance from the attacker's perspective

        int distance = Map.MapGrid.GetTileDistance(UnitTile, TargetUnitTile);
        float weaponAccuracyModifier = EquippedWeapon.Stats.BaseAccuracyModifier;
        float weaponAccuracyPenalty = EquippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + _unit.stats.aim - _targetUnit.stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (_targetUnit.CurrentCover && Map.MapGrid.CheckIfCovered(UnitTile, TargetUnitTile))
            hitModifier -= _targetUnit.CurrentCover.GetCoverBonus();

        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }
}
