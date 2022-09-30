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
    DODGE,
    AIM,
    STOW,
    DRAW,
    DEAD
};

public class Unit : GridObject, IFaction, IPointerEnterHandler, IPointerExitHandler
{
    //List of units on opposing faction that are alive
    protected List<Unit> oppFactionUnits;
    public MapGrid grid;
    // lol
    protected CharacterActor charActor;
    protected CharacterAnimator charAnim;
    protected CharacterSFX charSFX;

    public List<FlagType> flags = new List<FlagType>();
    
    public int numActionsInFlight = 0;
    
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Healthbar healthbar;
    public IFaction ifaction;
    Faction IFaction.faction { get { return attributes.faction; } set { attributes.faction = value; } }
    GameState gameState;

    [HideInInspector] public CoverObject currentCover;
    public List<ActionList> availableActions;

    // Attributes are mosty permanent descriptors about the character
    [System.Serializable] public class Attributes
    {
        public string name;
        public Faction faction;
        public AudioManager.FootstepSource footstepSource;
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
    
    void Start() 
    {
        
        // character = gameObject.GetComponent<Character>();
        grid = currentTile.GetGrid();

        // Characters start with full health and action points
        stats.healthCurrent = stats.healthMax;
        stats.actionPointsCurrent = stats.actionPointsMax;

        SetupUnit();
    }

    protected override void Awake()
    {
        base.Awake();
        this.name = string.Format("{0} (Character)", attributes.name);
        inventory = GetComponent<Inventory>();
        ifaction = this;
        
        if (objectTiles.Count > 0) currentTile = objectTiles[0];
        
        // Initialize the character actor
        charActor = new CharacterActor(this);

        // Init character animator
        charAnim = new CharacterAnimator(this);
        charAnim.SetRagdoll(GetComponentsInChildren<Rigidbody>());
        
        // Init character SFX
        charSFX = new CharacterSFX(this);
        charSFX.SetAudioSource(GetComponent<AudioSource>());

        // Init health bar
        healthbar = transform.Find("Healthbar").GetComponent<Healthbar>();
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

    public void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }

    public virtual void OnTurnStart()
    {
        RefreshActionPoints();
    }

    public CharacterActor GetActor()
    {
        return charActor;
    }

    public CharacterAnimator GetAnimator()
    {
        return charAnim;
    }

    public CharacterSFX GetSFX()
    {
        return charSFX;
    }

    public List<ActionList> GetAvailableActions()
    {
        return availableActions;
    }

    public List<Item> GetItems()
    {
        return inventory.items;
    }

    public void SetupUnit()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);

        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        charActor.SetPlayerAction(playerAction);
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

    public void Event_PlayAnimation(CharacterAnimator.AnimationEventContext context)
    {
        GetAnimator().Event_PlayAnimation(context);
    }

    public void Event_PlaySound(CharacterSFX.AnimationEventSound sound)
    {
        GetSFX().Event_PlaySound(sound);
    }

    public void RefreshActionPoints()
    {
        // Used to refresh character action points to max.
        stats.actionPointsCurrent = stats.actionPointsMax;
    }

    //TODO: Move this to the individual action classes, and add delegates/inherited method
    public void ActionStart(Action action)
    {
        switch (action.context)
        {
            case ActionList.MOVE:
                numActionsInFlight++;
                break;
            case ActionList.SHOOT:
                numActionsInFlight++;
                break;
            case ActionList.RELOAD:
                numActionsInFlight++;
                break;
            case ActionList.SWAP:
                numActionsInFlight++;
                break;
            case ActionList.USEITEM:
                numActionsInFlight++;
                break;
            default:
                break; 
        }
    }

    //TODO: Move this to the individual action classes, and add delegates/inherited method
    public void ActionComplete(Action action)
    {
        switch (action.context)
        {
            case ActionList.MOVE:
                numActionsInFlight--;
                break;
            case ActionList.SHOOT:
                numActionsInFlight--;
                break;
            case ActionList.RELOAD:
                numActionsInFlight--;
                break;
            case ActionList.SWAP:
                numActionsInFlight--;
                break;
            case ActionList.USEITEM:
                numActionsInFlight--;
                break;
            default:
                break; 
        }
        if (numActionsInFlight < 0) Debug.LogError(string.Format("Number of actions in flight: {0}", numActionsInFlight));
    }

    //Basic action callback
    //TODO: Move this to action classes
    public void ActionCompleteCallback()
    {

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
            if (v.GetComponent<IFaction>() != null)
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

    //Overload for simplicity
    public float CalculateHitChance(Unit attacker, Unit defender)
    {
        return CalculateHitChance(attacker, defender, attacker.currentTile);
    }

    //Calculate Hit Chance
    public float CalculateHitChance(Unit attacker, Unit defender, Tile attackerTile)
    {
        int distance = grid.GetTileDistance(attackerTile, defender.currentTile);
        float weaponAccuracyModifier = attacker.inventory.equippedWeapon.stats.baseAccuracyModifier;

        float weaponAccuracyPenalty = attacker.inventory.equippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit + attacker.stats.aim - stats.dodge - weaponAccuracyPenalty;
        // Add cover bonus if not being flanked
        if (currentCover && CheckIfCovered(attacker)) hitModifier -= currentCover.CoverBonus();
        
        float hitChance = weaponAccuracyModifier * hitModifier;
        return hitChance / 100.0f;
    }

    // Returns calculated hit chance for a given target
    public float GetCurrentHitChance()
    {
        return CalculateHitChance(this, charActor.targetCharacter);
    }

    bool RollForHit(Unit attacker, int distanceToTarget)
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

    public void TakeDamage(Unit attacker, int damage, int distanceToTarget)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        // If attacked missed, do not take damage
        if (!RollForHit(attacker, distanceToTarget))
        {
            if (currentCover) currentCover.Impact();
            AddFlag(FlagType.DODGE);
            Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
            GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DODGE, true);
            return;
        }

        Vector3 direction =  (transform.position - attacker.transform.position);
        float distance = (transform.position - attacker.transform.position).magnitude;
        CheckDeath(attacker, direction, distance, damage);
    }

    public void TakeDamage(Unit attacker, int damage, Vector3 attackPoint)
    {
        // Called by an attacking item when taking damage
        // TO DO: More complex damage reduction will be added here

        Vector3 direction = (transform.position - attackPoint);
        float distance = (transform.position - attackPoint).magnitude;
        CheckDeath(attacker, direction, distance, damage, 50f);
    }

    void CheckDeath(Unit attacker, Vector3 direction, float distance, int damage, float impactForce = 2f)
    {
        // Inflict damage on character
        Debug.Log(string.Format("{0} has attacked {1} for {2} damage!", attacker.attributes.name, attributes.name, damage)); // This will eventually be shown visually instead of told

        stats.healthCurrent -= Mathf.Min(damage, stats.healthCurrent);
        GetComponentInChildren<Healthbar>().UpdateHealthPoints();

        // Character death
        if (stats.healthCurrent <= 0) 
        {
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
        currentTile.occupant = null;
        enabled = false;

        if (inventory.equippedWeapon)
            inventory.equippedWeapon.DropGun();
    }

    public bool CheckIfCovered(Unit attacker)
    {
        // Checks if any cover objects are between character and attacker
        // Does raycast from character to attacker in order to find closest potential cover object

        // NOTE -- We use the tiles for raycast, not the characters or weapons
        // This is to prevent animations or standpoints from impacting the calculation

        //TODO: Rework this to iterate through tiles, similar to weapon line of sight logic

        Vector3 defenderPosition = currentTile.transform.position;
        Vector3 attackerPosition = attacker.currentTile.transform.position;

        Vector3 direction = (attackerPosition - defenderPosition);
        RaycastHit hit;
        Ray ray = new Ray(defenderPosition, direction);
        Debug.DrawRay(defenderPosition, direction, Color.red, 20, true); // For debug purposes
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));

        // If cover object detected, and is the target character's current cover, return true
        if (Physics.Raycast(ray, out hit, direction.magnitude * Mathf.Infinity, layerMask))
        {
            if (hit.collider.GetComponent<CoverObject>() && hit.collider.GetComponent<CoverObject>() == currentCover)
                return true;
        }
        return false;
    }

    public bool CheckIfCovered(Unit attacker, Tile defenderTile)
    {
        // Checks if any cover objects are between character and attacker
        // Does raycast from character to attacker in order to find closest potential cover object

        // NOTE -- We use the tiles for raycast, not the characters or weapons
        // This is to prevent animations or standpoints from impacting the calculation

        //TODO: Rework this to iterate through tiles, similar to weapon line of sight logic

        Vector3 defenderPosition = defenderTile.transform.position;
        Vector3 attackerPosition = attacker.currentTile.transform.position;

        Vector3 direction = (attackerPosition - defenderPosition);
        RaycastHit hit;
        Ray ray = new Ray(defenderPosition, direction);
        Debug.DrawRay(defenderPosition, direction, Color.red, 20, true); // For debug purposes
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));

        // If cover object detected, and is the target character's current cover, return true
        if (Physics.Raycast(ray, out hit, direction.magnitude * Mathf.Infinity, layerMask))
        {
            if (hit.collider.GetComponent<CoverObject>() && hit.collider.GetComponent<CoverObject>() == defenderTile.cover)
                return true;
        }
        return false;
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

    //Used by EnemyUnit to determine when to move on to the next unit
    public bool IsActing()
    {
        return numActionsInFlight > 0;
    }
}