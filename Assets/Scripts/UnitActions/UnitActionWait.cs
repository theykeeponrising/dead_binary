public class UnitActionWait : UnitAction
{
    public override void UseAction()
    {
        // Flags the unit as "waiting"
        // Displays waiting icon above character and flags them as ending turn

        if (unit.IsActing())
            return;

        SetPerformed(true);
        unit.PlayerAction.SelectAction();
        unit.Healthbar.ShowWaitIndicator(true);
        unit.PlayerAction.CheckTurnEnd();
    }
}
