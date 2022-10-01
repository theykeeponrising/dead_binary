using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChooseItem : StateCancel
{
    public StateChooseItem(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.GetComponent<InventoryPanelScript>().BindButtons();
    }
    public override void Exit(InCombatPlayerAction t)
    {
        base.Exit(t);
        inventoryPanel.gameObject.SetActive(false);
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        Unit c = t.selectedCharacter;

        if (!IsPointerOverUIElement(t))
            t.SelectUnit();

        if (t.selectedCharacter == null) ChangeState(new StateNoSelection(Machine));
        if (t.selectedCharacter != c) ChangeState(new StateIdle(Machine));
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        ActionPanelScript actionPanel = UIManager.GetActionPanel();
        int offset = actionPanel.GetButtons().Count;
        int fixed_index = index - 1 - offset;
        UnitAction action = t.GetBindings(index);
        List<Item> items = t.selectedCharacter.GetItems();

        // If the Choose Item button was selected again, close the panel and return to Idle
        if (action.GetType() == typeof(UnitActionInventory))
        {
            ButtonPress(index);
            InputCancel(t);
            return;
        }

        // Valid item was selected, check requirements and then proceed to use
        else if (action.GetType().IsSubclassOf(typeof(UnitActionItem)) && action.CheckRequirements())
        {
            ButtonPress(index);
            action.UseAction(this, items[fixed_index]);
            return;
        }

        // If a normal action button was selected, perform that action
        else if (index <= offset)
        {
            new StateIdle(Machine).InputActionBtn(t, index);
            return;
        }

        else
            Debug.Log("Invalid item selection.");
    }
}
