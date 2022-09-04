using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Character : GridObject, IPointerEnterHandler, IPointerExitHandler, IFaction

{
    // Main script for Player-Controlled characters

    [Header("-Character Attributes")]
    //[HideInInspector]
    GameObject selectionCircle;

    [Header("--Pathfinding")]
    [HideInInspector] public Tile currentTile;
    [HideInInspector] public List<Tile> movePath;
    Tile moveTargetImmediate;
    Tile moveTargetDestination;
    [HideInInspector] Grid grid;
    [HideInInspector] public bool isAtDestination => IsAtDestination();

    private bool IsAtDestination()
    {
        bool b = moveTargetDestination == null ? true : false;
        return b;
    }
    [HideInInspector] public CoverObject currentCover;

    // Transform lookTarget; -- NOT IMPLEMENTED
    [HideInInspector] public Character targetCharacter;

    public float velocityX = 0f;
    public float velocityZ = 0f;

    [HideInInspector] public InCombatPlayerAction playerAction;
    [HideInInspector] public Inventory inventory;
    
    CharacterAnimator charAnim;
    CharacterSFX charSFX;

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
    public List<Actions.ActionsList> availableActions;
    public Actions.Action currentAction;
    public IFaction ifaction;
    Faction IFaction.faction { get { return attributes.faction; } set { attributes.faction = value; } }
    public List<Character> potentialTargets;

    // Start is called before the first frame update
    void Start()
    {
        // Characters start with full health and action points
        stats.healthCurrent = stats.healthMax;
        stats.actionPointsCurrent = stats.actionPointsMax;
        PlayerTurnState playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
    }

    protected override void Awake()
    {
        base.Awake();
        this.name = string.Format("{0} (Character)", attributes.name);
        charAnim = new CharacterAnimator(this);
        charAnim.SetRagdoll(GetComponentsInChildren<Rigidbody>());
        
        charSFX = new CharacterSFX(this);
        charSFX.SetAudioSource(GetComponent<AudioSource>());

        inventory = GetComponent<Inventory>();

        
        
        if (objectTiles.Count > 0) currentTile = objectTiles[0];
        selectionCircle = transform.Find("SelectionCircle").gameObject;

        ifaction = this;
        potentialTargets = null;
    }

    public CharacterSFX GetSFX()
    {
        return charSFX;
    }

    public CharacterAnimator GetAnimator()
    {
        return charAnim;
    }

    // Update is called once per frame
    void Update()
    {
        charAnim.Update();
        Movement();
    }

    void LateUpdate()
    {
        charAnim.LateUpdate();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights unit on mouse over
        if (playerAction.selectedCharacter != this)
        {
            selectionCircle.SetActive(true);
            selectionCircle.GetComponent<Renderer>().material.color = new Color(0, 255, 0, 0.10f);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave
        if (playerAction.selectedCharacter != this)
        {
            selectionCircle.SetActive(false);
            selectionCircle.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    //TODO: Should add additional events to specifically play a sound
    //These ones probably aren't good places for that 
    public void Event_OnAnimationStart(CharacterAnimator.AnimationEventContext context)
    {
        charAnim.Event_OnAnimationStart(context);
    }

    public void Event_OnAnimationEnd(CharacterAnimator.AnimationEventContext context)
    {
        charAnim.Event_OnAnimationEnd(context);
    }

    public void Event_PlayAnimation(CharacterAnimator.AnimationEventContext context)
    {
        charAnim.Event_PlayAnimation(context);
    }

    public void Event_PlaySound(CharacterSFX.AnimationEventSound sound)
    {
        charSFX.Event_PlaySound(sound);
    }

    public Vector3 GetCharacterChestPosition()
    {
        return charAnim.GetCharacterChestPosition();
    }

    public void SelectUnit(bool selected)
    {
        // Highlights selected unit, or removes highlight if not selected

        if (selected)
        {
            selectionCircle.SetActive(true);
            selectionCircle.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            selectionCircle.SetActive(false);
            selectionCircle.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void IsTargetUX(bool isTarget, bool isPotentialTarget)
    {
        if(isPotentialTarget)
        {
            selectionCircle.SetActive(true);

            if (isTarget)
                selectionCircle.GetComponent<Renderer>().material.color = Color.red;
            else
                selectionCircle.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else
        {
            selectionCircle.SetActive(false);
            selectionCircle.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void ProcessAction(Actions.Action actionToPerform, Tile contextTile=null, List<Tile> contextPath=null, Character contextCharacter=null, string contextString=null)
    {
        // Determine if action can be performed, and perform action if so

        // Check if action is in allowed list of actions for character
        if (!availableActions.Contains(actionToPerform.tag))
        {
            Debug.Log(string.Format("{0} does not contain action {1}", this.attributes.name, actionToPerform.name));
            return;
        }    

        int actionCost = actionToPerform.cost;
        if (actionCost > stats.actionPointsCurrent)
        {
            Debug.Log("Not enough AP!"); // This will eventually be shown in UI
        }
        else
        {
            currentAction = actionToPerform;
            switch (actionToPerform.context)
            {
                case "move":
                    MoveAction(contextTile, contextPath);
                    break;
                case "shoot":
                    ShootAction(contextCharacter, contextString);
                    break;
                case "reload":
                    ReloadAction();
                    break;
                case "swap":
                    StartCoroutine(EquipWeapon(inventory.CycleWeapon()));
                    break;
                case "useItem":
                    //The item is used in the State Machine. WOrk on bringing it over here.
                    break;
            }
        }
    }

    void Movement()
    {
        // Function that actually moves target towards destination
        // If there is no move target, then this action is skipped
        
        // If we have a move target, begin moving
        if (moveTargetImmediate)
        {
            Vector3 relativePos;
            Vector3 moveTargetPoint = moveTargetDestination.standPoint;
            float distance = Vector3.Distance(transform.position, moveTargetPoint);
            velocityZ = distance / 2;

            // Slow down movement speed if character is vaulting
            float distanceDelta = (charAnim.GetFlag("vaulting")) ? 0.01f : 0.03f;

            // If the final move target is also the most immediate one, slow down move speed as we approach
            if (moveTargetDestination == moveTargetImmediate)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetPoint, distanceDelta);
                relativePos = moveTargetPoint - transform.position;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetImmediate.transform.position, distanceDelta);
                relativePos = moveTargetImmediate.transform.position - transform.position;
            }

            // Gradually rotate character to face towards move target
            if (relativePos != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(relativePos);
                toRotation.x = transform.rotation.x;
                toRotation.z = transform.rotation.z;
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10 * Time.deltaTime);
            }
        }

        // Gradually rotate character to expected look direction while behind cover
        else if (currentCover && !charAnim.AnimationPause())
        {
            // Get which the direction the cover is relative to the tile
            Vector3 lookDirection = (currentCover.transform.position - currentTile.transform.position);

            // Add the direction to the tile world space position to get a world space point to look at
            lookDirection = lookDirection + currentTile.transform.position;

            // Remove vertical position for a flat lookat point
            lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);

            // Character look at position
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection - transform.position), 3 * Time.deltaTime);

            // Crouch down if its a half-sized cover
            if (currentCover.coverSize == CoverObject.CoverSize.half && !charAnim.GetFlag("crouching"))
                ToggleCrouch(true, false);
        }
    }

    public void MoveAction(Tile newTile, List<Tile> previewPath)
    {
        // Sets the target destination tile
        // Once a path is found, begin movement routine

        if (!availableActions.Contains(Actions.ActionsList.MOVE))
            return;

        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);

        if (!charAnim.GetFlag("moving"))
        {
            if (CheckTileMove(newTile))
            {
                // If tile is occupied, we can't move there
                if (newTile.occupant)
                    movePath = null;
                else movePath = currentTile.FindCost(newTile, stats.movement);
                
                if (movePath.Count > 0)
                {
                    StartCoroutine(MoveToPath());
                    stats.actionPointsCurrent -= currentAction.cost;
                }
                
            }
        }
    }

    IEnumerator MoveToPath()
    {
        //stateMachine.ChangeState(new SelectedStates.Moving(stateMachine));

        // Movement routine
        // Sets "moving" flag before and removes after
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.MOVE, true);

        // Stand up if crouched
        if (charAnim.GetFlag("crouching"))
            ToggleCrouch(false, true);

        moveTargetDestination = movePath[movePath.Count - 1];
        currentTile.ChangeTileOccupant();
        currentCover = null;

        // Move to each tile in the provided path
        foreach (Tile path in movePath)
        {
            if (moveTargetImmediate)
                moveTargetImmediate.ChangeTileOccupant();
            moveTargetImmediate = path;

            // Wait until immediate tile is reached before moving to the next one
            while (currentTile != path)
            {
                CheckForObstacle();
                currentTile = FindCurrentTile();
                yield return new WaitForSeconds(0.01f);
            }
            path.ChangeTileOccupant(this);
            charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.VAULT, false);
        }

        // Wait until character comes to a stop before completing movement action
        if (moveTargetImmediate == moveTargetDestination)
            while (Vector3.Distance(transform.position, moveTargetImmediate.standPoint) > 0)
                yield return new WaitForSeconds(0.001f);
        else
            while (Vector3.Distance(transform.position, moveTargetImmediate.transform.position) > 0)
                yield return new WaitForSeconds(0.001f);
        transform.position = new Vector3(currentTile.standPoint.x, 0f, currentTile.standPoint.z);

        // End movement animation
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.MOVE, false);

        // Register cover object
        if (currentTile.cover)
        {
            currentCover = currentTile.cover;
            //Crouch, should maybe set a bool outside of the animator
            if (currentTile.cover.coverSize == CoverObject.CoverSize.half && !charAnim.GetFlag("crouching"))
                // charAnim.AnimationTransition(CharacterAnimator.AnimationEventContext.CROUCH_DOWN);
                ToggleCrouch(true, false);
        }
        moveTargetImmediate = null;
        moveTargetDestination = null;


       // stateMachine.ChangeState(new SelectedStates.Idle(stateMachine));
    }

    public bool CheckTileMove(Tile newTile)
    {
        // Gets the shortest tile distance to target and compares to maximum allowed moves
        // If destination is too far, abort move action

        movePath = currentTile.FindCost(newTile);
        if (movePath.Count == 0 || !newTile.isTileTraversable())
        {
            Debug.Log("No move path."); // Replace this with UI eventually
            return false;
        }
        if (movePath.Count > stats.movement)
        {
            Debug.Log(string.Format("Destination Too Far! \nDistance: {0}, Max Moves: {1}", movePath.Count, stats.movement)); // This will eventually be shown visually instead of told
            return false;
        }
        return true;
    }

    bool CheckForObstacle()
    {
        // Checks a short distance in front of character for objects in the "VaultOver" layer
        if (charAnim.GetFlag("vaulting"))
            return false;

        Vector3 direction = (moveTargetImmediate.transform.position - transform.position);
        RaycastHit hit;
        Ray ray = new Ray(transform.position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));
        float distance = 0.5f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out hit, direction.magnitude * distance, layerMask))
        {
            if (hit.collider.GetComponent<CoverObject>().canVaultOver)
            {
                charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.VAULT, true);
                //animator.Play("Default", inventory.equippedWeapon.weaponLayer);
                return true;
            }
        }
        return false;
    }

    public bool CheckIfCovered(Character attacker)
    {
        // Checks if any cover objects are between character and attacker
        // Does raycast from character to attacker in order to find closest potential cover object

        // NOTE -- We use the tiles for raycast, not the characters or weapons
        // This is to prevent animations or standpoints from impacting the calculation

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

    IEnumerator EquipWeapon(Weapon weapon)
    {
        // Character equip or swap weapons
        // Previous weapon is stowed in extra slot

        // If character has a weapon equipped currently, stow it
        if (inventory.equippedWeapon && inventory.equippedWeapon != AssetManager.Instance.weapon.noWeapon)
        {
            // If crouching, do not play stow animation
            // This is until we can get a proper crouch-stow animation
            if (!charAnim.GetFlag("crouching"))
            {
                charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.STOW, true);
                while (charAnim.GetFlag("stowing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.STOW, false);
            }
        }

        // Equipping the new weapon
        if (weapon)
        {
            inventory.equippedWeapon = weapon;

            // Enable weapon object, set position and animation layer
            inventory.equippedWeapon.gameObject.SetActive(true);
            inventory.equippedWeapon.DefaultPosition(this);
            
            charAnim.SetLayerWeight(inventory.equippedWeapon.weaponLayer, 1);
            charAnim.SetAnimationSpeed(inventory.equippedWeapon.attributes.animSpeed);

            // If crouching, do not play draw animation
            // This is until we can get a proper crouch-draw animation
            if (!charAnim.GetFlag("crouching"))
            {
                inventory.equippedWeapon.PlaySound(Weapon.WeaponSound.SWAP, this);
                charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DRAW, true);

                while (charAnim.GetFlag("drawing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DRAW, false);
            }
        }
    }

    public void ReloadAction()
    {
        // Reload action handler

        if (inventory.equippedWeapon.stats.ammoCurrent >= inventory.equippedWeapon.stats.ammoMax)
        {
            Debug.Log("Ammo is max already!"); // TO DO - Show this in UI
            return;
        }
        stats.actionPointsCurrent -= currentAction.cost;
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.RELOAD, true);
        inventory.equippedWeapon.Reload();
    }

    public Vector3 GetTargetPosition()
    {
        // Gets target's position relative to the tip of the gun

        Vector3 targetDirection = targetCharacter.GetCharacterChestPosition() - inventory.equippedWeapon.transform.position;
        Vector3 aimDirection = inventory.equippedWeapon.transform.forward;
        float blendOut = 0.0f;
        float angleLimit = 90f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50f;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return inventory.equippedWeapon.transform.position + direction;
    }

    IEnumerator ShootWeapon(int distanceToTarget, Character shootTarget=null)
    {
        // Sets the characters aiming and physics flags
        // Performs the shoot animation and inflicts damage on the target
        // When done, returns flags and physics to default

        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, true);

        // animator.SetBool("aiming", true);
        // animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        yield return new WaitForSeconds(0.5f);
        charAnim.AddFlag("aiming");
        yield return new WaitForSeconds(0.25f);

        // Inflict damage on target character
        if (shootTarget)
            shootTarget.TakeDamage(this, inventory.equippedWeapon.stats.damage, distanceToTarget);

        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.SHOOT, true);
        inventory.equippedWeapon.stats.ammoCurrent -= 1;

        // Wait until shoot state completes
        while (playerAction.stateMachine.GetCurrentState().GetType() == typeof(SelectedStates.ShootTarget)) yield return new WaitForSeconds(0.01f);

        // If target is dodging, remove flag        
        if (shootTarget && shootTarget.charAnim.GetFlag("dodging")) shootTarget.charAnim.RemoveFlag("dodging");

        // Shooting animation completed (should maybe just implement this with a callback function, honestly)
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.SHOOT, false);
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, false);
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.IDLE, true);
    }

    void ToggleCrouch(bool crouching=false, bool instant=false)
    {
        // Placeholder function
        // Test crouch animation when button pressed
        // Just call char animation for now. Left in character.cs in case of more crouching logic
        charAnim.ToggleCrouch(crouching, instant);
    }

    bool RollForHit(Character attacker, int distanceToTarget)
    {
        // Dodge change for character vs. attacker's aim

        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float weaponAccuracyModifier = attacker.inventory.equippedWeapon.stats.baseAccuracyModifier;

        float weaponAccuracyPenalty = attacker.inventory.equippedWeapon.GetAccuracyPenalty(distanceToTarget);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit - stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (currentCover && CheckIfCovered(attacker)) hitModifier -= currentCover.CoverBonus();
        
        float baseChance = (20 * attacker.stats.aim * weaponAccuracyModifier * hitModifier) / 100;
        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distanceToTarget, baseChance, randomChance));

        // Return true/false if hit connected
        return (baseChance >= randomChance);
    }

    void TakeDamage(Character attacker, int damage, int distanceToTarget)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        // If attacked missed, do not take damage
        if (!RollForHit(attacker, distanceToTarget))
        {
            Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
            charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DODGE, true);
            return;
        }

        // Inflict damage on character
        Debug.Log(string.Format("{0} has attacked {1} for {2} damage!", attacker.attributes.name, attributes.name, damage)); // This will eventually be shown visually instead of told

        Vector3 direction =  (transform.position - attacker.transform.position);
        stats.healthCurrent -= damage;

        // Character death
        if (stats.healthCurrent <= 0)
        {
            StartCoroutine(Death(attacker, direction));
            Debug.DrawRay(transform.position, direction, Color.red, 20, true); // For debug purposes
        }
    }

    public void RestoreHealth(int amount)
    {
        // Heals character by the indicated amount

        stats.healthCurrent += amount;
    }

    IEnumerator Death(Character attacker, Vector3 attackDirection, float impactForce = 2f)
    {
        // Disables animator, turns on ragdoll effect, and applies a small force to push the character over

        // Wait for attacker animation to complete
        while (attacker.GetAnimator().AnimatorIsPlaying())
            yield return new WaitForSeconds(0.01f);

        // Disable top collider
        GetComponent<CapsuleCollider>().enabled = false;

        // Disable animator and top rigidbody
        charAnim.OnDeath(attackDirection * impactForce, ForceMode.Impulse);

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (playerAction.selectedCharacter == this)
            playerAction.selectedCharacter = null;
        SelectUnit(false);

        // Disable any character lights
        foreach (Light light in GetComponentsInChildren<Light>())
            light.enabled = false;

        // Remove character as a obstacle on the map
        currentTile.occupant = null;
        this.enabled = false;

        if (inventory.equippedWeapon)
            inventory.equippedWeapon.DropGun();

        // TO DO -- DISABLE ENEMY AI

    }

    public void ShootAction(Character selectedTarget=null, string action="")
    {
        // Sets the character's target and performs action on them
        // Called by InCombatPlayerAction

        if (selectedTarget)
            if (action == "attack")
            {
                int minWeaponRange = inventory.equippedWeapon.GetMinimumRange();
                int distanceToTarget = currentTile.FindCost(selectedTarget.currentTile, 15).Count;

                //Check if target within weapon range
                if (distanceToTarget >= minWeaponRange)
                    {
                    if (inventory.equippedWeapon.stats.ammoCurrent > 0)
                    {
                        targetCharacter = selectedTarget;
                        stats.actionPointsCurrent -= currentAction.cost;
                        StartCoroutine(ShootWeapon(distanceToTarget, targetCharacter));
                        // RemoveFlag("targeting");
                    }
                    else
                    {
                        Debug.Log("Out of Ammo! Reload weapon"); // This will eventually be shown in UI
                    }
                } 
                else Debug.Log(string.Format("Target is too close! \nDistance: {0}, Weapon Range: {1}", distanceToTarget, minWeaponRange)); // This will eventually be shown visually instead of told
            }
    }

    public void GetTarget()
    {
        // Character it put into "targeting" mode
        // Target selected with left-click will have action done to it (such as attack action)

        GetComponentInChildren<CharacterCamera>().enabled = true;
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, true);
        ToggleCrouch(false);
    }


    public void ClearTarget()
    {
        // Removes targeting flag and combat stance

        GetComponentInChildren<CharacterCamera>().enabled = false;
        charAnim.ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, false);
        targetCharacter = null;
    }

    public void RefreshActionPoints()
    {
        // Used to refresh character action points to max.
        // Ideally this would be called when a player's turn is started.

        stats.actionPointsCurrent = stats.actionPointsMax;
    }
}
