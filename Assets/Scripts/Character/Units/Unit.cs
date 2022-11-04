using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//This class is a high-level handler for a character/unit, intended to deal with e.g. stats/attributes/faction/inventory/etc.
//Implementation of character/unit actions should be done in CharacterActor.cs
//Implementation of character/unit animation logic should be done in CharacterAnimator.cs
//Implementation of character/unit SFX logic should be in CharacterSFX.cs
public enum FlagType {
    MOVE,
    SHOOT,
    RELOAD,
    VAULT,
    AIM,
    STOW,
    DRAW,
    DEAD
};

public enum UnitType {
    HUMAN,
    ROBOTIC
}

public class Unit : GridObject, IPointerEnterHandler, IPointerExitHandler
{
    //List of units on opposing faction that are alive
    protected List<Unit> oppFactionUnits;
    public MapGrid grid;

    protected CharacterActor charActor;
    protected CharacterAnimator charAnim;
    protected CharacterSFX charSFX;
    private Rigidbody _unitRigidbody;
    private Collider[] _unitColliders;

    public List<FlagType> flags = new();
    
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Healthbar healthbar;
    [HideInInspector] public Weapon EquippedWeapon => GetEquippedWeapon();

    [HideInInspector] public CoverObject currentCover => currentTile.Cover;
    [SerializeField] private List<UnitAction> _unitActions;
    private Transform _unitActionsContainer;

    // Attributes are mosty permanent descriptors about the character
    [System.Serializable] public class Attributes
    {
        public string name;
        public Faction faction;
        public UnitType unitType;
        public UnitIconEnum unitIcon;
        public FootstepSource footstepSource;
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

    public Rigidbody UnitRigidBody { get { return _unitRigidbody; } }
    public Collider[] UnitColliders { get { return _unitColliders; } }

    protected override void Awake()
    {
        base.Awake();
        this.name = string.Format("{0} (Character)", attributes.name);
        _unitRigidbody = GetComponent<Rigidbody>();
        _unitColliders = GetComponents<Collider>();

        inventory = GetComponentInChildren<Inventory>();
        _unitActionsContainer = transform.Find("Actions");

        if (objectTiles.Count > 0) currentTile = objectTiles[0];

        // Initialize the character actor
        charActor = new CharacterActor(this);

        // Init character animator
        charAnim = new CharacterAnimator(this);

        // Init character SFX
        charSFX = new CharacterSFX(this);
        charSFX.SetAudioSource(GetComponent<AudioSource>());

        // Init health bar
        healthbar = transform.Find("Healthbar").GetComponent<Healthbar>();
    }

    private void Start() 
    {
        grid = currentTile.Grid;

        // Characters start with full health and action points
        stats.healthCurrent = stats.healthMax;
        stats.actionPointsCurrent = stats.actionPointsMax;

        SetupUnit();
        GenerateActions();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        charAnim.Update();
        charActor.Update();
    }

    void LateUpdate()
    {
        charAnim.LateUpdate();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        GetActor().OnPointerEnter(eventData);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        GetActor().OnPointerExit(eventData);
    }

    public virtual void OnTurnStart()
    {
        ResetActions(); 
        ResetActionPoints();
        GetActor()?.SetWaiting(false);
    }

    public bool HasTurnEnded()
    {
        if (stats.actionPointsCurrent == 0)
            return true;

        if (GetActor().FindActionOfType(typeof(UnitActionWait)).Performed())
            return true;

        return false;
    }

    public CharacterActor GetActor()
    { return charActor; }

    public CharacterAnimator GetAnimator()
    { return charAnim; }

    public CharacterSFX GetSFX()
    { return charSFX; }

    public List<UnitAction> GetUnitActions()
    { return _unitActions; }

    public Transform GetUnitActionsContainer()
    { return _unitActionsContainer; }

    public Weapon GetEquippedWeapon()
    { return inventory.equippedWeapon; }

    public void SetEquippedWeapon(Weapon weapon)
    { inventory.equippedWeapon = weapon; }

    public List<Item> GetItems()
    { return inventory.items; }

    public void SetupUnit()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);

        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        charActor.SetPlayerAction(playerAction);
        inventory.Init(this);
    }

    void GenerateActions()
    {
        // Adds actions to the character based on its current equipment

        int index = 0;

        // If we have movement, add move action
        if (stats.movement > 0)
        {
            _unitActions.Insert(index, ActionManager.Instance.unitActions.move);
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
            _unitActions.Insert(index, ActionManager.Instance.unitActions.reload);
            index += 1;
        }

        // If we have multiple weapons, add swap action
        if (inventory.weapons.Count > 1)
        {
            _unitActions.Insert(index, ActionManager.Instance.unitActions.swap);
            index += 1;
        }

        // If we have items, add inventory action
        if (GetItems().Count > 0)
        {
            _unitActions.Insert(index, ActionManager.Instance.unitActions.inventory);
            index += 1;
        }

        // Always add "Wait" action
        _unitActions.Insert(index, ActionManager.Instance.unitActions.wait);

        // Clone the prefabs
        for (index = 0; index < _unitActions.Count; index++)
        {
            _unitActions[index] = Instantiate(_unitActions[index], _unitActionsContainer);
        }
    }

    //TODO: Should add additional events to specifically play a sound
    //These ones probably aren't good places for that 
    public void Event_OnAnimationStart(CharacterAnimator.AnimationEventContext context)
    {
        GetAnimator().Event_OnAnimationStart(context);
    }

    public void Event_OnAnimationEnd(CharacterAnimator.AnimationEventContext context)
    {
        GetAnimator().Event_OnAnimationEnd(context);
    }

    public void Event_PlaySound(AnimationType sound)
    {
        GetSFX().Event_PlaySound(sound);
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

    public bool WouldKill(float damage)
    {
        return damage >= stats.healthCurrent;
    }

    public void RestoreHealth(int amount)
    {
        // Heals character by the indicated amount

        stats.healthCurrent += amount;
        if (stats.healthCurrent > stats.healthMax) stats.healthCurrent = stats.healthMax;
        healthbar.UpdateHealthPoints();
    }

    //Construct oppFactionUnits List
    //TODO: Should maybe put this in the PlayerTurn/EnemyTurnProcess so we don't duplicate work or something
    public List<Unit> GetOppFactionUnits()
    {
        List<Unit> oppFactionUnits = new List<Unit>();
        Unit[] gos = GameObject.FindObjectsOfType<Unit>();

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

    //TODO: Get all tiles this unit can move to
    public List<Tile> GetTilesInMoveRange()
    {
        Vector3 pos = currentTile.transform.position;
        return grid.GetTilesInRange(pos, stats.movement);
    }

    protected float CalculateExpectedDamage(Unit attacker, Unit defender, Tile attackerTile, bool debug=false)
    {
        float weaponDamange = attacker.inventory.equippedWeapon.GetDamage();
        float hitChance = CalculateHitChance(attacker, defender, attackerTile);
        if (debug) Debug.Log(string.Format("Wep Damage {0}, Hit Chance: {1}", weaponDamange, hitChance));

        return weaponDamange * hitChance;
    }

    public float CalculateHitChance(Unit attacker, Unit defender)
    {
        // Overload for simplicity
        return CalculateHitChance(attacker, defender, attacker.currentTile);
    }

    public float CalculateHitChance(Unit attacker, Unit defender, Tile attackerTile)
    {
        // Calculate Hit Chance
        int distance = grid.GetTileDistance(attackerTile, defender.currentTile);
        float weaponAccuracyModifier = attacker.inventory.equippedWeapon.Stats.BaseAccuracyModifier;
        float weaponAccuracyPenalty = attacker.inventory.equippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + attacker.stats.aim - stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (defender.currentCover && grid.CheckIfCovered(attackerTile, defender.currentTile))
            hitModifier -= defender.currentCover.GetCoverBonus();
        
        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }

    public float GetCurrentHitChance()
    {
        // Returns calculated hit chance for a given target
        return CalculateHitChance(this, charActor.targetCharacter);
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
        if (currentCover) currentCover.PlayImpactSFX();
        GetAnimator().SetTrigger("dodge");
        Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
    }

    // caution: the unit is actually taking damage in this method!
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
            AddFlag(FlagType.DEAD);
            StartCoroutine(Death(attacker, direction, distance, impactForce));
        }
    }

    IEnumerator Death(Unit attacker, Vector3 attackDirection, float distance, float impactForce)
    {
        // Disables animator, turns on ragdoll effect, and applies a small force to push the character over

        // Wait for attacker animation to complete
       // while (attacker.GetAnimator().AnimatorIsPlaying())
       while (attacker.GetFlag(FlagType.SHOOT))
            yield return new WaitForSeconds(0.01f);

        // Disable top collider
        GetComponent<CapsuleCollider>().enabled = false;
        healthbar.gameObject.SetActive(false);

        if (distance == 0) distance = 1;

        // Disable animator and top rigidbody
        GetAnimator().OnDeath(attackDirection * impactForce/distance, ForceMode.Impulse);

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (GetActor().playerAction.selectedCharacter == this)
            GetActor().playerAction.SelectAction();

        // Disable any character lights
        foreach (Light light in GetComponentsInChildren<Light>())
            light.enabled = false;

        // Remove character as a obstacle on the map
        currentTile.Occupant = null;
        enabled = false;

        if (inventory.equippedWeapon)
            inventory.equippedWeapon.DropGun();

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
            float tileDist = grid.GetTileDistance(unitTile, target.currentTile);
            if (tileDist < minDistance)
            {
                minDistance = tileDist;
                closestUnit = target;
            }
        }
        return closestUnit;
    }

    public void AddFlag(FlagType flag)
    {
        // Handler for adding new flags
        // Used to prevent duplicate flags

        if (!flags.Contains(flag))
            flags.Add(flag);
    }

    public void RemoveFlag(FlagType flag)
    {
        // Handler for adding new flags
        // Used for consistency with AddFlag function

        if (flags.Contains(flag))
            flags.Remove(flag);
    }

    public bool GetFlag(FlagType flag)
    {
        return flags.Contains(flag);
    }
}
