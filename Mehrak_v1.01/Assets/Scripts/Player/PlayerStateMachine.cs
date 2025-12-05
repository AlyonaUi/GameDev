using System;
using UnityEngine;

public class PlayerStateMachine
{
    public enum State
    {
        Idle,
        Moving
    }

    public State Current { get; private set; } = State.Idle;

    public event Action<State> OnStateChanged;

    public void UpdateState(Vector2 velocity)
    {
        var newState = velocity.sqrMagnitude > 0.001f ? State.Moving : State.Idle;
        if (newState != Current)
        {
            Current = newState;
            OnStateChanged?.Invoke(Current);
        }
    }
}