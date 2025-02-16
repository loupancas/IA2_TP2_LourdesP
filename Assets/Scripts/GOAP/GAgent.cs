using FSM;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GAgent : MonoBehaviour
{
    public GState _state;
    private List<GAction> actions = new List<GAction>();
    private Queue<GAction> actionQueue;
    private GPlanner planner = new GPlanner();
    private IEnumerator currentActionRoutine;
    private FiniteStateMachine _fsm;
    void Start()
    {
        // **Estado inicial**
        GState originalState = new GState();
        originalState.Set("IsAlive", true);
        originalState.Set("Weapon", "none");
        originalState.Set("DistanciaPlayer", 5f); // Inicialmente lejos
        originalState.Set("Fatiga", 0);
        GState clonedState = new GState(originalState);
        // **Definir acciones**

        //// Buscar jugador
        //actions.Add(new GAction("Buscar jugador", 3,
        //    state => state.Get<float>("DistanciaPlayer") > 3f && state.Get<bool>("IsAlive"),
        //    state => state.Set("DistanciaPlayer", 2.5f))); // Reduce la distancia

        //// Ataque 2 (ataque de área)
        //actions.Add(new GAction("Ataque de área", 10,
        //    state => state.Get<float>("DistanciaPlayer") < 3f && state.Get<int>("Fatiga") < 6,
        //    state => {
        //        Debug.Log("¡Ataque de área ejecutado!");
        //        state.Set("Fatiga", state.Get<int>("Fatiga") + 6);
        //    }));

        //// Ataque 1 (ataque con arma)
        //actions.Add(new GAction("Ataque con arma", 5,
        //    state => state.Get<float>("DistanciaPlayer") < 1.5f && state.Get<string>("Weapon") == "HasWeapon" && state.Get<int>("Fatiga") < 4,
        //    state => {
        //        Debug.Log("¡Ataque con arma ejecutado!");
        //        state.Set("Fatiga", state.Get<int>("Fatiga") + 2);
        //    }));

        //// Obtener arma
        //actions.Add(new GAction("Obtener arma", 1,
        //    state => state.Get<bool>("IsAlive") && state.Get<string>("Weapon") == "none" && state.Get<float>("DistanciaPlayer") > 3f,
        //    state => state.Set("Weapon", "HasWeapon")));

        //// Descansar
        //actions.Add(new GAction("Descansar", 15,
        //    state => state.Get<int>("Fatiga") > 0,
        //    state => state.Set("Fatiga", 0)));

        //// Acercarse para usar ataque con arma
        //actions.Add(new GAction("Acercarse", 3,
        //    state => state.Get<float>("DistanciaPlayer") > 1.5f,
        //    state => state.Set("DistanciaPlayer", 1.4f))); // Reduce la distancia para atacar

        // **Definir objetivo**
        GState goal = new GState();
        goal.Set("HacerDaño", true); // Indicamos que el objetivo es atacar al jugador

        // **Generar plan**
       //actionQueue = new Queue<GAction>(planner.Run(_state, goal, actions));

        StartCoroutine(ExecuteActions());
    }

    IEnumerator ExecuteActions()
    {
        while (actionQueue.Count > 0)
        {
            GAction action = actionQueue.Dequeue();
            Debug.Log($"Ejecutando acción: {action.Name}");
            currentActionRoutine = PerformAction(action);
            yield return StartCoroutine(currentActionRoutine);
        }
        Debug.Log("Plan completado");
    }

    IEnumerator PerformAction(GAction action)
    {
        yield return new WaitForSeconds(action.Cost);
        action.ApplyEffect(_state);
    }
}
