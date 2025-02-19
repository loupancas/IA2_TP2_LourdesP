using System;
using System.Collections.Generic;
using System.Linq;

public class GState
{
    public WorldState worldState;
    public Dictionary<string, object> state = new Dictionary<string, object>();//object sera int,string,float
    public GAction generatingAction = null;
    public int step = 0;

    #region CONSTRUCTORES
    // Constructor por defecto
    public GState(GAction gen = null)
    {
        generatingAction = gen;
        worldState = new WorldState()
        {
            //values = new Dictionary<string, bool>() // Muy importane inicializarlo en este caso

            playerHP = 100,
            distance = 0,
            weapon = "",
            hasWeapon = false
        };


    }

    // Constructor para copia
    public GState(GState source, GAction gen = null)
    {
        //foreach (var elem in source.state)
        //{
        //    if (state.ContainsKey(elem.Key))
        //        state[elem.Key] = elem.Value;
        //    else
        //        state.Add(elem.Key, elem.Value);
        //}
        worldState = source.worldState.Clone();
        generatingAction = gen;
    }
    #endregion

    //public void Set<T>(string key, T value) => state[key] = value; 
    //public T Get<T>(string key) => state.ContainsKey(key) ? (T)state[key] : default;
    //public bool Has(string key) => state.ContainsKey(key);

    //crear un diccionario con valores int float string bool 


    public override bool Equals(object obj)
    {
        //if (obj is GState other)
        //{
        //    return other.generatingAction == generatingAction &&
        //           other.state.Count == state.Count &&
        //           other.state.All(kv => state.ContainsKey(kv.Key) && Equals(state[kv.Key], kv.Value));
        //}
        //return false;

        var result =
           obj is GState other
           && other.generatingAction == generatingAction; 
           //&& other.worldState.values.Count == worldState.values.Count
           //&& other.worldState.values.All(kv => kv.In(worldState.values));
        return result;
    }

    public override int GetHashCode()
    {
        return state.Count == 0 ? 0 : 31 * state.Count + 31 * 31 * state.First().GetHashCode();
    }

    public override string ToString()
    {
        return "---> " + (generatingAction != null ? generatingAction.Name : "NULL") + "\n" +
               string.Join("\n", state.OrderBy(x => x.Key).Select(kv => $"{kv.Key}: {kv.Value}"));
    }

}


public struct WorldState
{
    public int playerHP;
    public float distance;
    public string weapon;
    public bool hasWeapon;
    //float string bool 
    //public Dictionary<string, bool> values;// eliminar y utilizar todas las variables ejemplo playerHP

    //MUY IMPORTANTE TENER UN CLONE PARA NO TENER REFENCIAS A LO VIEJO
    public WorldState Clone()
    {
        return new WorldState()
        {
            playerHP = this.playerHP,
            //values = this.values.ToDictionary(kv => kv.Key, kv => kv.Value)//eliminar
        };
    }
}