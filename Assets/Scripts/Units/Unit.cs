using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//This class is a high-level handler for a character/unit, intended to deal with e.g. stats/attributes/faction/inventory/etc.
//Implementation of character/unit actions should be done in CharacterActor.cs
//Implementation of character/unit animation logic should be done in CharacterAnimator.cs
//Implementation of character/unit SFX logic should be in CharacterSFX.cs

public class Unit : GridObject, IPointerEnterHandler, IPointerExitHandler
{
    private AudioSource _audioSource;
    private Rigidbody _rigidbody;
    private Collider[] _colliders;
    private Transform _actionsContainer;

    private UnitActor _unitActor;
    private UnitAnimator _unitAnimator;
    private UnitSFX _unitSFX;
    private UnitCombat _unitCombat;

    private Inventory _inventory;
    private UnitHealthbar _healthbar;

    private InCombatPlayerAction _playerAction;
    [SerializeField] private List<UnitAction> _unitActions;

    [Header("--Character Info")]
    public UnitStats Stats;
    public UnitAttributes Attributes;

    public Rigidbody Rigidbody { get { return _rigidbody; } }
    public Collider[] Colliders { get { return _colliders; } }
    public Inventory Inventory { get { return _inventory; } }
    public UnitHealthbar Healthbar { get { return _healthbar; } }
    public CoverObject CurrentCover { get { return objectTile.Cover; } }
    public InCombatPlayerAction PlayerAction { get { return _playerAction; } }
    public Weapon EquippedWeapon { get { return Inventory.EquippedWeapon; } set { Inventory.EquipWeapon(value); } }
    public Weapon AltWeapon { get { return Inventory.AltWeapon; } }
    public Unit TargetUnit { get { return _unitCombat.TargetUnit; } set { _unitCombat.TargetUnit = value; } }
    public List<Unit> PotentialTargets { get { return _unitCombat.PotentialTargets; } set { _unitCombat.PotentialTargets = value; } }
    public bool InCombat { get { return _unitCombat.InCombat; } }
    public MoveData MoveData { get { return _unitActor.MoveData; } set { _unitActor.MoveData = value; } }

    public event System.Action OnHealthModified;
    public event System.Action OnUnitDied;

    protected override void Awake()
    {
        base.Awake();
        this.name = string.Format("{0} (Character)", Attributes.Name);

        _audioSource = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();
        _colliders = GetComponents<Collider>();
        _actionsContainer = transform.Find("Actions");

        _unitActor = new UnitActor(this);
        _unitAnimator = new UnitAnimator(this);
        _unitSFX = new UnitSFX(this, _audioSource);
        _unitCombat = new UnitCombat(this);

        _inventory = GetComponentInChildren<Inventory>();
        _healthbar = GetComponentInChildren<UnitHealthbar>();
    }

    protected override void Start()
    {
        base.Start();
        if (objectTiles.Count > 0) objectTile = objectTiles[0];

        // Characters start with full health and action points
        Stats.HealthCurrent = Stats.HealthMax;
        Stats.ActionPointsCurrent = Stats.ActionPointsMax;

        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        _playerAction = playerTurnState.GetPlayerAction();
        _inventory.Init(this);

        GenerateActions();
    }

    protected virtual void FixedUpdate()
    {
        _unitAnimator.FixedUpdate();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        _unitActor.Update();
    }

    private void LateUpdate()
    {
        _unitAnimator.LateUpdate();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        _unitActor.OnPointerEnter(eventData);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        _unitActor.OnPointerExit(eventData);
    }

    public virtual void OnTurnStart()
    {
        ResetActions();
        ResetActionPoints();
        _unitActor?.SetWaiting(false);
    }

    public bool HasTurnEnded()
    {
        if (Stats.ActionPointsCurrent == 0)
            return true;

        if (_unitActor.FindActionOfType(typeof(UnitActionWait)).Performed())
            return true;

        return false;
    }

    public List<UnitAction> GetUnitActions()
    { return _unitActions; }

    public Transform GetUnitActionsContainer()
    { return _actionsContainer; }

    public Weapon CycleWeapon()
    { return Inventory.CycleWeapon(); }

    public List<Item> GetItems()
    { return Inventory.Items; }

    public bool IsAlive()
    { return !IsDead(); }

    public bool IsDead()
    { return Stats.HealthCurrent <= 0; }

    public void GetTarget(bool useCharacterCamera = false)
    { _unitCombat.GetTarget(useCharacterCamera); }

    public void ClearTarget()
    { _unitCombat.ClearTarget(); }

    public Unit GetNearestTarget(Tile unitTile, List<Unit> targets)
    { return _unitCombat.GetNearestTarget(unitTile, targets); }

    public Vector3 GetTargetPosition(bool snapToTarget = false)
    { return _unitCombat.GetTargetPosition(snapToTarget); }

    public List<Unit> GetHostileUnits()
    { return _unitCombat.GetHostileUnits(); }

    public float CalculateExpectedDamage()
    { return _unitCombat.CalculateExpectedDamage(); }

    public float CalculateExpectedDamage(Unit sampleUnit)
    { return _unitCombat.CalculateExpectedDamage(sampleUnit); }

    public float GetHitChance()
    { return _unitCombat.CalculateHitChance(); }

    public float GetHitChance(Unit sampleUnit)
    { return _unitCombat.CalculateHitChance(sampleUnit); }

    public bool RollForHit(int distanceToTarget)
    { return _unitCombat.RollForHit(distanceToTarget); }

    public bool IsEnemy(Unit unit)
    { return _unitCombat.IsEnemy(unit); }

    public virtual void EnterCombat(bool alertFriendlies = true)
    { _unitCombat.EnterCombat(alertFriendlies); }

    public virtual void LeaveCombat()
    { _unitCombat.LeaveCombat(); }

    public void DodgeAttack(Unit attacker)
    { _unitCombat.DodgeAttack(attacker); }

    public void UpdateHitStats()
    { _unitCombat.UpdateHitStats(); }

    public virtual void TakeDamage(Unit attacker, int damage)
    { _unitCombat.TakeDamage(attacker, damage); }

    public virtual void TakeDamage(Unit attacker, int damage, Vector3 attackPoint)
    { _unitCombat.TakeDamage(attacker, damage, attackPoint); }

    public void CheckSight()
    { _unitCombat.CheckSight(); }

    public List<Unit> GetTargetsInLineOfSight<TargetType>()
    { return _unitCombat.GetTargetsInLineOfSight<TargetType>(); }

    public bool IsTargetInLineOfSight(Unit target)
    { return _unitCombat.IsTargetInLineOfSight(target); }

    public bool IsActing()
    { return _unitActor.IsActing(); }

    public UnitAction FindActionOfType(System.Type actionType)
    { return _unitActor.FindActionOfType(actionType); }

    public void Say(string dialog)
    { _unitActor.Say(dialog); }

    public void SelectUnit(SelectionType selectionType)
    { _unitActor.SelectUnit(selectionType); }

    public void UseItem(Item item, Unit target)
    { _unitActor.ItemAction(item, target); }

    public void UseItem(Item item, Tile target)
    { _unitActor.ItemAction(item, target); }

    public void SetWaiting(bool isWaiting)
    { _unitActor.SetWaiting(isWaiting); }

    public List<Tile> GetMovePath(Tile tile)
    { return _unitActor.GetMovePath(tile); }

    public UnitRig GetRig()
    { return _unitAnimator.GetRig(); }

    public Transform GetBoneTransform(HumanBodyBones bone)
    { return _unitAnimator.GetBoneTransform(bone); }

    public void ToggleCrouch(bool instant = false)
    { _unitAnimator.ToggleCrouch(instant); }

    public bool IsCrouching()
    { return _unitAnimator.IsCrouching(); }

    public void CoverCrouch()
    { _unitAnimator.CoverCrouch(); }

    public bool IsDodging()
    { return _unitAnimator.IsDodging(); }

    public bool IsVaulting()
    { return _unitAnimator.IsVaulting(); }

    public bool IsAiming()
    { return _unitAnimator.IsAiming(); }

    public bool IsShooting()
    { return _unitAnimator.IsShooting(); }

    public bool IsMoving()
    { return _unitAnimator.IsMoving(); }

    public bool IsPatrolling()
    { return _unitAnimator.IsPatrolling(); }

    public void ProcessAnimationEvent(AnimationEventContext animationEvent)
    { _unitAnimator.ProcessAnimationEvent(animationEvent, false); }

    public void ProcessAnimationEvent(AnimationEventContext animationEvent, bool state)
    { _unitAnimator.ProcessAnimationEvent(animationEvent, state); }

    public void SetAnimatorBool(string name, bool state)
    { _unitAnimator.SetBool(name, state); }

    public void SetAnimatorMode(AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal)
    { _unitAnimator.SetUpdateMode(updateMode); }

    public void SetAnimatorTrigger(string name)
    { _unitAnimator.SetTrigger(name); }

    public void OnDeath(Vector3 forceDirection, ForceMode forceMode)
    { _unitAnimator.OnDeath(forceDirection, forceMode); }

    public Transform GetAttachPoint(WeaponAttachPoint attachPoint)
    { return _unitAnimator.GetAttachPoint(attachPoint); }

    public void TakeDamageEffect(Weapon weapon = null, DamageItem item = null)
    { _unitAnimator.TakeDamageEffect(weapon, item); }

    public bool IsPlayingAnimation(string animationName)
    { return _unitAnimator.AnimatorIsPlaying(animationName); }

    public void PlayAnimation(string animationName)
    { _unitAnimator.Play(animationName); }

    public void SetAnimationLayerWeight(int layer, float weight)
    { _unitAnimator.SetLayerWeight(layer, weight); }

    public void SetAnimationSpeed(float animationSpeed)
    { _unitAnimator.SetAnimationSpeed(animationSpeed); }

    public void PlaySound(AnimationType soundType)
    { _unitSFX.PlaySound(soundType); }

    public void PlayDeathSound()
    { _unitSFX.PlayDeathSound(); }

    private void GenerateActions()
    {
        // Adds actions to the character based on its current equipment

        int index = 0;

        // If we have movement, add move action
        if (Stats.Movement > 0)
        {
            _unitActions.Insert(index, ActionManager.UnitActions.Move);
            index += 1;
        }

        // If we have an equipped weapon, add shoot action
        if (EquippedWeapon)
        {
            _unitActions.Insert(index, EquippedWeapon.WeaponAction);
            index += 1;
        }

        // If we have an equipped weapon, add reload action
        if (EquippedWeapon)
        {
            _unitActions.Insert(index, ActionManager.UnitActions.Reload);
            index += 1;
        }

        // If we have multiple weapons, add swap action
        if (Inventory.Weapons.Count > 1)
        {
            _unitActions.Insert(index, ActionManager.UnitActions.Swap);
            index += 1;
        }

        // If we have items, add inventory action
        if (GetItems().Count > 0)
        {
            _unitActions.Insert(index, ActionManager.UnitActions.Inventory);
            index += 1;
        }

        // Always add "Wait" action
        _unitActions.Insert(index, ActionManager.UnitActions.Wait);

        // Clone the prefabs
        for (index = 0; index < _unitActions.Count; index++)
        {
            _unitActions[index] = Instantiate(_unitActions[index], _actionsContainer);
        }
    }

    public void SpendActionPoints(int amount)
    {
        // Reduces action points by amount provided

        Stats.ActionPointsCurrent -= amount;
    }

    public void ResetActionPoints()
    {
        // Used to refresh character action points to max
        // TO-DO -- Add AP penalties here from debuffs

        Stats.ActionPointsCurrent = Stats.ActionPointsMax;
    }

    public void ResetActions()
    {
        // Resets all actions back to starting point
        // Called at the beginning of the turn

        foreach (UnitAction unitAction in _unitActions)
            unitAction.OnTurnStart();
    }

    public int GetHealth()
    {
        return Stats.HealthCurrent;
    }

    public void RestoreHealth(int amount)
    {
        // Heals character by the indicated amount

        Stats.HealthCurrent += amount;
        if (Stats.HealthCurrent > Stats.HealthMax) Stats.HealthCurrent = Stats.HealthMax;
        Healthbar.UpdateHealthPoints();
    }

    public List<Tile> GetTilesInMoveRange()
    {
        Vector3 pos = objectTile.transform.position;
        return Map.MapGrid.GetTilesInRange(pos, Stats.Movement);
    }

    public void CheckDeath(
        Unit attacker,
        Vector3 direction,
        float distance,
        int damage,
        float impactForce = 2f)
    {
        //todo: show visually instead of told.
        // Inflict damage on character
        Debug.Log($"{attacker.Attributes.Name} has attacked {Attributes.Name} for {damage} damage!");

        Stats.HealthCurrent -= Mathf.Min(damage, Stats.HealthCurrent);
        Healthbar.UpdateHealthPoints();

        OnHealthModified?.Invoke();

        // Character death
        if (IsDead())
        {
            OnUnitDied?.Invoke();
            LeaveCombat();
            SelectUnit(SelectionType.DESELECT);
            Death(attacker, direction, distance, impactForce);
        }
    }

    public void Death(Unit attacker, Vector3 attackDirection, float distance, float impactForce)
    {
        // Disables animator, turns on ragdoll effect, and applies a small force to push the character over

        // Disable top collider
        GetComponent<CapsuleCollider>().enabled = false;
        Healthbar.gameObject.SetActive(false);

        if (distance == 0) distance = 1;

        // Disable animator and top rigidbody
        OnDeath(attackDirection * impactForce / distance, ForceMode.Impulse);

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (_playerAction.selectedCharacter == this)
            _playerAction.SelectAction();

        // Disable any character lights
        foreach (Light light in GetComponentsInChildren<Light>())
            light.enabled = false;

        // Remove character as a obstacle on the map
        objectTile.Occupant = null;
        enabled = false;

        if (EquippedWeapon)
            EquippedWeapon.DropGun();

        PlayDeathSound();

        // Display message
        if (attacker.Attributes.Faction == FactionManager.ACS)
            UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.PV_DEATH);

        else if (Attributes.Faction == FactionManager.ACS)
            UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.ACS_DEATH);
    }
}

// Attributes are mosty permanent descriptors about the character
[System.Serializable]
public class UnitAttributes
{
    public string Name;
    public Faction Faction;
    public UnitType UnitType;
    public UnitIconEnum UnitIcon;
    public FootstepSource FootstepSource;
    public UnitPortrait UnitPortrait;
    public DialogVoice UnitVoice;
    [Range(0.01f, 3f)] public float UnitVoicePitch = 1f;
    public DeathType UnitDeathType;
    public bool UseTorsoTwist;
}

// Stats are values that will be referenced and changed frequently during combat
[System.Serializable]
public class UnitStats
{
    public int HealthCurrent;
    public int HealthMax;
    public int Movement;
    public float Aim;
    public int Armor;
    public float Dodge;
    public int ActionPointsCurrent;
    public int ActionPointsMax;
    public int Sight;
}

public enum UnitType
{
    HUMAN,
    ROBOTIC
}