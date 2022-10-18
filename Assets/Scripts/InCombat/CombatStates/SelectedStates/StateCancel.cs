using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCancel : FiniteState<InCombatPlayerAction>
{
    public StateCancel(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

    public InfoPanelScript infoPanel = UIManager.GetInfoPanel();
    public InventoryPanelScript inventoryPanel = UIManager.GetInventoryPanel();

    public override void InputSecndry(InCombatPlayerAction t)
    {
        // Right mouse button will exit targeting

        t.selectedCharacter.GetActor().ClearTarget();
        ChangeState(new StateIdle(Machine));
    }

    public override void InputCancel(InCombatPlayerAction t)
    {
        t.selectedCharacter.GetActor().ClearTarget();
        ChangeState(new StateIdle(Machine));
    }
}