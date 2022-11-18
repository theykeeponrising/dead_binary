public class UnitActionWait : UnitAction
{
    public override void UseAction()
    {
        // Flags the unit as "waiting"
        // Displays waiting icon above character and flags them as ending turn

        if (Unit.IsActing())
            return;

        SetPerformed(true);
        Unit.PlayerAction.SelectAction();
        Unit.Healthbar.ShowWaitIndicator(true);
        Unit.PlayerAction.CheckTurnEnd();
    }
}
