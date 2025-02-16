using System;
using System.Collections.Generic;
using System.Linq;

public class GState
{
    private Dictionary<string, object> state = new Dictionary<string, object>();//object sera int,string,float
    public GAction generatingAction = null;
    public int step = 0;


    public void Set<T>(string key, T value) => state[key] = value; 
    public T Get<T>(string key) => state.ContainsKey(key) ? (T)state[key] : default;
    public bool Has(string key) => state.ContainsKey(key);

    //crear un diccionario con valores int float string bool 



    public bool MeetsConditions(Dictionary<string, Func<GState, bool>> conditions)
    {
        return conditions.All(c => c.Value(this));
    }
}
