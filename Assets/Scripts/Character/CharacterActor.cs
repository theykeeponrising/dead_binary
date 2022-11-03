using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Class if focused on implementing the characters actions
//I.e. the logic for moving, shooting, jumping, etc.
//May be worth splitting up further
public class CharacterActor
{
    InfoPanelScript infoPanel;
    SelectionCircle selectionCircle;

    public struct MoveData
    {
        public List<Tile> path;
        public Tile immediate;
        public Tile destination;
    }
    public MoveData moveData = new MoveData();

    [HideInInspector] public Unit targetCharacter;
    [HideInInspector] public InCombatPlayerAction playerAction;
    
    Unit unit;

    public UnitAction currentAction;
    public List<Unit> potentialTargets;

    public CharacterActor(Unit unit)
    {
        infoPanel = UIManager.GetInfoPanel();
        this.unit = unit;
        selectionCircle = unit.GetComponentInChildren<SelectionCircle>();
        potentialTargets = null;
    }

    public void Update()
    {
        Movement();
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
        SelectUnit(SelectionType.HIGHLIGHT);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave
        SelectUnit(SelectionType.CLEAR);
    }

    public Vector3 GetCharacterChestPosition()
    {
        return unit.GetAnimator().GetCharacterChestPosition();
    }

    public void SelectUnit(SelectionType selectionType = SelectionType.CLEAR)
    {
        // Changes selection circle based on selection type
        selectionCircle.ChangeSelection(selectionType);
    }

    public void IsTargetUX(bool isTarget, bool isPotentialTarget)
    {
        if(isPotentialTarget)
        {
            if (isTarget)
                SelectUnit(SelectionType.TARGET_MAIN);
            else
                SelectUnit(SelectionType.TARGET_POTENTIAL);
        }
        else
        {
            SelectUnit(SelectionType.CLEAR);
        }
    }

    public UnitAction FindActionOfType(System.Type actionType, bool printDebug = false)
    {
        // Finds an action by type from the unit's current action list

        foreach (UnitAction unitAction in unit.GetUnitActions())
        {
            if (printDebug) Debug.Log(unitAction.GetType());
            if (unitAction.GetType() == actionType)
                return unitAction;
        }
        return null;
    }

    public bool IsActing()
    {
        // Checks all unit actions to see if any are currently performing
        // Returns True/False if any action is currently performing

        foreach (UnitAction action in unit.GetUnitActions())
            if (action.Performing())
                return true;

        foreach (Item item in unit.GetItems())
            if (item.itemAction.Performing())
                return true;

        return false;
    }

    public void SetWaiting(bool isWaiting)
    {
        // Toggles the unit's "waiting" state

        UnitAction waitAction = FindActionOfType(typeof(UnitActionWait));
        waitAction?.SetPerformed(performed: isWaiting);
        unit.healthbar.WaitingIndicator(showSprites: isWaiting);
    }

    void Movement()
    {
        // Function that actually moves target towards destination
        // If there is no move target, then this action is skipped
        
        // If we have a move target, begin moving
        if (moveData.immediate)
        {
            Vector3 relativePos;
            Vector3 moveTargetPoint = moveData.destination.StandPoint;
            float distance = Vector3.Distance(unit.transform.position, moveTargetPoint);
            unit.velocityZ = distance / 2;

            // Slow down movement speed if character is vaulting
            float distanceDelta = (unit.GetFlag(FlagType.VAULT)) ? 0.0125f : 0.03f;

            // If the final move target is also the most immediate one, slow down move speed as we approach
            if (moveData.destination == moveData.immediate)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, moveTargetPoint, distanceDelta);
                relativePos = moveTargetPoint - unit.transform.position;
            }
            else
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, moveData.immediate.transform.position, distanceDelta);
                relativePos = moveData.immediate.transform.position - unit.transform.position;
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
        else if (unit.currentCover && !unit.GetActor().IsActing())
        {
            // Get which the direction the cover is relative to the tile
            Vector3 lookDirection = (unit.currentCover.transform.position - unit.currentTile.transform.position);

            // Add the direction to the tile world space position to get a world space point to look at
            lookDirection = lookDirection + unit.currentTile.transform.position;

            // Remove vertical position for a flat lookat point
            lookDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);

            // Character look at position
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, Quaternion.LookRotation(lookDirection - unit.transform.position), 3 * Time.deltaTime);
        }
    }

    public bool CheckForObstacle()
    {
        // Checks a short distance in front of character for objects in the "VaultOver" layer
        if (unit.GetFlag(FlagType.VAULT))
            return false;

        Vector3 direction = (moveData.immediate.transform.position - unit.transform.position);
        RaycastHit hit;
        Ray ray = new Ray(unit.transform.position, direction);
        int layerMask = (1 << LayerMask.NameToLayer("CoverObject"));
        float distance = 0.5f;

        // If vaultable object detected, play vaulting animation
        if (Physics.Raycast(ray, out hit, direction.magnitude * distance, layerMask))
        {
            if (hit.collider.GetComponentInParent<CoverObject>().IsVaultable)
            {
                unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.VAULT, true);
                return true;
            }
        }
        return false;
    }

    public Vector3 GetTargetPosition(bool snap=false)
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
        if (snap) direction = targetDirection;
        return unit.inventory.equippedWeapon.transform.position + direction;
    }

    void ToggleCrouch(bool instant=false)
    {
        // Activates crouch trigger, which will toggle crouching state

        unit.GetAnimator().ToggleCrouch(instant);
    }

    bool IsCrouching()
    {
        // Returns true if any crouch animation is playing

        return unit.GetAnimator().IsCrouching();
    }

    void CoverCrouch()
    {
        // Makes character crouch if they should be crouching behind cover

        unit.GetAnimator().CoverCrouch();
    }

    public void GetTarget(bool useCharacterCamera = false)
    {
        // Character it put into "targeting" mode
        // Target selected with left-click will have action done to it (such as attack action)

        unit.GetComponentInChildren<CharacterCamera>().enabled = useCharacterCamera;
        unit.GetComponent<Collider>().enabled = !useCharacterCamera;
        unit.GetAnimator().ProcessAnimationEvent(CharacterAnimator.AnimationEventContext.AIMING, true);
        if (IsCrouching()) ToggleCrouch();

        infoPanel.gameObject.SetActive(true);
        UpdateHitStats();
    }

    public void UpdateHitStats()
    {
        infoPanel.UpdateHit(unit.GetCurrentHitChance());
        infoPanel.UpdateDamage(-unit.EquippedWeapon.GetDamage());
    }

    public void ClearTarget()
    {
        // Cleans up targeting-related objects

        unit.GetComponentInChildren<CharacterCamera>().enabled = false;
        unit.GetComponent<Collider>().enabled = true;
        infoPanel.gameObject.SetActive(false);

        unit.GetAnimator().SetBool("aiming", false);
        unit.GetAnimator().SetUpdateMode();
        unit.RemoveFlag(FlagType.AIM);
        targetCharacter = null;

        CoverCrouch();
    }

    public void ItemAction(Item item, Unit target)
    {
        if (item.immuneUnitTypes.Contains(target.attributes.unitType))
        {
            Debug.Log("Units of this type are immune to the effect of this item.");
            return;
        }

        item.UseItem(unit, target);
        unit.SpendActionPoints(item.itemAction.actionCost);
        unit.transform.LookAt(target.transform);
    }

    public void ItemAction(Item item, Tile targetTile)
    {
        unit.SpendActionPoints(item.itemAction.actionCost);
        item.UseItem(unit, targetTile.transform.position);
        unit.transform.LookAt(targetTile.transform);
    }
}
