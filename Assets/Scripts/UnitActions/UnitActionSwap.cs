using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionSwap : UnitAction
{

    public override void UseAction()
    {
        // Kicks off performance with the swap animation

        if (unit.GetActor().IsActing())
            return;

        StartPerformance("Stow");
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

        if (unit.GetEquippedWeapon() && unit.GetEquippedWeapon() != AssetManager.Instance.weapon.noWeapon)
        {
            unit.GetEquippedWeapon().gameObject.SetActive(false);
            unit.GetAnimator().SetLayerWeight(unit.GetEquippedWeapon().weaponLayer, 0);
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
            unit.inventory.equippedWeapon.gameObject.SetActive(true);
            unit.inventory.equippedWeapon.DefaultPosition(unit);

            // Set animator values, play animation
            unit.GetAnimator().Play("Draw");
            unit.GetAnimator().SetLayerWeight(unit.inventory.equippedWeapon.weaponLayer, 1);
            unit.GetAnimator().SetAnimationSpeed(unit.inventory.equippedWeapon.attributes.animSpeed);
            unit.GetEquippedWeapon().PlaySound(Weapon.WeaponSound.SWAP, unit);

            // Update info panel to the new weapon values
            InfoPanelScript infoPanel = UIManager.GetInfoPanel();
            if (infoPanel.gameObject.activeSelf)
            {
                infoPanel.UpdateHit(unit.GetCurrentHitChance());
                infoPanel.UpdateDamage(-unit.inventory.equippedWeapon.GetDamage());
            }
        }
    }
}
