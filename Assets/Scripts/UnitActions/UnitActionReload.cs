using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionReload : UnitAction
{
    Weapon weapon;

    public override void UseAction()
    {
        // Kicks off unit and weapon's reload methods
        // Sets action to "performing" state

        unit.SpendActionPoints(actionCost);
        weapon = unit.GetEquippedWeapon();
        weapon.ReloadEffect();
        StartPerformance("Reload");
    }

    public override void CheckAction()
    {
        // Waits until reload animation completes
        // Then changes weapons ammo and ends action performance

        while (unit.GetAnimator().AnimatorIsPlaying("Reload"))
            return;

        weapon.Reload();
        EndPerformance();
    }
}
