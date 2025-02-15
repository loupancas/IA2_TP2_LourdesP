using System;
using System.Collections.Generic;
using System.Linq;

public class WorldState
{
    private Dictionary<string, object> state = new Dictionary<string, object>();

    public void Set<T>(string key, T value) => state[key] = value;
    public T Get<T>(string key) => state.ContainsKey(key) ? (T)state[key] : default;
    public bool Has(string key) => state.ContainsKey(key);

    public WorldState Clone()
    {
        var newState = new WorldState();
        foreach (var kvp in state) newState.state[kvp.Key] = kvp.Value;
        return newState;
    }

    public bool MeetsConditions(Dictionary<string, Func<WorldState, bool>> conditions)
    {
        return conditions.All(c => c.Value(this));
    }
}
