using System;
using System.Collections.Generic;
using UnityEngine;
using FSM;


public class GAction
{
    public string Name { get; private set; }
    public float Cost { get; private set; }
    public Func<GState, bool> preconditions { get; private set; }
    public Action<GState> effects { get; private set; }

    //public Dictionary<string, T> preconditions { get; private set; }
    //public Dictionary<string, T> effects { get; private set; }
    public IState linkedState { get; private set; }
    //fsm o ienumerable

    public GAction(string name, float cost, Func<GState, bool> precondition, Action<GState> effect)
    {
        Name = name;
        Cost = cost;
        preconditions = precondition;
        effects = effect;
    }

    //public GAction(string name)
    //{
    //    this.name = name;
    //    cost = 1f;
    //    preconditions = new Dictionary<string, T>();
    //    effects = new Dictionary<string, T>();
    //}

    public GAction Costs(float cost)
    {
        if (cost < 1f)
        {
            //Costs < 1f make the heuristic non-admissible. h() could overestimate and create sub-optimal results.
            //https://en.wikipedia.org/wiki/A*_search_algorithm#Properties
            Debug.Log(string.Format("Warning: Using cost < 1f for '{0}' could yield sub-optimal results", Name));
        }

        this.Cost = cost;
        return this;
    }

    //public GAction Pre(string s, bool value)
    //{
    //    preconditions[s] = value;
    //    return this;
    //}

    //public GAction Effect(string s, bool value)
    //{
    //    effects[s] = value;
    //    return this;
    //}

    public GAction LinkedState(IState state)
    {
        linkedState = state;
        return this;
    }

  
}
