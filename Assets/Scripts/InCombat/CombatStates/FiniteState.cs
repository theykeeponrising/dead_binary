using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

abstract public class FiniteState<T>
{
    public string StateName;
    public StateMachine<T> Machine;
    public FiniteState(StateMachine<T> machine) { Machine = machine; }
    public T Owner;

    public virtual void Enter(T t)
    {
        Owner = t;
        StateName = GetType().Name.ToString();

        if (t.GetType() == typeof(InCombatPlayerAction)) { }
        {
            var v = t as InCombatPlayerAction;
            v.playerInput.Controls.InputPrimary.performed += _InputPrimary;
            v.playerInput.Controls.InputSecondary.performed += _InputSecndry;
            v.playerInput.Controls.ActionButton_1.performed += _InputAction1;
            v.playerInput.Controls.ActionButton_2.performed += _InputAction2;
            v.playerInput.Controls.ActionButton_3.performed += _InputAction3;
            v.playerInput.Controls.ActionButton_4.performed += _InputAction4;
            v.playerInput.Controls.ActionButton_5.performed += _InputAction5;
            v.playerInput.Controls.ActionButton_6.performed += _InputAction6;
            v.playerInput.Controls.ActionButton_7.performed += _InputAction7;
            v.playerInput.Controls.ActionButton_8.performed += _InputAction8;
            v.playerInput.Controls.ActionButton_9.performed += _InputAction9;
            v.playerInput.Controls.InputSpacebar.performed += _InputSpacebar;
            v.playerInput.Controls.InputTab.performed += _InputTab;
            v.playerInput.Controls.InputCancel.performed += _InputCancel;
        }
    }

    public virtual void Execute(T t) { }

    public virtual void Exit(T t)
    {
        if (t.GetType() == typeof(InCombatPlayerAction)) { }
        {
            var v = t as InCombatPlayerAction;
            v.playerInput.Controls.InputPrimary.performed -= _InputPrimary;
            v.playerInput.Controls.InputSecondary.performed -= _InputSecndry;
            v.playerInput.Controls.ActionButton_1.performed -= _InputAction1;
            v.playerInput.Controls.ActionButton_2.performed -= _InputAction2;
            v.playerInput.Controls.ActionButton_3.performed -= _InputAction3;
            v.playerInput.Controls.ActionButton_4.performed -= _InputAction4;
            v.playerInput.Controls.ActionButton_5.performed -= _InputAction5;
            v.playerInput.Controls.ActionButton_6.performed -= _InputAction6;
            v.playerInput.Controls.ActionButton_7.performed -= _InputAction7;
            v.playerInput.Controls.ActionButton_8.performed -= _InputAction8;
            v.playerInput.Controls.ActionButton_9.performed -= _InputAction9;
            v.playerInput.Controls.InputSpacebar.performed -= _InputSpacebar;
            v.playerInput.Controls.InputTab.performed -= _InputTab;
            v.playerInput.Controls.InputCancel.performed -= _InputCancel;
        }
    }

    public virtual void ChangeState(FiniteState<T> newState)
    {
        Machine.ChangeState(newState);
    }

    // Input Subscriptions
    private void _InputPrimary(InputAction.CallbackContext cxt)
    { InputPrimary(Owner); }
    private void _InputSecndry(InputAction.CallbackContext cxt)
    { InputSecndry(Owner); }
    private void _InputAction1(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 1); }
    private void _InputAction2(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 2); }
    private void _InputAction3(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 3); }
    private void _InputAction4(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 4); }
    private void _InputAction5(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 5); }
    private void _InputAction6(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 6); }
    private void _InputAction7(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 7); }
    private void _InputAction8(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 8); }
    private void _InputAction9(InputAction.CallbackContext cxt)
    { InputActionBtn(Owner, 9); }
    private void _InputSpacebar(InputAction.CallbackContext cxt)
    { InputSpacebar(Owner); }
    private void _InputTab(InputAction.CallbackContext cxt)
    {
        var shift = Keyboard.current.shiftKey.isPressed;
        InputTab(Owner, shift);
    }
    private void _InputCancel(InputAction.CallbackContext cxt)
    { InputCancel(Owner); }

    // Do On Input
    public virtual void InputPrimary(T t) 
    { Debug.Log("Primary has no function in this State. (" + StateName + ")"); }
    public virtual void InputSecndry(T t) 
    { Debug.Log("Secondary has no function in this State.(" + StateName + ")"); }
    public virtual void InputActionBtn(T t, int index)
    { Debug.Log(string.Format("Action {0} has no function in this State: {1}.", index, StateName)); }
    public virtual void InputSpacebar(T t)
    { Debug.Log("Spacebar has no function in this State. (" + StateName + ")"); }
    public virtual void InputTab(T t, bool shift)
    { Debug.Log("Tab has no function in this State. (" + StateName + ")"); }
    public virtual void InputCancel(T t)
    { StateHandler.Instance.GetStateObject(StateHandler.State.CombatState).ChangeState(StateHandler.State.StatusMenuState); }

    // Helper Functions

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement(InCombatPlayerAction t)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(t));
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        int UILayer = LayerMask.NameToLayer("UI");
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults(InCombatPlayerAction t)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = t.playerInput.Controls.InputPosition.ReadValue<Vector2>();
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public void ButtonPress(int index)
    {
        // Button press effect
        List<ActionButton> actionPanel = UIManager.Instance.actionPanel.GetButtons();
        List<ActionButton> inventoryPanel = UIManager.Instance.inventoryPanel.GetButtons();
        List<ActionButton> buttons = actionPanel.Concat(inventoryPanel).ToList();

        buttons[index-1].ButtonTrigger();
    }
}