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
    private FiniteStateMachine _fsm;

    [SerializeField] private EnemyMovement _enemyMovement;
    [SerializeField] private EnemyIdle _enemyIdle;
    [SerializeField] private EnemyAOAAttack _enemyAttackAOA;
    [SerializeField] private EnemyWeaponAttack _enemyAttackWeapon;
    [SerializeField] private EnemRest _enemyRest;
    [SerializeField] private EnemyAproach _enemyAproach;

    private void Awake()
    {
        _fsm = new FiniteStateMachine(_enemyIdle, StartCoroutine);

        //Idle
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyIdle, _enemyMovement);
        _fsm.AddTransition(StateTransitions.ToAOA, _enemyIdle, _enemyAttackAOA);
        _fsm.AddTransition(StateTransitions.ToAttack, _enemyIdle, _enemyAttackWeapon);
        _fsm.AddTransition(StateTransitions.ToGetWeapon, _enemyIdle, _enemyMovement);
        _fsm.AddTransition(StateTransitions.ToRest, _enemyIdle, _enemyRest);
        _fsm.AddTransition(StateTransitions.ToAproach, _enemyIdle, _enemyAproach);


        //Move
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyMovement, _enemyIdle);

        //AttackAOA
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyAttackAOA, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyAttackAOA, _enemyMovement);
        //AttackWeapon
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyAttackWeapon, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToAttack, _enemyAttackWeapon, _enemyAttackWeapon);

        //Rest
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyRest, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyRest, _enemyMovement);

        //Aproach
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyAproach, _enemyIdle);



        _fsm.Active= true;
    }


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

        // Definir acciones
        planngoap();

        // Definir objetivo
        GState goal = new GState();
        goal.Set("HacerDaño", true);

        // Generar plan
        planner.Run(_state, goal, actions, StartCoroutine);

       
    }


    private void Update()
    {
        _fsm.Update();
    }

    private void planngoap()
    {
        actions = new List<GAction>
        {
        new GAction("Buscar jugador", 3)
            .Effect("DistanciaPlayer", 2.5f)
            .LinkedState(_enemyMovement),

        new GAction("Ataque de área", 10)
            .Pre("DistanciaPlayer", 3f)
            .Pre("Fatiga", 6)
            .Effect("Fatiga", 6)
            .LinkedState(_enemyAttackAOA),

        new GAction("Ataque con arma", 5)
            .Pre("DistanciaPlayer", 1.5f)
            .Pre("Weapon", "HasWeapon")
            .Pre("Fatiga", 4)
            .Effect("Fatiga", 2)
            .LinkedState(_enemyAttackWeapon),

        new GAction("Obtener arma", 1)
            .Pre("IsAlive", true)
            .Pre("Weapon", "none")
            .Pre("DistanciaPlayer", 3f)
            .Effect("Weapon", "HasWeapon")
            .LinkedState(_enemyMovement),

        new GAction("Descansar", 15)
            .Pre("Fatiga", 1) // Solo descansará si tiene fatiga mayor a 0
            .Effect("Fatiga", 0)
             .LinkedState(_enemyRest),

        new GAction("Acercarse", 3)
            .Pre("DistanciaPlayer", 1.5f)
            .Effect("DistanciaPlayer", 1.4f)
            .LinkedState(_enemyAproach)// Reduce la distancia para atacar
         };
    }
   

    IEnumerator PerformAction(GAction action)
    {
        yield return new WaitForSeconds(action.Cost);
        action.ApplyEffect(_state);
    }
}
