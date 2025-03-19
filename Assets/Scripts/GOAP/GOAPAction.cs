using System;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using U = Utility;
using System.Linq;
public class GOAPAction
{
    public string ActionName { get; set; }
    public Dictionary<string, object> preconditions { get; private set; }
    public Dictionary<string, object> effects { get; private set; }

    public Func<GOAPState, bool> Preconditions = delegate { return true; };

    public Func<GOAPState, GOAPState> Effects;
    public string name { get; private set; }
    public float cost { get; private set; }
    public IState linkedState { get; private set; }


    public GOAPAction(string name)
    {
        this.name = name;
        cost = 1f;
        preconditions = new Dictionary<string, object>();
        effects = new Dictionary<string, object>();

        Effects = (s) =>
        {
            //Debug.Log("State before applying effects:-----------------");
            //foreach (var state in s.worldState.values)
            //{
            //    Debug.Log($"{state.Key}: {state.Value}");
            //}

            foreach (var effect in effects)
            {

                if (effect.Value is int intValue)
                {
                    //Debug.Log("Setting effect: " + effect.Key + " " + intValue);
                    s.worldState.values[effect.Key] = intValue;
                }
                else if (effect.Value is float floatValue)
                {
                    //Debug.Log("Setting effect: " + effect.Key + " " + floatValue);
                    s.worldState.values[effect.Key] = floatValue;
                }
                else if (effect.Value is bool boolValue)
                {
                    //Debug.Log("Setting effect: " + effect.Key + " " + boolValue);
                    s.worldState.values[effect.Key] = boolValue;
                }
                else if (effect.Value is string stringValue)
                {
                    //Debug.Log("Setting effect: " + effect.Key + " " + stringValue);
                    s.worldState.values[effect.Key] = stringValue;
                }
                else
                {
                    Debug.LogWarning($"Unsupported type for effect value: {effect.Value.GetType()}");
                }
            }

            //Debug.Log("State after applying effects:----------------");
            //foreach (var state in s.worldState.values)
            //{
            //    Debug.Log($"{state.Key}: {state.Value}");
            //}

            return s;


        };
    }

    public GOAPAction Cost(float cost)
    {
        if (cost < 1f)
        {
            //Costs < 1f make the heuristic non-admissible. h() could overestimate and create sub-optimal results.
            //https://en.wikipedia.org/wiki/A*_search_algorithm#Properties
            Debug.Log(string.Format("Warning: Using cost < 1f for '{0}' could yield sub-optimal results", name));
        }

        this.cost = cost;
        return this;
    }
    public GOAPAction Pre<T>(string key, T value)
    {
        preconditions[key] = value;
        return this;
    }
    public GOAPAction Pre(string s, bool value)
    {
        preconditions[s] = value;
        return this;
    }

    public GOAPAction Pre(string key, string value)
    {
        preconditions[key] = value;
        return this;
    }

    public GOAPAction Pre(string key, int value)
    {
        preconditions[key] = value;
        return this;
    }

    public GOAPAction Pre(string key, float value)
    {
        preconditions[key] = value;
        return this;
    }

    public GOAPAction Pre(Func<GOAPState, bool> p)
    {
        Preconditions = p;
        return this;
    }

    public GOAPAction Effect(string s, bool value)
    {
        effects[s] = value;
        return this;
    }

    public GOAPAction Effect(string s, string value)
    {
        effects[s] = value;
        return this;
    }

    public GOAPAction Effect(string s, int value)
    {
        effects[s] = value;
        return this;
    }

    public GOAPAction Effect(string s, float value)
    {
        effects[s] = value;
        return this;
    }


    public GOAPAction Effect(Func<GOAPState, GOAPState> e)
    {
        Effects = e;
        return this;
    }


    public GOAPAction SetItem(IState state)
    {
        linkedState = state;
        return this;
    }


    public bool CheckPreconditions(GOAPState state)
    {
        Debug.Log("Checking preconditions for: " + name);
        foreach (var pre in preconditions)
        {

            if (!CheckPrecondition(state, pre.Key, pre.Value))
            {
                Debug.Log($"{pre.Key}: expected {pre.Value}, actual {state.worldState.values[pre.Key]} -----FAIL-------");
                return false;
            }


        }

        Debug.Log("Preconditions met for: " + name);

        return true;
    }




    private bool CheckPrecondition(GOAPState state, string key, object value)
    {
        switch (key)
        {

            case "deadMace":  //Debug.Log("Checking deadMace"+value); 
                return state.worldState.values["deadMace"].Equals(value);
            case "otherHasMace":
                //Debug.Log("Checking otherHasMace" + value);
                return state.worldState.values["otherHasMace"].Equals(value);
            case "accessibleMace":
                //Debug.Log("Checking accessibleMace" + value);
                return state.worldState.values["accessibleMace"].Equals(value);
            case "hasMace":
                //Debug.Log("Checking hasMace" + value);
                return state.worldState.values["hasMace"].Equals(value);
            case "deadPastaFrola": //Debug.Log("Checking deadPastafrola" + value);
                return state.worldState.values["deadPastaFrola"].Equals(value);
            case "otherHasPastaFrola": //Debug.Log("Checking otherHasPastafrola" + value);
                return state.worldState.values["otherHasPastaFrola"].Equals(value);
            case "accessiblePastaFrola":// Debug.Log("Checking accessiblePastafrola" + value);
                return state.worldState.values["accessiblePastaFrola"].Equals(value);
            case "hasPastaFrola":// Debug.Log("Checking hasPastafrola" + value);
                return state.worldState.values["hasPastaFrola"].Equals(value);

            case "deadKey":// Debug.Log("Checking deadKey" + value);
                return state.worldState.values["deadKey"].Equals(value);
            case "otherHasKey":// Debug.Log("Checking otherHasKey" + value);
                return state.worldState.values["otherHasKey"].Equals(value);
            case "accessibleKey": //Debug.Log("Checking accessibleKey" + value);
                return state.worldState.values["accessibleKey"].Equals(value);
            case "hasKey": //Debug.Log("Checking hasKey" + value);
                return state.worldState.values["hasKey"].Equals(value);

            case "accessibleEntity": //Debug.Log("Checking accessibleEntity" + value);
                return state.worldState.values["accessibleEntity"].Equals(value);
            case "deadEntity": //Debug.Log("Checking deadEntity" + value);
                return state.worldState.values["deadEntity"].Equals(value);
            case "entity":// Debug.Log("Checking entity" + value);
                return state.worldState.values["entity"].Equals(value);

            case "deadDoor":// Debug.Log("Checking deadDoor" + value);
                return state.worldState.values["deadDoor"].Equals(value);
            case "accessibleDoor": //Debug.Log("Checking accessibleDoor" + value);
                return state.worldState.values["accessibleDoor"].Equals(value);
            case "doorOpen":// Debug.Log("Checking doorOpen" + value);
                return state.worldState.values["doorOpen"].Equals(value);

            case "money"://Debug.Log("Checking money" + value);
                return state.worldState.values["money"].Equals(value);
            default:
                return false;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is GOAPAction other)
        {
            return ActionName == other.ActionName
                && preconditions.Count == other.preconditions.Count
                && effects.Count == other.effects.Count
                && preconditions.All(kv => other.preconditions.ContainsKey(kv.Key) && other.preconditions[kv.Key].Equals(kv.Value))
                && effects.All(kv => other.effects.ContainsKey(kv.Key) && other.effects[kv.Key].Equals(kv.Value));
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + (ActionName?.GetHashCode() ?? 0);
        foreach (var kv in preconditions)
        {
            hash = hash * 31 + kv.Key.GetHashCode();
            hash = hash * 31 + (kv.Value?.GetHashCode() ?? 0);
        }
        foreach (var kv in effects)
        {
            hash = hash * 31 + kv.Key.GetHashCode();
            hash = hash * 31 + (kv.Value?.GetHashCode() ?? 0);
        }
        return hash;
    }


}
