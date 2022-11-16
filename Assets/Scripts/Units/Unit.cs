using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//This class is a high-level handler for a character/unit, intended to deal with e.g. stats/attributes/faction/inventory/etc.
//Implementation of character/unit actions should be done in CharacterActor.cs
//Implementation of character/unit animation logic should be done in CharacterAnimator.cs
//Implementation of character/unit SFX logic should be in CharacterSFX.cs

public class Unit : GridObject, IPointerEnterHandler, IPointerExitHandler
{
    //List of units on opposing faction that are alive
    protected List<Unit> oppFactionUnits;

    private AudioSource _audioSource;
    private UnitActor _unitActor;
    private UnitAnimator _unitAnimator;
    private UnitSFX _unitSFX;
    private UnitCombat _unitCombat;

    private Inventory _inventory;
    private Healthbar _healthbar;

    private Rigidbody _rigidbody;
    private Collider[] _colliders;
    private Transform _actionsContainer;

    private InCombatPlayerAction _playerAction;
    [SerializeField] private List<UnitAction> _unitActions;

    private readonly List<AnimationFlag> _animationFlags = new();
    
    public Rigidbody Rigidbody { get { return _rigidbody; } }
    public Collider[] Colliders { get { return _colliders; } }
    public Inventory Inventory { get { return _inventory; } }
    public Healthbar Healthbar { get { return _healthbar; } }
    public CoverObject CurrentCover { get { return Tile.Cover; } }
    public Weapon EquippedWeapon { get { return Inventory.equippedWeapon; } set { Inventory.EquipWeapon(value); } }
    public Unit TargetUnit { get { return _unitCombat.TargetUnit; } set { _unitCombat.TargetUnit = value; } }
    public List<Unit> PotentialTargets { get { return _unitCombat.PotentialTargets; } set { _unitCombat.PotentialTargets = value; } }
    public InCombatPlayerAction PlayerAction { get { return _playerAction; } }
    public MoveData MoveData { get { return _unitActor.MoveData; } set { _unitActor.MoveData = value; } }

    // Attributes are mosty permanent descriptors about the character
    [System.Serializable] public class Attributes
    {
        public string name;
        public Faction faction;
        public UnitType unitType;
        public UnitIconEnum unitIcon;
        public FootstepSource footstepSource;
        public UnitPortrait UnitPortrait;
        public DialogVoice UnitVoice;
        [Range(0.01f, 3f)] public float UnitVoicePitch = 1f;
    }

    // Stats are values that will be referenced and changed frequently during combat
    [System.Serializable] public class Stats
    {
        public int healthCurrent;
        public int healthMax;
        public int movement;
        public float aim;
        public int armor;
        public float dodge;
        public int actionPointsCurrent;
        public int actionPointsMax;
    }

    [Header("--Character Info")]
    public Stats stats;
    public Attributes attributes;

    public float velocityX = 0f;
    public float velocityZ = 0f;
    
    public event System.Action OnHealthModified;
    public event System.Action OnUnitDied;

    protected override void Awake()
    {
        base.Awake();
        this.name = string.Format("{0} (Character)", attributes.name);

        _audioSource = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();
        _colliders = GetComponents<Collider>();
        _actionsContainer = transform.Find("Actions");

        _unitActor = new UnitActor(this);
        _unitAnimator = new UnitAnimator(this);
        _unitSFX = new UnitSFX(this, _audioSource);
        _unitCombat = new UnitCombat(this);

        _inventory = GetComponentInChildren<Inventory>();
        _healthbar = GetComponentInChildren<Healthbar>();
    }

    protected override void Start() 
    {
        base.Start();
        if (objectTiles.Count > 0) Tile = objectTiles[0];

        // Characters start with full health and action points
        stats.healthCurrent = stats.healthMax;
        stats.actionPointsCurrent = stats.actionPointsMax;

        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        _playerAction = playerTurnState.GetPlayerAction();
        _inventory.Init(this);

        GenerateActions();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        _unitAnimator.Update();
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
        if (stats.actionPointsCurrent == 0)
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
    { return Inventory.items; }

    public void Say(string dialog)
    { _unitActor.Say(dialog); }

    public void GetTarget(bool useCharacterCamera=false)
    { _unitCombat.GetTarget(useCharacterCamera); }

    public void ClearTarget()
    { _unitCombat.ClearTarget(); }

    public bool IsActing()
    { return _unitActor.IsActing(); }

    public UnitAction FindActionOfType(System.Type actionType)
    { return _unitActor.FindActionOfType(actionType); }

    public Vector3 GetTargetPosition(bool snapToTarget=false)
    { return _unitCombat.GetTargetPosition(snapToTarget); }

    public void SelectUnit(SelectionType selectionType)
    { _unitActor.SelectUnit(selectionType); }

    public void UseItem(Item item, Unit target)
    { _unitActor.ItemAction(item, target); }

    public void UseItem(Item item, Tile target)
    { _unitActor.ItemAction(item, target); }

    public void UpdateHitStats()
    { _unitCombat.UpdateHitStats(); }

    public void SetWaiting(bool isWaiting)
    { _unitActor.SetWaiting(isWaiting); }

    public Transform GetBoneTransform(HumanBodyBones bone)
    { return _unitAnimator.GetBoneTransform(bone); }

    public void ToggleCrouch(bool instant=false)
    { _unitAnimator.ToggleCrouch(instant); }

    public bool IsCrouching()
    { return _unitAnimator.IsCrouching(); }

    public void CoverCrouch()
    { _unitAnimator.CoverCrouch(); }

    public bool IsDodging()
    { return _unitAnimator.IsDodging(); }

    public bool IsVaulting()
    { return _unitAnimator.IsVaulting(); }

    public void ProcessAnimationEvent(AnimationEventContext animationEvent, bool state)
    { _unitAnimator.ProcessAnimationEvent(animationEvent, state); }

    public void SetAnimatorBool(string name, bool state)
    { _unitAnimator.SetBool(name, state); }

    public void SetAnimatorMode(AnimatorUpdateMode updateMode=AnimatorUpdateMode.Normal)
    { _unitAnimator.SetUpdateMode(updateMode); }

    public void SetAnimatorTrigger(string name)
    { _unitAnimator.SetTrigger(name); }

    public void OnDeath(Vector3 forceDirection, ForceMode forceMode)
    { _unitAnimator.OnDeath(forceDirection, forceMode); }

    public Transform GetWeaponAttachPoint()
    { return _unitAnimator.GetWeaponAttachPoint(); }

    public void TakeDamageEffect(Weapon weapon=null, DamageItem item=null)
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
    { _unitSFX.Event_PlaySound(soundType); }

    private void GenerateActions()
    {
        // Adds actions to the character based on its current equipment

        int index = 0;

        // If we have movement, add move action
        if (stats.movement > 0)
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
        if (Inventory.weapons.Count > 1)
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

        stats.actionPointsCurrent -= amount;
    }

    public void ResetActionPoints()
    {
        // Used to refresh character action points to max
        // TO-DO -- Add AP penalties here from debuffs

        stats.actionPointsCurrent = stats.actionPointsMax;
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
        return stats.healthCurrent;
    }

    public void RestoreHealth(int amount)
    {
        // Heals character by the indicated amount

        stats.healthCurrent += amount;
        if (stats.healthCurrent > stats.healthMax) stats.healthCurrent = stats.healthMax;
        Healthbar.UpdateHealthPoints();
    }

    public List<Unit> GetOppFactionUnits()
    {
        List<Unit> oppFactionUnits = new();
        List<Unit> gos = Map.FindUnits();

        foreach (var v in gos)
        {
            if (attributes.faction != v.attributes.faction)
                if (v.stats.healthCurrent > 0)
                    oppFactionUnits.Add(v);
        }

        //Sort list by distance to current unit
        if(oppFactionUnits.Count > 0)
        {
            oppFactionUnits.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(transform.position, a.transform.position).CompareTo(
                    Vector2.Distance(transform.position, b.transform.position));
            });
        }
        return oppFactionUnits;
    }

    public List<Tile> GetTilesInMoveRange()
    {
        Vector3 pos = Tile.transform.position;
        return Map.MapGrid.GetTilesInRange(pos, stats.movement);
    }

    protected float CalculateExpectedDamage(Unit attacker, Unit defender, Tile attackerTile, bool debug=false)
    {
        float weaponDamange = attacker.EquippedWeapon.GetDamage();
        float hitChance = CalculateHitChance(attacker, defender, attackerTile);
        if (debug) Debug.Log(string.Format("Wep Damage {0}, Hit Chance: {1}", weaponDamange, hitChance));

        return weaponDamange * hitChance;
    }

    public float CalculateHitChance(Unit attacker, Unit defender)
    {
        // Overload for simplicity
        return CalculateHitChance(attacker, defender, attacker.Tile);
    }

    public float CalculateHitChance(Unit attacker, Unit defender, Tile attackerTile)
    {
        // Calculate Hit Chance
        int distance = Map.MapGrid.GetTileDistance(attackerTile, defender.Tile);
        float weaponAccuracyModifier = attacker.EquippedWeapon.Stats.BaseAccuracyModifier;
        float weaponAccuracyPenalty = attacker.EquippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + attacker.stats.aim - stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (defender.CurrentCover && Map.MapGrid.CheckIfCovered(attackerTile, defender.Tile))
            hitModifier -= defender.CurrentCover.GetCoverBonus();
        
        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }

    public float GetHitChance()
    {
        // Returns calculated hit chance for a given target
        return _unitCombat.CalculateHitChance();
    }

    public bool RollForHit(Unit attacker, int distanceToTarget)
    {
        // Dodge change for character vs. attacker's aim

        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float hitChance = CalculateHitChance(attacker, this);
        float baseChance = hitChance * 100.0f;

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distanceToTarget, baseChance, randomChance));

        // Return true/false if hit connected
        return (baseChance  >= randomChance);
    }

    public virtual void TakeDamage(Unit attacker, int damage, MessageType damageType = MessageType.DMG_CONVENTIONAL)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        Vector3 direction =  (transform.position - attacker.transform.position);
        float distance = (transform.position - attacker.transform.position).magnitude;

        CheckDeath(attacker, direction, distance, damage);
    }

    public virtual void TakeDamage(Unit attacker, int damage, Vector3 attackPoint, MessageType damageType = MessageType.DMG_CONVENTIONAL)
    {
        // Called by an attacking item when taking damage
        // TO DO: More complex damage reduction will be added here

        Vector3 direction = transform.position - attackPoint;
        float distance = direction.magnitude;

        CheckDeath(attacker, direction, distance, damage, 50f);
    }

    public void DodgeAttack(Unit attacker)
    {
        if (CurrentCover) CurrentCover.PlayImpactSFX();
        SetAnimatorTrigger("dodge");
        Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
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
        Debug.Log($"{attacker.attributes.name} has attacked {attributes.name} for {damage} damage!");

        stats.healthCurrent -= Mathf.Min(damage, stats.healthCurrent);

        OnHealthModified?.Invoke();
        GetComponentInChildren<Healthbar>().UpdateHealthPoints();

        // Character death
        if (stats.healthCurrent <= 0) 
        {
            OnUnitDied?.Invoke();
            AddFlag(AnimationFlag.DEAD);
            StartCoroutine(Death(attacker, direction, distance, impactForce));
        }
    }

    IEnumerator Death(Unit attacker, Vector3 attackDirection, float distance, float impactForce)
    {
        // Disables animator, turns on ragdoll effect, and applies a small force to push the character over

        // Wait for attacker animation to complete
       // while (attacker.GetAnimator().AnimatorIsPlaying())
       while (attacker.GetFlag(AnimationFlag.SHOOT))
            yield return new WaitForSeconds(0.01f);

        // Disable top collider
        GetComponent<CapsuleCollider>().enabled = false;
        Healthbar.gameObject.SetActive(false);

        if (distance == 0) distance = 1;

        // Disable animator and top rigidbody
        OnDeath(attackDirection * impactForce/distance, ForceMode.Impulse);

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (_playerAction.selectedCharacter == this)
            _playerAction.SelectAction();

        // Disable any character lights
        foreach (Light light in GetComponentsInChildren<Light>())
            light.enabled = false;

        // Remove character as a obstacle on the map
        Tile.Occupant = null;
        enabled = false;

        if (EquippedWeapon)
            EquippedWeapon.DropGun();

        // Display message
        if (attacker.attributes.faction == FactionManager.ACS)
            UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.PV_DEATH);

        else if (attributes.faction == FactionManager.ACS)
            UIManager.GetTurnIndicator().SetTurnIndicatorMessage(MessageType.ACS_DEATH);
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

    public void AddFlag(AnimationFlag flag)
    {
        // Handler for adding new flags
        // Used to prevent duplicate flags

        if (!_animationFlags.Contains(flag))
            _animationFlags.Add(flag);
    }

    public void RemoveFlag(AnimationFlag flag)
    {
        // Handler for adding new flags
        // Used for consistency with AddFlag function

        if (_animationFlags.Contains(flag))
            _animationFlags.Remove(flag);
    }

    public bool GetFlag(AnimationFlag flag)
    {
        return _animationFlags.Contains(flag);
    }
}

public enum UnitType
{
    HUMAN,
    ROBOTIC
}