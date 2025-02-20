using System;
using System.Collections.Generic;
using System.Linq;

public class GState
{
    public WorldState worldState;
    public GAction generatingAction = null;
    public int step = 0;

    #region CONSTRUCTORES
    // Constructor por defecto
    public GState(GAction gen = null)
    {
        generatingAction = gen;
        worldState = new WorldState();



    }

    // Constructor para copia
    public GState(GState source, GAction gen = null)
    {
      
        worldState = source.worldState.Clone();
        generatingAction = gen;
    }
    #endregion


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
        //return state.Count == 0 ? 0 : 31 * state.Count + 31 * 31 * state.First().GetHashCode();

        return worldState.GetHashCode();
    }

    public override string ToString()
    {
        //return "---> " + (generatingAction != null ? generatingAction.Name : "NULL") + "\n" +
               //string.Join("\n", state.OrderBy(x => x.Key).Select(kv => $"{kv.Key}: {kv.Value}"));
        return $"HP: , Distancia: , Vivo: , Arma: ";
    }

}


public struct WorldState
{
    public int playerHP;
    public float distance;
    public string weapon;
    public bool hasWeapon;

    public object values { get; internal set; }

    //MUY IMPORTANTE TENER UN CLONE PARA NO TENER REFENCIAS A LO VIEJO
    public WorldState Clone()
    {
        return new WorldState()
        {
            playerHP = this.playerHP,
            distance = this.distance,
            weapon = this.weapon,
            hasWeapon = this.hasWeapon
        };
    }
}