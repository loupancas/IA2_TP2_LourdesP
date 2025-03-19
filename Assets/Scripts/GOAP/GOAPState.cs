using System.Collections.Generic;
using System.Linq;
using System;

public class GOAPState
{
   public WorldState worldState;
    public Dictionary<string, bool> values = new Dictionary<string, bool>();
    public GOAPAction generatingAction = null;
    public int step = 0;

    #region CONSTRUCTOR
    public GOAPState(GOAPAction gen = null)
    {
        generatingAction = gen;
        worldState = new WorldState();
        {
            worldState.values = new Dictionary<string, object>();

        };
    }

    public GOAPState(GOAPState source, GOAPAction gen = null)
    {
        worldState = source.worldState.Clone();
        generatingAction = gen;
        Console.WriteLine("GoapState" + gen);
    }
    #endregion

    public override bool Equals(object obj)
    {
        var result =
           obj is GOAPState other
           && other.generatingAction == generatingAction
           && other.worldState.values.Count == worldState.values.Count
           && other.worldState.values.All(kv => kv.In(worldState.values));
        return result;
    }

    public override int GetHashCode()
    {
       
        return worldState.values.Count == 0 ? 0 : 31 * worldState.values.Count + 31 * 31 * worldState.values.First().GetHashCode();
    }

    public override string ToString()
    {
        var str = "";
        foreach (var kv in values.OrderBy(x => x.Key))
        {
            str += $"{kv.Key:12} : {kv.Value}\n";
        }
        return "--->" + (generatingAction != null ? generatingAction.name : "NULL") + "\n" + str;
    }




}

public struct WorldState
{
    public Dictionary<string, object> values;// eliminar y utilizar todas las variables ejemplo playerHP
  

    //MUY IMPORTANTE TENER UN CLONE PARA NO TENER REFENCIAS A LO VIEJO
    public WorldState Clone()
    {
        return new WorldState()
        {
            values = this.values.ToDictionary(kv => kv.Key, kv => kv.Value),//eliminar
    

        };
    }
}