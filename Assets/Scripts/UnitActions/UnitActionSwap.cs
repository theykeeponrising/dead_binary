using System.Collections.Generic;
using UnityEngine;

public class UnitActionSwap : UnitAction
{
    new StateTarget CurrentState;

    public override void UseAction()
    {
        // Kicks off performance with the swap animation

        if (Unit.IsActing())
            return;

        StartPerformance("Stow");
    }

    public override void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        // Updates the shoot action while unit is aiming

        CurrentState = (StateTarget)setState;

        UseAction();
    }

    public override void CheckAction()
    {
        // Waits until swap animation completes
        // Then cycles weapons and begins draw animation
        // Waits until draw is complete to end performance

        while (Unit.IsPlayingAnimation("Stow"))
            return;

        if (ActionStage(0))
        {
            StowWeapon();
            DrawWeapon();
            NextStage();
        }

        while (Unit.IsPlayingAnimation("Draw"))
            return;

        EndPerformance();
    }

    private void StowWeapon()
    {
        // Stows currently equipped weapon by disabling object and animation layer

        if (Unit.EquippedWeapon && Unit.EquippedWeapon != AssetManager.Instance.weapon.noWeapon)
        {
            Unit.EquippedWeapon.gameObject.SetActive(false);
            Unit.SetAnimationLayerWeight(Unit.EquippedWeapon.GetAnimationLayer(), 0);
        }
    }

    private void DrawWeapon()
    {
        // Cylce to next weapon
        Weapon weapon = Unit.CycleWeapon();

        if (weapon)
        {
            Unit.EquippedWeapon = weapon;

            // Enable weapon object, set position and animation layer

            SwapWeaponAction();
            UnitTargetAction shootAction = null;

            if (CurrentState != null)
            {
                shootAction = (UnitTargetAction)Unit.FindActionOfType(Unit.EquippedWeapon.WeaponAction.GetType());
                CurrentState.SetStoredAction(shootAction, weapon);
            }

            // Set animator values, play animation
            Unit.PlayAnimation("Draw");
            Unit.PlaySound(AnimationType.SWAP);

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
                infoPanel.UpdateHit(Unit.GetHitChance());
                infoPanel.UpdateDamage(-Unit.EquippedWeapon.GetDamage());
            }
        }
    }

    public void SwapWeaponAction()
    {
        // Finds the previous weapon's shoot action
        // Replaces it with the new weapon's shoot action

        List<UnitAction> unitActions = Unit.GetUnitActions();
        foreach (UnitAction foundAction in unitActions)
        {
            if (foundAction.GetType().IsSubclassOf(typeof(UnitTargetAction)))
            {
                int index = unitActions.IndexOf(foundAction);
                Transform _unitActionsContainer = Unit.GetUnitActionsContainer();
                UnitAction newAction = Instantiate(Unit.EquippedWeapon.WeaponAction, _unitActionsContainer);
                unitActions.Insert(index, newAction);
                unitActions.Remove(foundAction);
                Destroy(foundAction.gameObject);
                return;
            }
        }
    }
}
