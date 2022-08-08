using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    { InputAction1(Owner); }
    private void _InputAction2(InputAction.CallbackContext cxt)
    { InputAction2(Owner); }
    private void _InputAction3(InputAction.CallbackContext cxt)
    { InputAction3(Owner); }
    private void _InputAction4(InputAction.CallbackContext cxt)
    { InputAction4(Owner); }
    private void _InputAction5(InputAction.CallbackContext cxt)
    { InputAction5(Owner); }
    private void _InputAction6(InputAction.CallbackContext cxt)
    { InputAction6(Owner); }
    private void _InputAction7(InputAction.CallbackContext cxt)
    { InputAction7(Owner); }
    private void _InputAction8(InputAction.CallbackContext cxt)
    { InputAction8(Owner); }
    private void _InputAction9(InputAction.CallbackContext cxt)
    { InputAction9(Owner); }

    // Do On Input
    public virtual void InputPrimary(T t) 
    { Debug.Log("Primary has no function in this State. (" + StateName + ")"); }
    public virtual void InputSecndry(T t) 
    { Debug.Log("Secondary has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction1(T t)
    { Debug.Log("Action (1) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction2(T t)
    { Debug.Log("Action (2) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction3(T t)
    { Debug.Log("Action (3) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction4(T t)
    { Debug.Log("Action (4) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction5(T t)
    { Debug.Log("Action (5) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction6(T t)
    { Debug.Log("Action (6) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction7(T t)
    { Debug.Log("Action (7) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction8(T t)
    { Debug.Log("Action (8) has no function in this State.(" + StateName + ")"); }
    public virtual void InputAction9(T t)
    { Debug.Log("Action (9) has no function in this State.(" + StateName + ")"); }
}