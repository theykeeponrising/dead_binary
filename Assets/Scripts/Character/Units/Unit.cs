using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//This class is a high-level handler for a character/unit, intended to deal with e.g. stats/attributes/faction/inventory/etc.
//Implementation of character/unit actions should be done in CharacterActor.cs
public class Unit : GridObject, IFaction, IPointerEnterHandler, IPointerExitHandler
{
    //List of units on opposing faction that are alive
    protected List<Unit> oppFactionUnits;
    public Grid grid;
    // lol
    protected CharacterActor charActor;
    protected CharacterAnimator charAnim;
    protected CharacterSFX charSFX;
    
    [HideInInspector] public Inventory inventory;
    public IFaction ifaction;
    Faction IFaction.faction { get { return attributes.faction; } set { attributes.faction = value; } }
    [HideInInspector] public Tile currentTile;
    GameState gameState;

    [HideInInspector] public CoverObject currentCover;
    public List<Actions.ActionsList> availableActions;

    // Attributes are mosty permanent descriptors about the character
    [System.Serializable] public class Attributes
    {
        public string name;
        public Faction faction;
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
        
        //Initialize the character actor
        charActor = new CharacterActor(this);

        //Init character animator
        charAnim = new CharacterAnimator(this);
        charAnim.SetRagdoll(GetComponentsInChildren<Rigidbody>());
        
        //Init character SFX
        charSFX = new CharacterSFX(this);
        charSFX.SetAudioSource(GetComponent<AudioSource>());        
    }

    // Update is called once per frame
    void Update()
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

    public void OnTurnStart()
    {
        oppFactionUnits = new List<Unit>();
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

    public List<Actions.ActionsList> GetAvailableActions()
    {
        return availableActions;
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

    public void RestoreHealth(int amount)
    {
        // Heals character by the indicated amount

        stats.healthCurrent += amount;
    }

    //Construct oppFactionUnits List
    //TODO: Should maybe put this in the PlayerTurn/EnemyTurnProcess so we don't duplicate work or something
    public List<Unit> GetOppFactionUnits()
    {
        if (oppFactionUnits != null && oppFactionUnits.Count != 0) return oppFactionUnits;
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

    
    public bool Shoot(Unit otherUnit)
    {
        // Calculate whether target hit
        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float hitChance = CalculateHitChance(this, otherUnit);
        int distance = grid.GetTileDistance(currentTile, otherUnit.currentTile);

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distance, hitChance, randomChance));

        // Return true/false if hit connected
        return (hitChance >= randomChance);
    }

    protected float CalculateExpectedDamage(Unit attacker, Unit defender, Tile attackerTile)
    {
        float weaponDamange = attacker.inventory.equippedWeapon.GetDamage();
        float hitChance = CalculateHitChance(attacker, defender, attackerTile);
        return weaponDamange * hitChance;
    }

    //Overload for simplicity
    protected float CalculateHitChance(Unit attacker, Unit defender)
    {
        return CalculateExpectedDamage(attacker, defender, attacker.currentTile);
    }

    //Calculate Hit Chance
    protected float CalculateHitChance(Unit attacker, Unit defender, Tile attackerTile)
    {
        int distance = grid.GetTileDistance(attackerTile, defender.currentTile);
        float weaponAccuracyModifier = attacker.inventory.equippedWeapon.stats.baseAccuracyModifier;

        float weaponAccuracyPenalty = attacker.inventory.equippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit - stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (currentCover && CheckIfCovered(attacker)) hitModifier -= currentCover.CoverBonus();
        
        float hitChance = (20 * attacker.stats.aim * weaponAccuracyModifier * hitModifier) / 100;
        return hitChance;
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
}