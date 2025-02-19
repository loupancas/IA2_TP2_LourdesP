using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Goap : MonoBehaviour
{
    //El satisfies y la heuristica ahora son Funciones externas
    public static IEnumerable<GAction> Execute(GState from, GState to, Func<GState, bool> satisfies, Func<GState, float> h, IEnumerable<GAction> actions)
    {
        Debug.Log($"Estado inicial: {from.worldState}");
        Debug.Log($"Estado objetivo: {to.worldState}");
        int watchdog = 200;

        IEnumerable<GState> seq = AStarNormal<GState>.Run(
            from,
            to,
            (curr, goal) => h(curr),
            satisfies,
            curr =>
            {
                if (watchdog == 0)
                    return Enumerable.Empty<AStarNormal<GState>.Arc>();
                else
                    watchdog--;

                //en este Where se evaluan las precondiciones, al ser un diccionario de <string,bool> solo se chequea que todas las variables concuerdes
                //En caso de ser un Func<...,bool> se utilizaria ese func de cada estado para saber si cumple o no
                //return actions.Where(action => action.preconditions.All(kv => kv.In(curr.worldState.values)))//quitar este where si no se usan en diccionario
                return actions.Where(a => a.Preconditions(curr)) // Agregue esto para chequear las precondiuciones puestas  en el Func, Al final deberia quedar solo esta
                                                                 //dejar si se calculan las precondiciones con lambdas
                              .Aggregate(new FList<AStarNormal<GState>.Arc>(), (possibleList, action) =>
                              {
                                  var newState = new GState(curr);
                                  newState = action.Effects(newState); // se aplican lso effectos del Func
                                  newState.generatingAction = action;
                                  newState.step = curr.step + 1;
                                  Debug.Log($"Evaluando acción: {action.Name}, Estado resultante: {newState.worldState}");
                                  return possibleList + new AStarNormal<GState>.Arc(newState, action.Cost);
                              });
            });

        if (seq == null)
        {
            Debug.Log("Imposible planear");
            return null;
        }

        foreach (var act in seq.Skip(1))
        {
            Debug.Log($"Acción en el plan: {act.generatingAction.Name}");
        }

        Debug.Log("WATCHDOG " + watchdog);

        return seq.Skip(1).Select(x => x.generatingAction);
    }
}
