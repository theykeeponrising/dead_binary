using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine <T>
{
    public T Owner;
    private FiniteState<T> CurrentState;

    public void Awake()
    {
        CurrentState = null;
    }

    public void Configure(T owner, FiniteState<T> InitialState)
    {
        Owner = owner;
        ChangeState(InitialState);
    }

    public void Update()
    {
        if (CurrentState != null) CurrentState.Execute(Owner);
    }

    public void ChangeState(FiniteState<T> NewState)
    {
        if (CurrentState != null)
            CurrentState.Exit(Owner);
        CurrentState = NewState;
        if (CurrentState != null)
            CurrentState.Enter(Owner);
    }

    public FiniteState<T> GetCurrentState()
    {
        return CurrentState;
    }

    public void Quit()
    {
        if (CurrentState != null) CurrentState.Exit(Owner);
    }
}
