// StateMachine.cs
using System;

public class StateMachine
{
    private IState currentState;

    public void ChangeState(IState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        if (currentState != null) currentState.Enter();
    }

    public void Tick(float dt)
    {
        if (currentState != null) currentState.Tick(dt);
    }

    public IState CurrentState => currentState;
}
