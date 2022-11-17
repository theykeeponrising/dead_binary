public class UnitActionInventory : UnitStateAction
{
    public override void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        setState.ChangeState(new StateChooseItem(setState.Machine));
    }
}
