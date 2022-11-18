public class UnitActionReload : UnitAction
{
    private Weapon _weapon;

    public override void UseAction()
    {
        // Kicks off unit and weapon's reload methods
        // Sets action to "performing" state

        if (Unit.IsActing())
            return;

        Unit.SpendActionPoints(ActionCost);
        _weapon = Unit.EquippedWeapon;
        _weapon.PlaySound(WeaponSound.RELOAD);
        StartPerformance("Reload");
    }

    public override void CheckAction()
    {
        // Waits until reload animation completes
        // Then changes weapons ammo and ends action performance

        while (Unit.IsPlayingAnimation("Reload"))
            return;

        _weapon.Reload();
        EndPerformance();
    }
}
