using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class GOAP : MonoBehaviour
{
    public static IEnumerable<GOAPAction> Execute(GOAPState from, GOAPState to, Func<GOAPState, bool> satisfies, Func<GOAPState, float> h, IEnumerable<GOAPAction> actions)
    {
        int watchdog = 200;

        IEnumerable<GOAPState> seq = AStarNormal<GOAPState>.Run(
            from,
            to,
            (curr, goal) => h(curr),
            satisfies,
            curr =>
            {
                if (watchdog == 0)

                    return Enumerable.Empty<AStarNormal<GOAPState>.Arc>();
                else
                    watchdog--;
                Debug.Log($"Current state-----: {curr}");

                // Filter actions based on preconditions
                //var validActions = actions.Where(action => action.CheckPreconditions(curr)).ToList();

                //// Log valid actions
                //foreach (var action in validActions)
                //{
                //    Debug.Log($"Valid action: {action}");
                //}

                //en este Where se evaluan las precondiciones, al ser un diccionario de <string,bool> solo se chequea que todas las variables concuerdes
                //En caso de ser un Func<...,bool> se utilizaria ese func de cada estado para saber si cumple o no
                //return actions.Where(action => action.preconditions.All(kv => kv.In(curr.worldState.values)))//quitar este where si no se usan en diccionario
                //return actions.Where(action => action.precon.All(kv => kv.In(curr.worldState.values)))
                // return actions.Where(action => action.precon.All(kv => curr.worldState.values.ContainsKey(kv.Key))) //&& curr.worldState.values[kv.Key] == (bool)kv.Value))
                //return actions.Where(action => action.CheckPreconditions(curr))  
                // Agregue esto para chequear las precondiuciones puestas  en el Func, Al final deberia quedar solo esta
                return actions.Where(action => action.preconditions.All(kv => curr.worldState.values.ContainsKey(kv.Key)))
                              .Where(a => a.CheckPreconditions(curr))                          //dejar si se calculan las precondiciones con lambdas
                              .Aggregate(new FList<AStarNormal<GOAPState>.Arc>(), (possibleList, action) =>
                              {
                                  var newState = new GOAPState(curr);
                                  //Debug.Log("Estado antes de aplicar efectos: " + curr);
                                  newState = action.Effects(newState); // se aplican lso effectos del Func
                                  //Debug.Log("Estado después de aplicar efectos: " + newState);
                                  newState.generatingAction = action;
                                  //Debug.Log("Estado después de aplicar efectos y setear generatingAction: " + newState);
                                  newState.step = curr.step + 1;

                                  return possibleList + new AStarNormal<GOAPState>.Arc(newState, action.cost);


                              });
            });

        if (seq == null)
        {
            Debug.Log("Imposible planear");
            return null;
        }

        foreach (var act in seq.Skip(1))
        {
            Debug.Log(act);

        }

        Debug.Log("WATCHDOG " + watchdog);

        return seq.Skip(1).Select(x => x.generatingAction);
    }
}
