using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine {
    public abstract class BaseState
    {
        public FiniteStateMachine fsm = null;
        public abstract void Update();
    }

    public abstract class State<FSM> : BaseState where FSM : FiniteStateMachine
    {
        protected FSM GetFSM()
        {
            Debug.Assert(fsm != null);
            return fsm as FSM;
        }
    }

    BaseState current_state = null;
    Dictionary<string, BaseState> all_states = null;
    bool is_paused = false;

    protected FSMState GetCurrentState<FSMState>() where FSMState : BaseState
    {
        return current_state as FSMState;
    }

    public FiniteStateMachine(string start_state_key, Dictionary<string, BaseState> states)
    {
        Debug.Assert(!states.ContainsKey(start_state_key));
        foreach (KeyValuePair<string, BaseState> kvp in states)
        {
            kvp.Value.fsm = this;
        }
        all_states = states;
        TransitionTo(start_state_key);
    }

    public void Pause()
    {
        if (is_paused)
        {
            Debug.LogWarningFormat("Attempting to pause paused FSM { 0 : s }", this.GetType().ToString());
        }
        is_paused = true;
    }

    public void Unpause()
    {
        if (!is_paused)
        {
            Debug.LogWarningFormat("Attempting to unpause unpaused FSM { 0 : s }", this.GetType().ToString());
        }
        is_paused = false;
    }

    public void Update()
    {
        if (!is_paused)
        {
            current_state.Update();
        }
    }

    public void TransitionTo(string state_key)
    {
        Debug.Assert(all_states.ContainsKey(state_key));
        this.current_state = all_states[state_key];
    }

}
