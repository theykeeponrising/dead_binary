using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEditor;

public class Character : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Main script for Player-Controlled characters

    //[HideInInspector]
    public List<string> flags = new List<string>();
    GameObject selectionCircle;

    [Header("Pathfinding")]
    public Tile currentTile;
    public List<Tile> movePath;
    Tile moveTargetImmediate;
    Tile moveTargetDestination;
    Cover currentCover;
    Transform lookTarget;

    float velocityX = 0f;
    float velocityZ = 0f;

    [Header("Equipment")]
    public Weapon equippedWeapon;
    public Weapon storedWeapon;
    public Character targetCharacter;

    AssetManager assetManager;
    InCombatPlayerAction playerAction;

    [System.Serializable]
    public class Animators
    {

        public RuntimeAnimatorController animatorBase;
        public RuntimeAnimatorController animatorOverride;
    }

    [Header("Animation")]
    public Animators animators;
    Animator animator;

    enum AnimationEventContext { SHOOT, TAKE_DAMAGE, RELOAD, STOW, DRAW, VAULT }

    public class Body
    {
        public Transform handLeft;
        public Transform handRight;
        public Transform chest;
        public Transform head;
        public Transform shoulder;
        public Transform arm;
        public Transform hand;
        public Transform leg;
        public Transform shin;
        public Transform foot;
        public Transform mask;
    }
    //[HideInInspector]
    public Body body = new Body();
    Rigidbody[] ragdoll;

    [System.Serializable]
    public class Attributes
    {
        public string name;
    }

    [System.Serializable]
    public class Stats
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
    [Header("Character Info")]
    public Stats stats;
    public Attributes attributes;
    public List<Actions.ActionsList> availableActions;
    public Actions.Action currentAction;

    // Start is called before the first frame update
    void Awake()
    {
        assetManager = GameObject.FindGameObjectWithTag("GlobalManager").GetComponent<AssetManager>();
        playerAction = GameObject.FindGameObjectWithTag("Player").GetComponent<InCombatPlayerAction>();
        ragdoll = GetComponentsInChildren<Rigidbody>();

        animator = GetComponent<Animator>();
        animators.animatorBase = animator.runtimeAnimatorController;
        currentTile = FindCurrentTile();
        currentTile.ChangeTileOccupant(this, true);
        selectionCircle = transform.Find("SelectionCircle").gameObject;

        // Body parts for use in armor placement
        CharacterPart[] characterPart = GetComponentsInChildren<CharacterPart>();
        foreach (CharacterPart part in characterPart)
        {
            //if (part.bodyPart == CharacterPart.BodyPart.head)
            //    body.headArmor = part.transform;
            if (part.bodyPart == CharacterPart.BodyPart.chest)
                body.chest = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.shoulders)
            //    body.shoulderArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.arms)
            //    body.armArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.hands)
            //    body.handArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.legs)
            //    body.legArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.shins)
            //    body.shinArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.feet)
            //    body.footArmor = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.mask)
            //    body.maskArmor = part.transform;
            if (part.bodyPart == CharacterPart.BodyPart.hand_right)
            body.handRight = part.transform;
            //if (part.bodyPart == CharacterPart.BodyPart.hand_left)
            //    body.handLeft = part.transform;
        }

        // Characters start with full health and action points
        stats.healthCurrent = stats.healthMax;
        stats.actionPointsCurrent = stats.actionPointsMax;
        
        // Init starting weapons
        if (equippedWeapon)
        {
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(equippedWeapon) != null)
                equippedWeapon = Instantiate(equippedWeapon);
            equippedWeapon.gameObject.SetActive(true);
            equippedWeapon.DefaultPosition(this);
            animator.SetLayerWeight(equippedWeapon.weaponLayer, 1);
        }
        if (storedWeapon)
        {
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(storedWeapon) != null)
                storedWeapon = Instantiate(storedWeapon);
            storedWeapon.gameObject.SetActive(false);
            storedWeapon.DefaultPosition(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetAnimation();
        Movement();
    }

    public bool KeyPress(PlayerInput.ControlsActions controls, InputAction action)
    {
        // Clear any existing targeting
        CancelTarget();

        // ACTION BUTTON 1 -- Shoot
        if (action == controls.ActionButton_1 && equippedWeapon)
        {
            if (equippedWeapon && availableActions.Contains(Actions.ActionsList.SHOOT))
            {
                GetTarget("attack");
                return true;
            }
        }

        // ACTION BUTTON 2 -- Reload
        else if (action == controls.ActionButton_2 && equippedWeapon)
        {
            ProcessAction(Actions.action_reload);
            return true;
        }

        // ACTION BUTTON 3 -- Swap Gun (Temporary)
        else if (action == controls.ActionButton_3)
        {
            StartCoroutine(EquipWeapon(storedWeapon));
            return true;
        }

        // ACTION BUTTON 4 -- Refresh AP (Temporary)
        else if (action == controls.ActionButton_4)
        {
            RefreshActionPoints();
            return true;
        }

        return false;
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

    public void ProcessAction(Actions.Action actionToPerform, Tile contextTile=null, List<Tile> contextPath=null, Character contextCharacter=null, string contextString=null)
    {
        // Determine if action can be performed, and perform action if so

        // Check if action is in allowed list of actions for character
        if (!availableActions.Contains(actionToPerform.name))
        {
            Debug.Log("Invalid action");
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
            }
        }
    }

    void SetAnimation()
    {
        // Changes animation based on flags
        if (flags.Contains("moving"))
        {
            animator.SetFloat("velocityX", velocityX / GlobalManager.gameSpeed);
            animator.SetFloat("velocityZ", velocityZ / GlobalManager.gameSpeed);
        }
        else
        {
            animator.SetBool("moving", false);
            animator.SetFloat("velocityX", 0);
            animator.SetFloat("velocityZ", 0);
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

            // If the final move target is also the most immediate one, slow down move speed as we approach
            if (moveTargetDestination == moveTargetImmediate)
            {
                // Slow down movement speed if character is vaulting
                if (flags.Contains("vaulting"))
                    transform.position = Vector3.MoveTowards(transform.position, moveTargetPoint, 0.01f);
                else
                    transform.position = Vector3.MoveTowards(transform.position, moveTargetPoint, 0.03f);
                relativePos = moveTargetPoint - transform.position;
            }
            else
            {
                // Slow down movement speed if character is vaulting
                if (flags.Contains("vaulting"))
                    transform.position = Vector3.MoveTowards(transform.position, moveTargetImmediate.transform.position, 0.01f);
                else
                    transform.position = Vector3.MoveTowards(transform.position, moveTargetImmediate.transform.position, 0.03f);
                relativePos = moveTargetImmediate.transform.position - transform.position;
            }

            // Gradually rotate character to face towards move target
            if (relativePos != new Vector3(0,0,0))
            {
                Quaternion toRotation = Quaternion.LookRotation(relativePos);
                toRotation.x = transform.rotation.x;
                toRotation.z = transform.rotation.z;
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10 * Time.deltaTime);
            }
        }

        // Gradually rotate character to face towards look target
        else if (lookTarget)
        {
            Quaternion toRotation = Quaternion.LookRotation(lookTarget.position);
            toRotation.x = transform.rotation.x;
            toRotation.z = transform.rotation.z;
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10 * Time.deltaTime);
        }

        // Gradually rotate character to expected look direction while behind cover
        else if (currentCover && !flags.Contains("targeting") && !flags.Contains("shooting"))
        {
            Debug.DrawRay(currentCover.transform.position, currentCover.transform.forward, Color.green, 20f);
            transform.rotation = Quaternion.Lerp(transform.rotation, currentCover.transform.rotation, 10 * Time.deltaTime);
            ToggleCrouch(true);
        }
    }

    Tile FindCurrentTile()
    {
        // Finds the tile the character is currently standing on
        // Called during start

        Tile[] tiles = FindObjectsOfType<Tile>();

        foreach (Tile tile in tiles)
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                if (tile.CheckIfTileOccupant(this))
                {
                    return tile;
                }
        return null;
    }

    void MoveAction(Tile newTile, List<Tile> previewPath)
    {
        // Sets the target destination tile
        // Once a path is found, begin movement routine

        if (!availableActions.Contains(Actions.ActionsList.MOVE))
            return;

        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);

        if (!flags.Contains("moving"))
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
        // Movement routine
        // Sets "moving" flag before and removes after
        AddFlag("moving");
        animator.SetBool("moving", true);

        // Stand up if crouched
        if (flags.Contains("crouching"))
            ToggleCrouch();

        moveTargetDestination = movePath[movePath.Count - 1];
        currentTile.ChangeTileOccupant(this, false);
        currentCover = null;

        // Move to each tile in the provided path
        foreach (Tile path in movePath)
        {
            if (moveTargetImmediate)
                moveTargetImmediate.ChangeTileOccupant(this, false);
            moveTargetImmediate = path;
            //CheckForObstacle();

            // Wait until immediate tile is reached before moving to the next one
            while (currentTile != path)
            {
                CheckForObstacle();
                currentTile = FindCurrentTile();
                yield return new WaitForSeconds(0.01f);
            }
            path.ChangeTileOccupant(this, true);
            RemoveFlag("vaulting");
        }

        // Wait until character comes to a stop before completing movement action
        if (moveTargetImmediate == moveTargetDestination)
            while (Vector3.Distance(transform.position, moveTargetImmediate.standPoint) > 0)
                yield return new WaitForSeconds(0.001f);
        else
            while (Vector3.Distance(transform.position, moveTargetImmediate.transform.position) > 0)
                yield return new WaitForSeconds(0.001f);
        transform.position = new Vector3(currentTile.standPoint.x, 0f, currentTile.standPoint.z);

        // Clear movement flags
        RemoveFlag("moving");
        if (currentTile.cover && currentTile.cover.coverSize == Cover.CoverSize.half)
        {
            currentCover = currentTile.cover;
            ToggleCrouch();
        }
        moveTargetImmediate = null;
        moveTargetDestination = null;
    }

    bool CheckTileMove(Tile newTile)
    {
        // Gets the shortest tile distance to target and compares to maximum allowed moves
        // If destination is too far, abort move action

        movePath = currentTile.FindCost(newTile);
        if (movePath.Count == 0 || newTile.occupant)
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
        if (flags.Contains("vaulting"))
            return false;

        Vector3 direction = (moveTargetImmediate.transform.position - transform.position);
        RaycastHit hit;
        Ray ray = new Ray(transform.position, direction);
        Debug.DrawRay(transform.position, direction, Color.red, 20, true); // For debug purposes
        int layerMask = (1 << LayerMask.NameToLayer("VaultObject"));
        float distance = 0.5f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out hit, direction.magnitude * distance, layerMask))
        {
            AddFlag("vaulting");
            animator.Play("Vault-Over", equippedWeapon.weaponLayer);
            return true;
        }
        return false;
    }

    IEnumerator EquipWeapon(Weapon weapon)
    {
        // Character equip or swap weapons
        // Checks for prefab objects when equipping a new weapon
        // Previous weapon is stowed in extra slot

        // If character has a weapon equipped currently, stow it
        if (equippedWeapon && equippedWeapon != assetManager.weapon.noWeapon)
        {
            storedWeapon = equippedWeapon;
            equippedWeapon = null;

            // If crouching, do not play stow animation
            // This is until we can get a proper crouch-stow animation
            if (!flags.Contains("crouching"))
            {
                AddFlag("stowing");
                animator.Play("Stow", storedWeapon.weaponLayer);
                while (flags.Contains("stowing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
                AnimationEvent(AnimationEventContext.STOW);
            
        }

        // Equipping the new weapon
        if (weapon)
        {
            // If prefab, clone the object
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(weapon) != null)
                equippedWeapon = Instantiate(weapon);
            else
                equippedWeapon = weapon;

            // Enable weapon object, set position and animation layer
            equippedWeapon.gameObject.SetActive(true);
            equippedWeapon.DefaultPosition(this);
            animator.SetLayerWeight(equippedWeapon.weaponLayer, 1);

            // If crouching, do not play draw animation
            // This is until we can get a proper crouch-draw animation
            if (!flags.Contains("crouching"))
            {
                AddFlag("drawing");
                animator.Play("Draw", equippedWeapon.weaponLayer);
                while (flags.Contains("drawing"))
                    yield return new WaitForSeconds(0.01f);
            }
                        else
                AnimationEvent(AnimationEventContext.DRAW);
        }
    }

    void ReloadAction()
    {
        // Reload action handler

        if (equippedWeapon.stats.ammoCurrent >= equippedWeapon.stats.ammoMax)
        {
            Debug.Log("Ammo is max already!"); // TO DO - Show this in UI
            return;
        }
        stats.actionPointsCurrent -= currentAction.cost;
        StartCoroutine(ReloadWeapon());
    }

    IEnumerator ReloadWeapon()
    {
        // Reload animation

        AddFlag("reload");
        animator.Play("Reload", equippedWeapon.weaponLayer);
        equippedWeapon.Reload();
        while (AnimatorIsPlaying())
            yield return new WaitForSeconds(0.01f);
        RemoveFlag("reload");
        equippedWeapon.stats.ammoCurrent = equippedWeapon.stats.ammoMax;
    }

    IEnumerator ShootWeapon()
    {
        // Inflict damage on target character
        if (targetCharacter)
            targetCharacter.TakeDamage(this, equippedWeapon.stats.damage);

        AddFlag("shooting");
        animator.Play("Shoot", equippedWeapon.weaponLayer);
        equippedWeapon.stats.ammoCurrent -= 1;
        transform.LookAt(targetCharacter.transform);

        // Wait until shoot animation completes
        while (AnimatorIsPlaying())
        {
            yield return new WaitForSeconds(0.01f);
        }

        // Add a small delay before character exits shooting stance
        yield return new WaitForSeconds(1.0f);

        // Remove shooting flag
        RemoveFlag("shooting");

        // If target is dodging, remove flag
        targetCharacter.RemoveFlag("dodging");
    }

    bool AnimatorIsPlaying()
    {
        // True/False whether an animation is currently playing on the equipped weapon layer.

        return animator.GetCurrentAnimatorStateInfo(equippedWeapon.weaponLayer).length > animator.GetCurrentAnimatorStateInfo(2).normalizedTime;
    }

    void AnimationEvent(AnimationEventContext context)
    {
        // Handler for animation events
        // Evaluate context and perform appropriate actions

        // Weapon shooting effect and sound
        if (context == AnimationEventContext.SHOOT)
        {
            equippedWeapon.Shoot();
        }

        // Weapon impact effect on target
        else if (context == AnimationEventContext.TAKE_DAMAGE)
        {
            targetCharacter.TakeDamageEffect();
        }

        // Stow weapon animation is completed
        else if (context == AnimationEventContext.STOW)
        {
            RemoveFlag("stowing");
            storedWeapon.gameObject.SetActive(false);
            animator.SetLayerWeight(storedWeapon.weaponLayer, 0);
        }

        // Draw weapon animation is completed
        else if (context == AnimationEventContext.DRAW)
        {
            RemoveFlag("drawing");
        }

        // Reload weapon animation is completed -- NOT YET IMPLEMENTED
        else if (context == AnimationEventContext.RELOAD)
        {
            RemoveFlag("reloading");
        }

        // Reload weapon animation is completed -- NOT YET IMPLEMENTED
        else if (context == AnimationEventContext.VAULT)
        {
            RemoveFlag("vaulting");
        }
    }

    void ToggleCrouch(bool crouching)
    {
        // Placeholder function
        // Test crouch animation when button pressed

        if (!crouching)
        {
            animator.SetBool("crouching", false);
            RemoveFlag("crouching");
        }
        else
        {
            animator.SetBool("crouching", true);
            AddFlag("crouching");
        }
    }

    void ToggleCrouch()
    {
        // Placeholder function
        // Test crouch animation when button pressed

        if (flags.Contains("crouching"))
        {
            animator.SetBool("crouching", false);
            RemoveFlag("crouching");
        }
        else
        {
            animator.SetBool("crouching", true);
            AddFlag("crouching");
        }
    }

    void ToggleCombat()
    {
        // Placeholder function
        // Test combat transition animation when button pressed

        if (flags.Contains("combat"))
        {
            animator.runtimeAnimatorController = animators.animatorBase;
            if (!flags.Contains("crouching"))
                animator.Play("Combat-Transition", equippedWeapon.weaponLayer);
            RemoveFlag("combat");
        }
        else
        {
            animator.runtimeAnimatorController = animators.animatorOverride;
            if (!flags.Contains("crouching"))
                animator.Play("Combat-Transition", equippedWeapon.weaponLayer);
            AddFlag("combat");
        }
    }

    void ToggleCombat(bool inCombat)
    {
        // Placeholder function
        // Test combat transition animation when button pressed

        // Leaving combat
        if (!inCombat && flags.Contains("combat"))
        {
            animator.runtimeAnimatorController = animators.animatorBase;
            if (!flags.Contains("crouching"))
                animator.Play("Combat-Transition", equippedWeapon.weaponLayer);
            RemoveFlag("combat");
        }

        // Entering combat
        else if (inCombat && !flags.Contains("combat"))
        {
            animator.runtimeAnimatorController = animators.animatorOverride;
            if (!flags.Contains("crouching"))
                animator.Play("Combat-Transition", equippedWeapon.weaponLayer);
            AddFlag("combat");
        }
    }

    bool RollForHit(Character attacker)
    {
        // Dodge change for character vs. attacker's aim

        // Dice roll performed
        float baseChance;
        int randomChance = Random.Range(1, 100);
        float weaponAccuracyModifier = attacker.equippedWeapon.stats.accuracyModifier;

        // Calculate chance to be hit
        if (currentCover)
            baseChance = weaponAccuracyModifier * attacker.stats.aim * 20 * ((GlobalManager.globalHit - currentCover.CoverBonus() - stats.dodge) / 100);
        else
            baseChance = weaponAccuracyModifier * attacker.stats.aim * 20 * ((GlobalManager.globalHit - stats.dodge) / 100);

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Base chance to hit: {0}%, Dice roll: {1}", baseChance, randomChance));

        // Return true/false if hit connected
        return (baseChance >= randomChance);
    }

    void TakeDamage(Character attacker, int damage)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        // If attacked missed, do not take damage
        if (!RollForHit(attacker))
        {
            Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
            AddFlag("dodging");
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

    public void TakeDamageEffect()
    {
        if (flags.Contains("dodging"))
        {
            // TO DO -- Play dodging animation instead
            return;
        }

        // Effect shown when character is hit
        if (animator.GetCurrentAnimatorStateInfo(equippedWeapon.weaponLayer).IsName("Damage2"))
            animator.Play("Damage3", equippedWeapon.weaponLayer, .1f);
        else
            animator.Play("Damage2", equippedWeapon.weaponLayer);
    }

    IEnumerator Death(Character attacker, Vector3 attackDirection, float impactForce = 2f)
    {
        // Wait for attacker animation to complete
        while (attacker.AnimatorIsPlaying())
            yield return new WaitForSeconds(0.01f);

        // Disable top collider
        GetComponent<CapsuleCollider>().enabled = false;

        // Disable animator and top rigidbody
        animator.enabled = false;
        Destroy(ragdoll[0]);
        
        // Enable bodypart physics for the ragdoll effect
        foreach (Rigidbody rag in ragdoll)
        {
            rag.isKinematic = false;
            rag.GetComponent<Collider>().isTrigger = false;
        }

        // Apply impact force to center of mass
        body.chest.GetComponent<Rigidbody>().AddForce(attackDirection * impactForce, ForceMode.Impulse);
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (playerAction.selectedCharacter == this)
            playerAction.selectedCharacter = null;
        SelectUnit(false);

        // Remove character as a obstacle on the map
        currentTile.occupant = null;
        this.enabled = false;

        if (equippedWeapon)
            equippedWeapon.DropGun();

        // TO DO -- DISABLE ENEMY AI
        // TO DO -- DROP WEAPON
    }

    void GetTarget(string action)
    {
        // Character it put into "targeting" mode
        // Target selected with left-click will have action done to it (such as attack action)

        if (!flags.Contains("targeting"))
        {
            AddFlag("targeting");
            StartCoroutine(StandAndShoot());
            playerAction.clickAction = InCombatPlayerAction.ClickAction.target;
            playerAction.clickContext = action;
        }
    }

    IEnumerator StandAndShoot()
    {
        // Makes character fully stand before shooting to prevent animation skipping

        ToggleCombat(true);
        ToggleCrouch(false);
        yield return new WaitForSeconds(0.01f);
    }

    void ShootAction(Character selectedTarget=null, string action="")
    {
        // Sets the character's target and performs action on them
        // Called by InCombatPlayerAction

        if (selectedTarget)
            if (action == "attack")
            {
                int weaponRange = equippedWeapon.GetRange();
                int distanceToTarget = currentTile.FindCost(selectedTarget.currentTile, 15).Count;

                //Check if target within weapon range
                if (distanceToTarget <= weaponRange && distanceToTarget > 0)
                    {
                    if (equippedWeapon.stats.ammoCurrent > 0)
                    {
                        targetCharacter = selectedTarget;
                        stats.actionPointsCurrent -= currentAction.cost;
                        StartCoroutine(ShootWeapon());
                        RemoveFlag("targeting");
                    }
                    else
                    {
                        Debug.Log("Out of Ammo! Reload weapon"); // This will eventually be shown in UI
                    }
                } 
                else Debug.Log(string.Format("Target is out of range! \nDistance: {0}, Weapon Range: {1}", distanceToTarget, weaponRange)); // This will eventually be shown visually instead of told
            }
    }

    public void CancelTarget()
    {
        // Removes targeting flag and combat stance

        RemoveFlag("targeting");
        ToggleCombat(false);
    }

    public void RefreshActionPoints()
    {
        // Used to refresh character action points to max.
        // Ideally this would be called when a player's turn is started.

        stats.actionPointsCurrent = stats.actionPointsMax;
    }

    void AddFlag(string flag)
    {
        // Handler for adding new flags
        // Used to prevent duplicate flags

        if (!flags.Contains(flag))
            flags.Add(flag);
    }

    void RemoveFlag(string flag)
    {
        // Handler for adding new flags
        // Used for consistency with AddFlag function

        if (flags.Contains(flag))
            flags.Remove(flag);
    }
}
