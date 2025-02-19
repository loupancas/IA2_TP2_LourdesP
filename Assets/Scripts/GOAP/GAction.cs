using System;
using System.Collections.Generic;
using UnityEngine;
using FSM;


public class GAction
{
    public string Name { get; private set; }
    public float Cost { get; private set; }
    public readonly Func<GState, bool> Preconditions;
    public readonly Func<GState, GState> Effects;
    public ItemType item;

    public Dictionary<string, object> preconditions { get; private set; }
    public Dictionary<string, object> effects { get; private set; }
    public IState linkedState { get; private set; }
    //fsm o ienumerable

    public GAction(string name, float cost, Func<GState, bool> pre, Func<GState, GState> eff)
    {
        Name = name;
        Cost = cost;
        Preconditions = pre;
        Effects =eff;
    }



    public GAction SetCosts(float cost)
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

    public GAction Pre<T>(string key, T value)
    {
        preconditions[key] = value;
        return this;
    }

    public GAction Effect<T>(string key, T value)
    {
        effects[key] = value;
        return this;
    }

    public GAction LinkedState(IState state)
    {
        linkedState = state;
        return this;
    }

    public GAction SetItem(ItemType type)
    {
        item = type;
        return this;
    }

}
