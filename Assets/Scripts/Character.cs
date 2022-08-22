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
    public List<string> flags = new List<string>();
    GameObject selectionCircle;

    [Header("--Pathfinding")]
    public Tile currentTile;
    public List<Tile> movePath;
    Tile moveTargetImmediate;
    Tile moveTargetDestination;

    public bool isAtDestination => IsAtDestination();
    private bool IsAtDestination()
    {
        bool b = moveTargetDestination == null ? true : false;
        return b;
    }

    public CoverObject currentCover;

    // Transform lookTarget; -- NOT IMPLEMENTED
    Quaternion aimTowards = Quaternion.identity;
    public Character targetCharacter;

    float velocityX = 0f;
    float velocityZ = 0f;

    InCombatPlayerAction playerAction;
    [HideInInspector] public Inventory inventory;

    [Header("--Animation")]
    Transform[] boneTransforms;
    class HumanBone
    {
        public HumanBodyBones bone;
        public HumanBone(HumanBodyBones setBone)
        {
            bone = setBone;
        }
    }
    HumanBone[] humanBones;
    Animator animator;
    AudioSource audioSource;

    enum AnimationEventContext { SHOOT, TAKE_DAMAGE, RELOAD, STOW, DRAW, VAULT, CROUCH_DOWN, CROUCH_UP };

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
    public Body body = new Body();
    Rigidbody[] ragdoll;

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
        
        //A bit of a hack to get the InCombatPlayerAction
        StateHandler stateHandler = GameObject.FindGameObjectWithTag("StateHandler").GetComponent<StateHandler>();
        PlayerTurnState playerTurnState = (PlayerTurnState) stateHandler.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
    }

    protected override void Awake()
    {
        base.Awake();
        ragdoll = GetComponentsInChildren<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Animator, bones, and body transforms
        animator = GetComponent<Animator>();
        ragdoll = GetComponentsInChildren<Rigidbody>();
        humanBones = new HumanBone[]
        {
            new HumanBone(HumanBodyBones.Chest),
            new HumanBone(HumanBodyBones.UpperChest),
            new HumanBone(HumanBodyBones.Spine)
        };
        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < humanBones.Length; i++)
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);

        body.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        body.head = animator.GetBoneTransform(HumanBodyBones.Head);
        body.handRight = animator.GetBoneTransform(HumanBodyBones.RightHand);

        audioSource = GetComponent<AudioSource>();
        inventory = GetComponent<Inventory>();
        
        if (objectTiles.Count > 0) currentTile = objectTiles[0];
        selectionCircle = transform.Find("SelectionCircle").gameObject;

        ifaction = this;
        potentialTargets = null;
    }

    // Update is called once per frame
    void Update()
    {
        SetAnimation();
        Movement();
    }

    void LateUpdate()
    {
        AimGetTarget();
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
            }
        }
    }

    void SetAnimation()
    {
        // Changes movement animation based on flags

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

            // Slow down movement speed if character is vaulting
            float distanceDelta = (flags.Contains("vaulting")) ? 0.01f : 0.03f;

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
        else if (currentCover && !AnimationPause())
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
            if (currentCover.coverSize == CoverObject.CoverSize.half && !flags.Contains("crouching"))
                AnimationTransition(AnimationEventContext.CROUCH_DOWN);
        }
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
        //stateMachine.ChangeState(new SelectedStates.Moving(stateMachine));

        // Movement routine
        // Sets "moving" flag before and removes after
        AddFlag("moving");
        animator.SetBool("moving", true);

        // Stand up if crouched
        if (flags.Contains("crouching"))
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

        // Register cover object
        if (currentTile.cover)
        {
            currentCover = currentTile.cover;
            if (currentTile.cover.coverSize == CoverObject.CoverSize.half && !flags.Contains("crouching"))
                AnimationTransition(AnimationEventContext.CROUCH_DOWN);
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
        if (flags.Contains("vaulting"))
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
                AddFlag("vaulting");
                //animator.Play("Default", inventory.equippedWeapon.weaponLayer);
                animator.Play("Default");
                animator.SetTrigger("vaulting");
                return true;
            }
        }
        return false;
    }

    bool CheckIfCovered(Character attacker)
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
            if (!flags.Contains("crouching"))
            {
                AddFlag("stowing");
                animator.Play("Stow");
                while (flags.Contains("stowing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                AnimationEvent(AnimationEventContext.STOW);
            }
            
        }

        // Equipping the new weapon
        if (weapon)
        {
            inventory.equippedWeapon = weapon;

            // Enable weapon object, set position and animation layer
            inventory.equippedWeapon.gameObject.SetActive(true);
            inventory.equippedWeapon.DefaultPosition(this);
            animator.SetLayerWeight(inventory.equippedWeapon.weaponLayer, 1);
            animator.SetFloat("animSpeed", inventory.equippedWeapon.attributes.animSpeed);

            // If crouching, do not play draw animation
            // This is until we can get a proper crouch-draw animation
            if (!flags.Contains("crouching"))
            {
                inventory.equippedWeapon.PlaySound(Weapon.WeaponSound.SWAP, this);

                AddFlag("drawing");
                //animator.Play("Draw", inventory.equippedWeapon.weaponLayer);
                animator.Play("Draw");
                while (flags.Contains("drawing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                AnimationEvent(AnimationEventContext.DRAW);
            }
        }
    }

    void ReloadAction()
    {
        // Reload action handler

        if (inventory.equippedWeapon.stats.ammoCurrent >= inventory.equippedWeapon.stats.ammoMax)
        {
            Debug.Log("Ammo is max already!"); // TO DO - Show this in UI
            return;
        }
        stats.actionPointsCurrent -= currentAction.cost;
        AddFlag("reload");
        animator.Play("Reload");
        inventory.equippedWeapon.Reload();
    }

    void AimGetTarget()
    {
        // Twists characters torso to aim gun at target
        if (!targetCharacter)
            return;
        
        // Iterations improve accuracy of aim position
        int iterations = 10;

        for (int i = 0; i < iterations; i++)
        {
            for (int b = 0; b < boneTransforms.Length; b++)
            {
                // Gets the rotation needed to point weapon at enemy
                Transform bone = boneTransforms[b];
                Vector3 targetPosition = GetTargetPosition();
                Vector3 aimDirection = inventory.equippedWeapon.transform.forward;
                Vector3 targetDirection = targetPosition - inventory.equippedWeapon.transform.position;
                

                // Updates rotation up until the actual shoot animation happens
                if (flags.Contains("aiming"))
                    aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);

                // Rotates the character to face the target
                if (Vector3.Angle(targetDirection, transform.position) > 90f) transform.LookAt(new Vector3(targetPosition.x, 0f, targetPosition.z));
                bone.rotation = (aimTowards * bone.rotation).normalized;

                // TO DO -- Fix weirdness during shoot animation
            }
        }
    }

    Vector3 GetTargetPosition()
    {
        // Gets target's position relative to the tip of the gun

        Vector3 targetDirection = targetCharacter.body.chest.position - inventory.equippedWeapon.transform.position;
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

    IEnumerator ShootWeapon(int distanceToTarget)
    {
        animator.SetBool("aiming", true);
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        //transform.LookAt(targetCharacter.transform);

        yield return new WaitForSeconds(0.5f);
        AddFlag("aiming");
        yield return new WaitForSeconds(0.25f);
        // Inflict damage on target character
        if (targetCharacter)
            targetCharacter.TakeDamage(this, inventory.equippedWeapon.stats.damage, distanceToTarget);

        AddFlag("shooting");
        animator.Play("Shoot");
        inventory.equippedWeapon.stats.ammoCurrent -= 1;

        // Wait until shoot animation completes
        while (animator.IsInTransition(inventory.equippedWeapon.weaponLayer)) yield return new WaitForSeconds(0.01f);
        while (animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Shoot")) yield return new WaitForSeconds(0.01f);

        // If target is dodging, remove flag
        if (targetCharacter) targetCharacter.RemoveFlag("dodging");

        // Add a small delay before character exits shooting stance
        yield return new WaitForSeconds(0.25f);

        // Remove shooting flag
        RemoveFlag("shooting");
        animator.SetBool("aiming", false);
        RemoveFlag("aiming");
        animator.updateMode = AnimatorUpdateMode.Normal;

        // Wait until shoot animation completes
        while (AnimatorIsPlaying()) yield return new WaitForSeconds(0.01f);
        //targetCharacter = null;
    }

    bool AnimatorIsPlaying()
    {
        // True/False whether an animation is currently playing on the equipped weapon layer.
        // Note -- lengthy transitions will not work

        return animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).length > animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).normalizedTime;
    }

    void AnimationTransition(AnimationEventContext context)
    {
        // Used to better control when circumstantial animations are played

        if (AnimationPause())
            return;

        switch (context)
        {
            case (AnimationEventContext.CROUCH_DOWN):
                ToggleCrouch(true);
                break;
            case (AnimationEventContext.CROUCH_UP):
                ToggleCrouch(false);
                break;
        }
    }

    bool AnimationPause()
    {
        // True/False if any of the listed animations are playing

        bool[] uninterruptibleAnims = { 
            animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Aiming"), 
            animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Shoot"), 
            animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Crouch-Up"),
            animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Crouch")};
        return uninterruptibleAnims.Any(x => x == true);
    }

    void AnimationEvent(AnimationEventContext context)
    {
        // Handler for animation events
        // Evaluate context and perform appropriate actions

        // Weapon shooting effect and sound
        switch (context)
        {
            // Fire weapon effect
            case (AnimationEventContext.SHOOT):
                inventory.equippedWeapon.Shoot();
                break;

            // Weapon impact effect on target
            case (AnimationEventContext.TAKE_DAMAGE):
                targetCharacter.TakeDamageEffect(inventory.equippedWeapon);
                break;

            // Stow weapon animation is completed
            case (AnimationEventContext.STOW):
                RemoveFlag("stowing");
                inventory.equippedWeapon.gameObject.SetActive(false);
                animator.SetLayerWeight(inventory.equippedWeapon.weaponLayer, 0);
                break;

            // Draw weapon animation is completed
            case (AnimationEventContext.DRAW):
                RemoveFlag("drawing");
                break;

            // Reload weapon animation is completed
            case (AnimationEventContext.RELOAD):
                RemoveFlag("reload");
                inventory.equippedWeapon.stats.ammoCurrent = inventory.equippedWeapon.stats.ammoMax;
                break;

            // Reload weapon animation is completed -- NOT YET IMPLEMENTED
            case (AnimationEventContext.VAULT):
                RemoveFlag("vaulting");
                break;

            // Crouch-down
            case (AnimationEventContext.CROUCH_DOWN):
                AddFlag("crouching");
                break;

            // Crouch-up
            case (AnimationEventContext.CROUCH_UP):
                RemoveFlag("crouching");
                break;
        }
    }

    void ToggleCrouch(bool crouching=false, bool instant=false)
    {
        // Placeholder function
        // Test crouch animation when button pressed

        if (!crouching)
        {
            animator.SetBool("crouching", false);
            if (instant) RemoveFlag("crouching");
        }
        else
        {
            animator.SetBool("crouching", true);
            if (instant) AddFlag("crouching");
        }
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

    public void RestoreHealth(int amount)
    {
        stats.healthCurrent += amount;
    }

    public void TakeDamageEffect(Weapon weapon=null)
    {
        if (flags.Contains("dodging"))
        {
            // TO DO -- Play dodging animation instead
            return;
        }

        // Get impact sound
        AudioClip impactSound = AudioManager.Instance.GetRandomImpactSound(impactType);
        audioSource.PlayOneShot(impactSound);

        // Effect shown when character is hit
        if (animator.GetCurrentAnimatorStateInfo(inventory.equippedWeapon.weaponLayer).IsName("Damage2"))
            animator.Play("Damage3", 0, normalizedTime: .1f);
        else if (weapon.weaponImpact == Weapon.WeaponImpact.HEAVY)
            animator.Play("Damage1");
        else
            animator.Play("Damage2");
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

        if (inventory.equippedWeapon)
            inventory.equippedWeapon.DropGun();

        // TO DO -- DISABLE ENEMY AI

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

        ToggleCrouch(false);
        yield return new WaitForSeconds(0.01f);
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
                        StartCoroutine(ShootWeapon(distanceToTarget));
                        RemoveFlag("targeting");
                    }
                    else
                    {
                        Debug.Log("Out of Ammo! Reload weapon"); // This will eventually be shown in UI
                    }
                } 
                else Debug.Log(string.Format("Target is too close! \nDistance: {0}, Weapon Range: {1}", distanceToTarget, minWeaponRange)); // This will eventually be shown visually instead of told
            }
    }

    public void CancelTarget()
    {
        // Removes targeting flag and combat stance

        RemoveFlag("targeting");
        animator.SetBool("aiming", false);
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
