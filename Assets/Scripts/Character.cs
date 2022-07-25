using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Character : MonoBehaviour
{
    // Main script for Player-Controlled characters

    //[HideInInspector]
    public List<string> flags = new List<string>();
    GameObject selectionCircle;

    public Tile currentTile;
    public List<Tile> movePath;
    Tile moveTargetImmediate;
    Tile moveTargetDestination;

    float velocityX = 0f;
    float velocityZ = 0f;

    public Weapon equippedWeapon;
    public Weapon storedWeapon;
    public Character targetCharacter;

    AssetManager assetManager;
    ClickHandler clickHandler;

    [System.Serializable]
    public class Animators
    {

        public RuntimeAnimatorController animatorBase;
        public RuntimeAnimatorController animatorOverride;
    }
    public Animators animators;
    Animator animator;

    enum AnimationEventContext { SHOOT, TAKE_DAMAGE, RELOAD, STOW, DRAW }

    public class Body
    {
        public Transform handLeft;
        public Transform handRight;
        public Transform chestArmor;
        public Transform headArmor;
        public Transform shoulderArmor;
        public Transform armArmor;
        public Transform handArmor;
        public Transform legArmor;
        public Transform shinArmor;
        public Transform footArmor;
        public Transform maskArmor;
    }
    //[HideInInspector]
    public Body body = new Body();

    [System.Serializable]
    public class Attributes
    {
        public string name;
    }
    public Attributes attributes;

    [System.Serializable]
    public class Stats
    {
        public int health;
        public int movement;
        public float aim;
        public int armor;
        public float dodge;
    }
    public Stats stats;

    // Start is called before the first frame update
    void Start()
    {
        assetManager = GameObject.FindGameObjectWithTag("GlobalManager").GetComponent<AssetManager>();

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
            //if (part.bodyPart == CharacterPart.BodyPart.chest)
            //    body.chestArmor = part.transform;
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

    public void KeyPress(KeyCode keycode, ClickHandler incomingHandler)
    {
        // Handler for keypress

        clickHandler = incomingHandler;

        // TEMP WEAPON SPAWN TEST -- NO LONGER NEEDED
        //if (keycode == KeyCode.Z) 
        //    StartCoroutine(EquipWeapon(assetManager.weapon.ar));
        //else if (keycode == KeyCode.X)
        //    StartCoroutine(EquipWeapon(assetManager.weapon.pistol));

        // Clear any existing targeting
        CancelTarget();

        if (keycode == KeyCode.C)
            StartCoroutine(EquipWeapon(storedWeapon));
        else if (keycode == KeyCode.V)
            ToggleCrouch();
        else if (keycode == KeyCode.T)
            ToggleCombat();
        else if (keycode == KeyCode.R && equippedWeapon)
            StartCoroutine(ReloadWeapon());
        else if (keycode == KeyCode.F && equippedWeapon)
            GetTarget("attack");
    }

    private void OnMouseOver()
    {
        // Highlights unit on mouse over

        ClickHandler handler = FindObjectOfType<ClickHandler>();
        if (handler.selectedCharacter != this)
        {
            selectionCircle.SetActive(true);
            selectionCircle.GetComponent<Renderer>().material.color = new Color(0, 255, 0, 0.10f);
        }
    }

    private void OnMouseExit()
    {
        // Clears unit highlight on mouse leave

        ClickHandler handler = FindObjectOfType<ClickHandler>();
        if (handler.selectedCharacter != this)
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

    void SetAnimation()
    {
        // Changes animation based on flags

        if (flags.Contains("moving"))
        {
            animator.SetBool("moving", true);
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

        if (moveTargetImmediate)
        {
            Vector3 relativePos;
            Vector3 moveTargetPoint = moveTargetDestination.standPoint;
            float distance = Vector3.Distance(transform.position, moveTargetPoint);
            velocityZ = distance / 2;

            if (moveTargetDestination == moveTargetImmediate)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetPoint, 0.03f);
                relativePos = moveTargetPoint - transform.position;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetImmediate.transform.position, 0.03f);
                relativePos = moveTargetImmediate.transform.position - transform.position;
            }
            if (relativePos != new Vector3(0,0,0))
            {
                Quaternion toRotation = Quaternion.LookRotation(relativePos);
                toRotation.x = transform.rotation.x;
                toRotation.z = transform.rotation.z;
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10 * Time.deltaTime);
            }
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

    public void SetTile(Tile newTile)
    {
        // Sets the target destination tile
        // Once a path is found, begin movement routine

        if (!flags.Contains("moving"))
        {
            if (CheckTileMove(newTile))
            {
                movePath = currentTile.FindCost(newTile);
                StartCoroutine(MoveToPath());
            }
        }
    }

    IEnumerator MoveToPath()
    {
        // Movement routine
        // Sets "moving" flag before and removes after

        //Stand up if crouched
        if (flags.Contains("crouching"))
            ToggleCrouch();

        AddFlag("moving");
        moveTargetDestination = movePath[movePath.Count - 1];
        currentTile.ChangeTileOccupant(this, false);

        // Move to each tile in the provided path
        foreach (Tile path in movePath)
        {
            if (moveTargetImmediate)
                moveTargetImmediate.ChangeTileOccupant(this, false);
            moveTargetImmediate = path;

            // Wait until immediate tile is reached before moving to the next one
            while (currentTile != path)
            {
                currentTile = FindCurrentTile();
                yield return new WaitForSeconds(0.01f);
            }
            path.ChangeTileOccupant(this, true);
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
            ToggleCrouch();
        moveTargetImmediate = null;
        moveTargetDestination = null;
    }

    bool CheckTileMove(Tile newTile)
    {
        // Gets the shortest tile distance to target and compares to maximum allowed moves
        // If destination is too far, abort move action

        movePath = currentTile.FindCost(newTile);
        if (movePath == null)
        {
            Debug.Log("No move path.");
            return false;
        }
        if (movePath.Count > stats.movement)
        {
            Debug.Log(string.Format("Destination Too Far! \nDistance: {0}, Max Moves: {1}", movePath.Count, stats.movement));
            return false;
        }
        return true;
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

    IEnumerator ReloadWeapon()
    {
        // Reload animation

        AddFlag("reload");
        animator.Play("Reload", equippedWeapon.weaponLayer);
        equippedWeapon.Reload();
        while (animator.IsInTransition(equippedWeapon.weaponLayer))
            yield return new WaitForSeconds(0.01f);
        RemoveFlag("reload");
        equippedWeapon.stats.ammoCurrent = equippedWeapon.stats.ammoMax;
    }

    IEnumerator ShootWeapon()
    {
        // Placeholder function
        // Test shoot animation when button pressed

        AddFlag("shooting");
        animator.Play("Shoot", equippedWeapon.weaponLayer);
        equippedWeapon.stats.ammoCurrent -= 1;
        while (animator.IsInTransition(equippedWeapon.weaponLayer))
            yield return new WaitForSeconds(0.01f);
        RemoveFlag("shooting");

        if (targetCharacter)
            targetCharacter.TakeDamage(this, equippedWeapon.stats.damage);
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

    void TakeDamage(Character attacker, int damage)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        Debug.Log(string.Format("{0} has attacked {1} for {2} damage!", attacker.attributes.name, attributes.name, damage));
        stats.health -= damage;
    }

    public void TakeDamageEffect()
    {
        // Effect shown when character is hit

        if (animator.GetCurrentAnimatorStateInfo(equippedWeapon.weaponLayer).IsName("Damage2"))
            animator.Play("Damage3", equippedWeapon.weaponLayer, .1f);
        else
            animator.Play("Damage2", equippedWeapon.weaponLayer);
    }

    void GetTarget(string action)
    {
        // Character it put into "targeting" mode
        // Target selected with left-click will have action done to it (such as attack action)

        if (!flags.Contains("targeting"))
        {
            AddFlag("targeting");
            StartCoroutine(StandAndShoot());
            clickHandler.clickAction = ClickHandler.ClickAction.target;
            clickHandler.clickContext = action;
        }
    }

    IEnumerator StandAndShoot()
    {
        // Makes character fully stand before shooting to prevent animation skipping

        ToggleCombat(true);
        ToggleCrouch(false);
        yield return new WaitForSeconds(0.01f);
    }

    public void SetTarget(Character selectedTarget=null, string action="")
    {
        // Sets the character's target and performs action on them
        // Called by ClickHandler

        if (selectedTarget)
            if (action == "attack")
            {
                if (equippedWeapon.stats.ammoCurrent > 0)
                {
                    targetCharacter = selectedTarget;
                    StartCoroutine(ShootWeapon());
                    RemoveFlag("targeting");
                }
                else
                {
                    Debug.Log("Out of Ammo! Reload weapon");
                }
            }
    }

    public void CancelTarget()
    {
        // Removes targeting flag and combat stance

        RemoveFlag("targeting");
        ToggleCombat(false);
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
