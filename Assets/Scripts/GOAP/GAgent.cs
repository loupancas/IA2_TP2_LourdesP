using FSM;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Unity.IO.LowLevel.Unsafe;

public class GAgent :  BaseEnemy
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
    [SerializeField] private EnemyGetWeapon _enemyGetWeapon;

    public string _weapon = "none";
    public float _distanceToPlayer = 5f;
    public float _viewRadius = 5f;
    public float _closeView = 1.5f;
    MeshRenderer _Gun;
    [SerializeField] private GameObject weaponObject;
    private new void Awake()
    {
        base.Awake();
        _Gun = weaponObject.GetComponent<MeshRenderer>();
        _Gun.enabled = false;

        _fsm = new FiniteStateMachine(_enemyIdle, StartCoroutine);
        _state = new GState();
        //Idle
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyIdle, _enemyMovement);
        _fsm.AddTransition(StateTransitions.ToAOA, _enemyIdle, _enemyAttackAOA);
        _fsm.AddTransition(StateTransitions.ToAttack, _enemyIdle, _enemyAttackWeapon);
        _fsm.AddTransition(StateTransitions.ToGetWeapon, _enemyIdle, _enemyMovement);
        _fsm.AddTransition(StateTransitions.ToRest, _enemyIdle, _enemyRest);
        _fsm.AddTransition(StateTransitions.ToAproach, _enemyIdle, _enemyAproach);
        
        //Move
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyMovement, _enemyIdle);
        //GetWeapon
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyGetWeapon, _enemyIdle);

        //AttackAOA
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyAttackAOA, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyAttackAOA, _enemyMovement);
        //AttackWeapon
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyAttackWeapon, _enemyIdle);
        //_fsm.AddTransition(StateTransitions.ToAttack, _enemyAttackWeapon, _enemyAttackWeapon);

        //Rest
        _fsm.AddTransition(StateTransitions.ToIdle, _enemyRest, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToPursuit, _enemyRest, _enemyMovement);

        //Aproach
        //_fsm.AddTransition(StateTransitions.ToIdle, _enemyAproach, _enemyIdle);
        _fsm.AddTransition(StateTransitions.ToAttack, _enemyAproach, _enemyAttackWeapon);



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
        //planngoap();

        

    }


    private void Update()
    {
        _fsm.Update();

        //_state.Set("IsAlive", true);
        //_state.Set("Weapon", GetWeapon());
        //_state.Set("DistanciaPlayer", GetDistanceToPlayer());
        //_state.Set("Fatiga", GetFatigue());

        PlanAndExecute();
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

    private void PlanAndExecute()
    {
        var actions = new List<GAction>{
                                    new GAction("Buscar jugador", 3)
                                         .Effect("DistanciaPlayer", 2.5f)
                                        .LinkedState(_enemyMovement),

                                    new GAction("Ataque de área", 10)
                                        .Pre("DistanciaPlayer", 3f)
                                        .Effect("Fatiga", 6)
                                        .LinkedState(_enemyAttackAOA),

                                    new GAction("Ataque con arma", 5)
                                        .Pre("DistanciaPlayer", 1.5f)
                                        .Pre("Weapon", "HasWeapon")
                                        .Effect("Fatiga", 2)
                                        .LinkedState(_enemyAttackWeapon),

                                    new GAction("Obtener arma", 1)
                                        .Pre("IsAlive", true)
                                        .Pre("Weapon", "none")
                                        .Pre("DistanciaPlayer", 3f)
                                        .Effect("Weapon", "HasWeapon")
                                        .LinkedState(_enemyGetWeapon),

                                    new GAction("Descansar", 15)
                                        .Pre("Fatiga", 1) // Solo descansará si tiene fatiga mayor a 0
                                        .Effect("Fatiga", 0)
                                         .LinkedState(_enemyRest),

                                    new GAction("Acercarse", 3)
                                        .Pre("DistanciaPlayer", _viewRadius)
                                        .Effect("DistanciaPlayer", 1.4f)
                                        .LinkedState(_enemyAproach)// Reduce la distancia para atacar
                                          };

           var from = new GState();
           from.state["isPlayerInSight"] = IsPlayerInSight();
           from.state["Weapon"] = GetWeapon();
           from.state["DistanciaPlayer"] = GetDistanceToPlayer();
           from.state["Fatiga"] = GetFatigue();
           from.state["isPlayerAlive"] = true;

           var to = new GState();
           to.state["isPlayerAlive"] = false;

          var planner = new GPlanner();
          planner.OnPlanCompleted += OnPlanCompleted;
          planner.OnCantPlan += OnCantPlan;

          planner.Run(from, to, actions, StartCoroutine);
    }

    public bool IsPlayerInSight()
    {
        //tira raycast o algo así
        return false;
    }

    public string GetWeapon()
    {
        if (_state.Get<string>("Weapon") == "HasWeapon")
        {
            _Gun.enabled = true;
            return "HasWeapon";
        }

        return "none";
    }

    public float GetDistanceToPlayer()
    {
        //tira raycast o algo así
        return 5f;
    }

    public int GetFatigue()
    {
        
        return ActualFatigue;
    }


    private void OnPlanCompleted(IEnumerable<GAction> plan)
    {
        _fsm = GPlanner.ConfigureFSM(plan, StartCoroutine);
        _fsm.Active = true;

       // ExecutePlan(plan);
    }

    //private void ExecutePlan(IEnumerable<GAction> plan)
    //{
    //    // Ejecutar las acciones planificadas de GOAP en la FSM
    //    foreach (var action in plan)
    //    {
    //        // Cambiar el estado de la FSM según la acción
    //        if (action.Name == "Buscar jugador")
    //        {
    //            _fsm.TriggerTransition(StateTransitions.ToPursuit);
    //        }
    //        else if (action.Name == "Ataque de área")
    //        {
    //            _fsm.TriggerTransition(StateTransitions.ToAOA);
    //        }
    //        // Continuar con más acciones según sea necesario...
    //    }
    //}


    private void OnCantPlan()
    {
        //TODO: debuggeamos para ver por qué no pudo planear y encontrar como hacer para que no pase nunca mas
      


    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.DrawWireSphere(transform.position, _closeView);

       
    }

    //private IState currentState;
    //private Dictionary<StateTransitions, IState> stateTransitions;

    //private void Start()
    //{
    //    // Inicializa los estados y las transiciones
    //    stateTransitions = new Dictionary<StateTransitions, IState>
    //    {
    //        { StateTransitions.ToIdle, new IdleState(this) },
    //        { StateTransitions.ToMovement, new MovementState(this) },
    //        { StateTransitions.ToAOAAttack, new EnemyAOAAttack(this) },
    //        { StateTransitions.ToWeaponAttack, new EnemyWeaponAttack(this) },
    //        { StateTransitions.ToAproach, new EnemyAproach(this) },
    //        { StateTransitions.ToRest, new EnemRest(this) }
    //    };

    //    // Establece el estado inicial
    //    currentState = stateTransitions[StateTransitions.ToIdle];
    //}

    //private void Update()
    //{
    //    // Procesa la entrada y actualiza el estado actual
    //    IState nextState = currentState.ProcessInput();
    //    if (nextState != currentState)
    //    {
    //        currentState.Exit();
    //        currentState = nextState;
    //        currentState.Enter();
    //    }

    //    currentState.UpdateLoop();
    //}

    //public void ChangeState(StateTransitions transition)
    //{
    //    if (stateTransitions.ContainsKey(transition))
    //    {
    //        currentState.Exit();
    //        currentState = stateTransitions[transition];
    //        currentState.Enter();
    //    }
    //}


}

