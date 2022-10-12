using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionSwap : UnitAction
{
    new StateTarget currentState;

    public override void UseAction()
    {
        // Kicks off performance with the swap animation
        if (unit.GetActor().IsActing())
            return;

        StartPerformance("Stow");
    }


    public override void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        // Updates the shoot action while unit is aiming
        currentState = (StateTarget)setState;

        UseAction();
    }

    public override void CheckAction()
    {
        // Waits until swap animation completes
        // Then cycles weapons and begins draw animation
        // Waits until draw is complete to end performance

        while (unit.GetAnimator().AnimatorIsPlaying("Stow"))
            return;

        if (ActionStage(0))
        {
            StowWeapon();
            DrawWeapon();
            NextStage();
        }

        while (unit.GetAnimator().AnimatorIsPlaying("Draw"))
            return;

        EndPerformance();
    }

    void StowWeapon()
    {
        // Stows currently equipped weapon by disabling object and animation layer

        if (unit.EquippedWeapon && unit.EquippedWeapon != AssetManager.Instance.weapon.noWeapon)
        {
            unit.EquippedWeapon.gameObject.SetActive(false);
            unit.GetAnimator().SetLayerWeight(unit.EquippedWeapon.GetAnimationLayer(), 0);
        }
    }

    void DrawWeapon()
    {
        // Cylce to next weapon
        Weapon weapon = unit.inventory.CycleWeapon();

        if (weapon)
        {
            unit.inventory.equippedWeapon = weapon;

            // Enable weapon object, set position and animation layer
            unit.EquippedWeapon.gameObject.SetActive(true);
            unit.EquippedWeapon.DefaultPosition(unit);
            SwapWeaponAction();
            UnitTargetAction shootAction = null;

            if (currentState != null)
            {
                shootAction = (UnitTargetAction)unit.GetActor().FindActionOfType(unit.EquippedWeapon.WeaponAction.GetType());
                currentState.SetStoredAction(shootAction);
            }

            // Set animator values, play animation
            unit.GetAnimator().Play("Draw");
            unit.GetAnimator().SetLayerWeight(unit.EquippedWeapon.GetAnimationLayer(), 1);
            unit.GetAnimator().SetAnimationSpeed(unit.EquippedWeapon.attributes.animationSpeed);
            unit.GetSFX().Event_PlaySound(AnimationType.SWAP);

            ActionPanelScript actionPanel = UIManager.GetActionPanel();
            if (actionPanel.gameObject.activeSelf)
            {
                actionPanel.DestroyButtons();
                actionPanel.BindButtons();
            }

            // Update info panel to the new weapon values
            InfoPanelScript infoPanel = UIManager.GetInfoPanel();
            if (infoPanel.gameObject.activeSelf)
            {
                if (shootAction) infoPanel.UpdateAction(shootAction);
                infoPanel.UpdateHit(unit.GetCurrentHitChance());
                infoPanel.UpdateDamage(-unit.EquippedWeapon.GetDamage());
            }
        }
    }

    public void SwapWeaponAction()
    {
        // Finds the previous weapon's shoot action
        // Replaces it with the new weapon's shoot action

        List<UnitAction> unitActions = unit.GetUnitActions();
        foreach (UnitAction foundAction in unitActions)
        {
            if (foundAction.GetType().IsSubclassOf(typeof(UnitTargetAction)))
            {
                int index = unitActions.IndexOf(foundAction);
                Transform _unitActionsContainer = unit.GetUnitActionsContainer();
                UnitAction newAction = Instantiate(unit.EquippedWeapon.WeaponAction, _unitActionsContainer);
                unitActions.Insert(index, newAction);
                unitActions.Remove(foundAction);
                Destroy(foundAction.gameObject);
                return;
            }
        }
    }
}
