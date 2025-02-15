using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPAgent : MonoBehaviour
{
    public WorldState worldState;
    private List<ActionGoap> actions = new List<ActionGoap>();
    private Queue<ActionGoap> actionQueue;
    private Planner planner = new Planner();
    private IEnumerator currentActionRoutine;

    void Start()
    {
        // **Estado inicial**
        worldState = new WorldState();
        worldState.Set("IsAlive", true);
        worldState.Set("Weapon", "none");
        worldState.Set("DistanciaPlayer", 5f); // Inicialmente lejos
        worldState.Set("Fatiga", 0);

        // **Definir acciones**

        // Buscar jugador
        actions.Add(new ActionGoap("Buscar jugador", 3,
            state => state.Get<float>("DistanciaPlayer") > 3f && state.Get<bool>("IsAlive"),
            state => state.Set("DistanciaPlayer", 2.5f))); // Reduce la distancia

        // Ataque 2 (ataque de área)
        actions.Add(new ActionGoap("Ataque de área", 10,
            state => state.Get<float>("DistanciaPlayer") < 3f && state.Get<int>("Fatiga") < 6,
            state => {
                Debug.Log("¡Ataque de área ejecutado!");
                state.Set("Fatiga", state.Get<int>("Fatiga") + 6);
            }));

        // Ataque 1 (ataque con arma)
        actions.Add(new ActionGoap("Ataque con arma", 5,
            state => state.Get<float>("DistanciaPlayer") < 1.5f && state.Get<string>("Weapon") == "HasWeapon" && state.Get<int>("Fatiga") < 4,
            state => {
                Debug.Log("¡Ataque con arma ejecutado!");
                state.Set("Fatiga", state.Get<int>("Fatiga") + 2);
            }));

        // Obtener arma
        actions.Add(new ActionGoap("Obtener arma", 1,
            state => state.Get<bool>("IsAlive") && state.Get<string>("Weapon") == "none" && state.Get<float>("DistanciaPlayer") > 3f,
            state => state.Set("Weapon", "HasWeapon")));

        // Descansar
        actions.Add(new ActionGoap("Descansar", 15,
            state => state.Get<int>("Fatiga") > 0,
            state => state.Set("Fatiga", 0)));

        // Acercarse para usar ataque con arma
        actions.Add(new ActionGoap("Acercarse", 3,
            state => state.Get<float>("DistanciaPlayer") > 1.5f,
            state => state.Set("DistanciaPlayer", 1.4f))); // Reduce la distancia para atacar

        // **Definir objetivo**
        WorldState goal = new WorldState();
        goal.Set("HacerDaño", true); // Indicamos que el objetivo es atacar al jugador

        // **Generar plan**
        actionQueue = new Queue<ActionGoap>(planner.Plan(worldState, actions, goal));

        StartCoroutine(ExecuteActions());
    }

    IEnumerator ExecuteActions()
    {
        while (actionQueue.Count > 0)
        {
            ActionGoap action = actionQueue.Dequeue();
            Debug.Log($"Ejecutando acción: {action.Name}");
            currentActionRoutine = PerformAction(action);
            yield return StartCoroutine(currentActionRoutine);
        }
        Debug.Log("Plan completado");
    }

    IEnumerator PerformAction(ActionGoap action)
    {
        yield return new WaitForSeconds(action.Cost);
        action.ApplyEffect(worldState);
    }
}
