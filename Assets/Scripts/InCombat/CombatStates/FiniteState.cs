using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class FiniteState <T>
{
    public StateMachine<T> Machine;

    public FiniteState(StateMachine<T> machine) { Machine = machine; }

    abstract public void Enter(T t);
    abstract public void Execute(T t);
    abstract public void Exit(T t);

    public virtual void ChangeState(FiniteState<T> newState)
    {
        Machine.ChangeState(newState);
    }
}
