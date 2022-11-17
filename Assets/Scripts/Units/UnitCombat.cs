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
    public Unit TargetUnit { get { return _targetUnit; } set { _targetUnit = value; } }
    public List<Unit> PotentialTargets { get { return _potentialTargets; } set { _potentialTargets = value; } }

    public UnitCombat(Unit unit)
    {
        _unit = unit;
        _infoPanel = UIManager.GetInfoPanel();
    }

    public List<Unit> GetHostileUnits()
    {
        List<Faction> hostileFactions = _unit.attributes.faction.GetFactionsByRelation(FactionAffinity.ENEMY);
        List<Unit> hostileUnits = new();

        foreach (Faction faction in hostileFactions)
            hostileUnits.AddRange(Map.FindUnits(faction));

        // Sort list by distance to current unit
        if (hostileUnits.Count > 0)
        {
            hostileUnits.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(_unit.transform.position, a.transform.position).CompareTo(
                    Vector2.Distance(_unit.transform.position, b.transform.position));
            });
        }

        return hostileUnits;
    }

    public Vector3 GetTargetPosition(bool snapToTarget = false)
    {
        // Gets target's position relative to the tip of the gun

        Vector3 targetDirection = _targetUnit.GetBoneTransform(HumanBodyBones.Chest).position - _unit.EquippedWeapon.transform.position;
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
        _targetUnit = null;

        _unit.CoverCrouch();
    }

    public Unit GetNearestTarget(Tile unitTile, List<Unit> targets)
    {
        if (targets.Count == 0) return null;
        float minDistance = float.MaxValue;
        Unit closestUnit = targets[0];
        foreach (Unit target in targets)
        {
            float tileDist = Map.MapGrid.GetTileDistance(unitTile, target.Tile);
            if (tileDist < minDistance)
            {
                minDistance = tileDist;
                closestUnit = target;
            }
        }
        return closestUnit;
    }

    public float CalculateHitChance()
    {
        // Calculate hit chance from the attacker's perspective

        int distance = Map.MapGrid.GetTileDistance(UnitTile, _targetUnit.Tile);
        float weaponAccuracyModifier = EquippedWeapon.Stats.BaseAccuracyModifier;
        float weaponAccuracyPenalty = EquippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + _unit.stats.aim - _targetUnit.stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (_targetUnit.CurrentCover && Map.MapGrid.CheckIfCovered(UnitTile, _targetUnit.Tile))
            hitModifier -= _targetUnit.CurrentCover.GetCoverBonus();

        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }

    public float CalculateHitChance(Unit sampleUnit)
    {
        // Calculate hit chance from the attacker's perspective

        int distance = Map.MapGrid.GetTileDistance(UnitTile, sampleUnit.Tile);
        float weaponAccuracyModifier = EquippedWeapon.Stats.BaseAccuracyModifier;
        float weaponAccuracyPenalty = EquippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + _unit.stats.aim - sampleUnit.stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (sampleUnit.CurrentCover && Map.MapGrid.CheckIfCovered(UnitTile, sampleUnit.Tile))
            hitModifier -= sampleUnit.CurrentCover.GetCoverBonus();

        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }

    public float CalculateExpectedDamage()
    {
        float weaponDamange = EquippedWeapon.GetDamage();
        float hitChance = CalculateHitChance();

        return weaponDamange * hitChance;
    }

    public float CalculateExpectedDamage(Unit sampleUnit)
    {
        float weaponDamage = EquippedWeapon.GetDamage();
        float hitChance = CalculateHitChance(sampleUnit);

        return weaponDamage * hitChance;
    }

    public bool RollForHit(int distanceToTarget)
    {
        // Roll for hit against target unit

        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float hitChance = CalculateHitChance();
        float baseChance = hitChance * 100.0f;

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distanceToTarget, baseChance, randomChance));

        // Return true/false if hit connected
        return (baseChance >= randomChance);
    }

    public void TakeDamage(Unit attacker, int damage)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        Vector3 direction = (_unit.transform.position - attacker.transform.position);
        float distance = (_unit.transform.position - attacker.transform.position).magnitude;

        _unit.CheckDeath(attacker, direction, distance, damage);
    }

    public void TakeDamage(Unit attacker, int damage, Vector3 attackPoint)
    {
        // Called by an attacking item when taking damage
        // TO DO: More complex damage reduction will be added here

        Vector3 direction = _unit.transform.position - attackPoint;
        float distance = direction.magnitude;

        _unit.CheckDeath(attacker, direction, distance, damage, 50f);
    }

    public void DodgeAttack(Unit attacker)
    {
        _unit.CurrentCover?.PlayImpactSFX();
        _unit.SetAnimatorTrigger("dodge");
        Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, _unit.attributes.name));
    }
}
