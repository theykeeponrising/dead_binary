using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//Class if focused on implementing the characters actions
//I.e. the logic for moving, shooting, jumping, etc.
//May be worth splitting up further
public class CharacterActor
{
    [Header("-Character Attributes")]
    //[HideInInspector]
    GameObject selectionCircle;

    [Header("--Pathfinding")]
    [HideInInspector] public List<Tile> movePath;
    Tile moveTargetImmediate;
    Tile moveTargetDestination;
    [HideInInspector] public bool isAtDestination => IsAtDestination();

    private bool IsAtDestination()
    {
        bool b = moveTargetDestination == null ? true : false;
        return b;
    }
    // Transform lookTarget; -- NOT IMPLEMENTED
    [HideInInspector] public Unit targetCharacter;
    [HideInInspector] public InCombatPlayerAction playerAction;
    
    Unit unit;

    public Actions.Action currentAction;
    public List<Unit> potentialTargets;

    public CharacterActor(Unit unit)
    {
        selectionCircle = unit.transform.Find("SelectionCircle").gameObject;
        this.unit = unit;
        potentialTargets = null;
    }

    public void Update()
    {
        Movement();
    }

    public bool IsMoving()
    {
        return (moveTargetDestination != null);
    }

    public void SetPlayerAction(InCombatPlayerAction playerAction)
    {
        this.playerAction = playerAction;
    }

    public InCombatPlayerAction GetPlayerAction()
    {
        return playerAction;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlights unit on mouse over
        if (playerAction.selectedCharacter != unit)
        {
            selectionCircle.SetActive(true);
            selectionCircle.GetComponent<Renderer>().material.color = new Color(0, 255, 0, 0.10f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave
        if (playerAction.selectedCharacter != unit)
        {
            selectionCircle.SetActive(false);
            selectionCircle.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public Vector3 GetCharacterChestPosition()
    {
        return unit.GetAnimator().GetCharacterChestPosition();
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

    public void ProcessAction(Actions.Action actionToPerform, Tile contextTile=null, List<Tile> contextPath=null, Unit contextCharacter=null)
    {
        // Determine if action can be performed, and perform action if so

        // Check if action is in allowed list of actions for character
        if (!unit.GetAvailableActions().Contains(actionToPerform.tag))
        {
            Debug.Log(string.Format("{0} does not contain action {1}", unit.attributes.name, actionToPerform.name));
            return;
        }    

        int actionCost = actionToPerform.cost;
        if (actionCost > unit.stats.actionPointsCurrent)
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
                    ShootAction(contextCharacter);
                    break;
                case "reload":
                    ReloadAction();
                    break;
                case "swap":
                    unit.StartCoroutine(EquipWeapon(unit.inventory.CycleWeapon()));
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
            float distance = Vector3.Distance(unit.transform.position, moveTargetPoint);
            unit.velocityZ = distance / 2;

            // Slow down movement speed if character is vaulting
            float distanceDelta = (unit.GetAnimator().GetFlag("vaulting")) ? 0.01f : 0.03f;

            // If the final move target is also the most immediate one, slow down move speed as we approach
            if (moveTargetDestination == moveTargetImmediate)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, moveTargetPoint, distanceDelta);
                relativePos = moveTargetPoint - unit.transform.position;
            }
            else
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, moveTargetImmediate.transform.position, distanceDelta);
                relativePos = moveTargetImmediate.transform.position - unit.transform.position;
            }

            // Gradually rotate character to face towards move target
            if (relativePos != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(relativePos);
                toRotation.x = unit.transform.rotation.x;
                toRotation.z = unit.transform.rotation.z;
                unit.transform.rotation = Quaternion.Lerp(unit.transform.rotation, toRotation, 10 * Time.deltaTime);
            }
        }

        // Gradually rotate character to expected look direction while behind cover
        else if (unit.currentCover && !unit.GetAnimator().AnimationPause())
        {
            // Get which the direction the cover is relative to the tile
            Vector3 lookDirection = (unit.currentCover.transform.position - unit.currentTile.transform.position);

            // Add the direction to the tile world space position to get a world space point to look at
            lookDirection = lookDirection + unit.currentTile.transform.position;

            // Remove vertical position for a flat lookat point
            lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);

            // Character look at position
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, Quaternion.LookRotation(lookDirection - unit.transform.position), 3 * Time.deltaTime);

            // Crouch down if its a half-sized cover
            if (unit.currentCover.coverSize == CoverObject.CoverSize.half && !unit.GetAnimator().GetFlag("crouching"))
                ToggleCrouch(true, false);
        }
    }

    public void MoveAction(Tile newTile, List<Tile> previewPath)
    {
        // Sets the target destination tile
        // Once a path is found, begin movement routine

        if (!unit.GetAvailableActions().Contains(Actions.ActionsList.MOVE))
            return;

        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);

        if (!unit.GetAnimator().GetFlag("moving"))
        {
            if (CheckTileMove(newTile))
            {
                // If tile is occupied, we can't move there
                if (newTile.occupant)
                    movePath = null;
                else movePath = unit.currentTile.FindCost(newTile, unit.stats.movement);
                
                if (movePath.Count > 0)
                {
                    unit.StartCoroutine(MoveToPath());
                    unit.stats.actionPointsCurrent -= currentAction.cost;
                }
                
            }
        }
    }

    IEnumerator MoveToPath()
    {
        //stateMachine.ChangeState(new SelectedStates.Moving(stateMachine));

        // Movement routine
        // Sets "moving" flag before and removes after
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.MOVE, true);

        // Stand up if crouched
        if (unit.GetAnimator().GetFlag("crouching"))
            ToggleCrouch(false, true);

        moveTargetDestination = movePath[movePath.Count - 1];
        unit.currentTile.ChangeTileOccupant();
        unit.currentCover = null;

        // Move to each tile in the provided path
        foreach (Tile path in movePath)
        {
            if (moveTargetImmediate)
                moveTargetImmediate.ChangeTileOccupant();
            moveTargetImmediate = path;

            // Wait until immediate tile is reached before moving to the next one
            while (unit.currentTile != path)
            {
                CheckForObstacle();
                unit.currentTile = unit.grid.GetTile(unit.transform.position);
                yield return new WaitForSeconds(0.01f);
            }
            path.ChangeTileOccupant(unit);
            unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.VAULT, false);
        }

        // Wait until character comes to a stop before completing movement action
        if (moveTargetImmediate == moveTargetDestination)
            while (Vector3.Distance(unit.transform.position, moveTargetImmediate.standPoint) > 0)
                yield return new WaitForSeconds(0.001f);
        else
            while (Vector3.Distance(unit.transform.position, moveTargetImmediate.transform.position) > 0)
                yield return new WaitForSeconds(0.001f);
        unit.transform.position = new Vector3(unit.currentTile.standPoint.x, 0f, unit.currentTile.standPoint.z);

        // End movement animation
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.MOVE, false);

        // Register cover object
        if (unit.currentTile.cover)
        {
            unit.currentCover = unit.currentTile.cover;
            //Crouch, should maybe set a bool outside of the animator
            if (unit.currentTile.cover.coverSize == CoverObject.CoverSize.half && !unit.GetAnimator().GetFlag("crouching"))
                // unit.GetAnimator().AnimationTransition(CharacterAnimator.AnimationEventContext.CROUCH_DOWN);
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

        movePath = unit.currentTile.FindCost(newTile);
        if (movePath.Count == 0 || !newTile.isTileTraversable())
        {
            Debug.Log("No move path."); // Replace this with UI eventually
            return false;
        }
        if (movePath.Count > unit.stats.movement)
        {
            Debug.Log(string.Format("Destination Too Far! \nDistance: {0}, Max Moves: {1}", movePath.Count, unit.stats.movement)); // This will eventually be shown visually instead of told
            return false;
        }
        return true;
    }

    bool CheckForObstacle()
    {
        // Checks a short distance in front of character for objects in the "VaultOver" layer
        if (unit.GetAnimator().GetFlag("vaulting"))
            return false;

        Vector3 direction = (moveTargetImmediate.transform.position - unit.transform.position);
        RaycastHit hit;
        Ray ray = new Ray(unit.transform.position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));
        float distance = 0.5f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out hit, direction.magnitude * distance, layerMask))
        {
            if (hit.collider.GetComponent<CoverObject>().canVaultOver)
            {
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.VAULT, true);
                //animator.Play("Default", unit.inventory.equippedWeapon.weaponLayer);
                return true;
            }
        }
        return false;
    }

    IEnumerator EquipWeapon(Weapon weapon)
    {
        // Character equip or swap weapons
        // Previous weapon is stowed in extra slot

        // If character has a weapon equipped currently, stow it
        if (unit.inventory.equippedWeapon && unit.inventory.equippedWeapon != AssetManager.Instance.weapon.noWeapon)
        {
            // If crouching, do not play stow animation
            // This is until we can get a proper crouch-stow animation
            if (!unit.GetAnimator().GetFlag("crouching"))
            {
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.STOW, true);
                while (unit.GetAnimator().GetFlag("stowing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.STOW, false);
            }
        }

        // Equipping the new weapon
        if (weapon)
        {
            unit.inventory.equippedWeapon = weapon;

            // Enable weapon object, set position and animation layer
            unit.inventory.equippedWeapon.gameObject.SetActive(true);
            unit.inventory.equippedWeapon.DefaultPosition(unit);
            
            unit.GetAnimator().SetLayerWeight(unit.inventory.equippedWeapon.weaponLayer, 1);
            unit.GetAnimator().SetAnimationSpeed(unit.inventory.equippedWeapon.attributes.animSpeed);

            // If crouching, do not play draw animation
            // This is until we can get a proper crouch-draw animation
            if (!unit.GetAnimator().GetFlag("crouching"))
            {
                unit.inventory.equippedWeapon.PlaySound(Weapon.WeaponSound.SWAP, unit);
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DRAW, true);

                while (unit.GetAnimator().GetFlag("drawing"))
                    yield return new WaitForSeconds(0.01f);
            }
            else
            {
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DRAW, false);
            }
        }
    }

    public void ReloadAction()
    {
        // Reload action handler

        if (unit.inventory.equippedWeapon.stats.ammoCurrent >= unit.inventory.equippedWeapon.stats.ammoMax)
        {
            Debug.Log("Ammo is max already!"); // TO DO - Show this in UI
            return;
        }
        unit.stats.actionPointsCurrent -= currentAction.cost;
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.RELOAD, true);
        unit.inventory.equippedWeapon.Reload();
    }

    public Vector3 GetTargetPosition()
    {
        // Gets target's position relative to the tip of the gun

        Vector3 targetDirection = targetCharacter.GetAnimator().GetCharacterChestPosition() - unit.inventory.equippedWeapon.transform.position;
        Vector3 aimDirection = unit.inventory.equippedWeapon.transform.forward;
        float blendOut = 0.0f;
        float angleLimit = 90f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50f;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return unit.inventory.equippedWeapon.transform.position + direction;
    }

    IEnumerator ShootWeapon(int distanceToTarget, Unit shootTarget=null)
    {
        // Sets the characters aiming and physics flags
        // Performs the shoot animation and inflicts damage on the target
        // When done, returns flags and physics to default

        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, true);

        // animator.SetBool("aiming", true);
        // animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        yield return new WaitForSeconds(0.5f);
        unit.GetAnimator().AddFlag("aiming");
        yield return new WaitForSeconds(0.25f);

        // Inflict damage on target character
        if (shootTarget)
            shootTarget.GetActor().TakeDamage(unit, unit.inventory.equippedWeapon.stats.damage, distanceToTarget);

        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.SHOOT, true);
        unit.inventory.equippedWeapon.stats.ammoCurrent -= 1;

        // Wait until shoot state completes
        while (playerAction.stateMachine.GetCurrentState().GetType() == typeof(SelectedStates.ShootTarget)) yield return new WaitForSeconds(0.01f);

        // If target is dodging, remove flag        
        if (shootTarget && shootTarget.GetAnimator().GetFlag("dodging")) shootTarget.GetAnimator().RemoveFlag("dodging");

        // Shooting animation completed (should maybe just implement this with a callback function, honestly)
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.SHOOT, false);
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, false);
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.IDLE, true);
    }

    void ToggleCrouch(bool crouching=false, bool instant=false)
    {
        // Placeholder function
        // Test crouch animation when button pressed
        // Just call char animation for now. Left in character.cs in case of more crouching logic
        unit.GetAnimator().ToggleCrouch(crouching, instant);
    }

    bool RollForHit(Unit attacker, int distanceToTarget)
    {
        // Dodge change for character vs. attacker's aim

        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float hitChance = unit.CalculateHitChance(attacker, unit);
        float baseChance = hitChance * 100.0f;

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distanceToTarget, baseChance, randomChance));

        // Return true/false if hit connected
        return (baseChance  >= randomChance);
    }

    void TakeDamage(Unit attacker, int damage, int distanceToTarget)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        // If attacked missed, do not take damage
        if (!RollForHit(attacker, distanceToTarget))
        {
            Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, unit.attributes.name));
            unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.DODGE, true);
            return;
        }

        // Inflict damage on character
        Debug.Log(string.Format("{0} has attacked {1} for {2} damage!", attacker.attributes.name, unit.attributes.name, damage)); // This will eventually be shown visually instead of told

        Vector3 direction =  (unit.transform.position - attacker.transform.position);
        unit.stats.healthCurrent -= damage;

        // Character death
        if (unit.stats.healthCurrent <= 0)
        {
            unit.StartCoroutine(Death(attacker, direction));
            Debug.DrawRay(unit.transform.position, direction, Color.red, 20, true); // For debug purposes
        }
    }

    IEnumerator Death(Unit attacker, Vector3 attackDirection, float impactForce = 2f)
    {
        // Disables animator, turns on ragdoll effect, and applies a small force to push the character over

        // Wait for attacker animation to complete
        while (attacker.GetAnimator().AnimatorIsPlaying())
            yield return new WaitForSeconds(0.01f);

        // Disable top collider
        unit.GetComponent<CapsuleCollider>().enabled = false;

        // Disable animator and top rigidbody
        unit.GetAnimator().OnDeath(attackDirection * impactForce, ForceMode.Impulse);

        unit.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // TO DO -- Layer specifically for dead characters??

        // Remove player selection
        if (playerAction.selectedCharacter == unit)
            playerAction.selectedCharacter = null;
        SelectUnit(false);

        // Disable any character lights
        foreach (Light light in unit.GetComponentsInChildren<Light>())
            light.enabled = false;

        // Remove character as a obstacle on the map
        unit.currentTile.occupant = null;
        unit.enabled = false;

        if (unit.inventory.equippedWeapon)
            unit.inventory.equippedWeapon.DropGun();
    }

    public void ShootAction(Unit selectedTarget=null)
    {
        // Sets the character's target and performs action on them
        // Called by InCombatPlayerAction

        if (selectedTarget)
        {
            int minWeaponRange = unit.inventory.equippedWeapon.GetMinimumRange();
            int distanceToTarget = unit.currentTile.FindCost(selectedTarget.currentTile, 15).Count;

            //Check if target within weapon range
            if (distanceToTarget >= minWeaponRange)
                {
                if (unit.inventory.equippedWeapon.stats.ammoCurrent > 0)
                {
                    targetCharacter = selectedTarget;
                    unit.stats.actionPointsCurrent -= currentAction.cost;
                    unit.StartCoroutine(ShootWeapon(distanceToTarget, targetCharacter));
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

        unit.GetComponentInChildren<CharacterCamera>().enabled = true;
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, true);
        ToggleCrouch(false);
    }


    public void ClearTarget()
    {
        // Removes targeting flag and combat stance

        unit.GetComponentInChildren<CharacterCamera>().enabled = false;
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, false);
        targetCharacter = null;
    }
}
