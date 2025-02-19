using FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
       
        // **Definir acciones**

        // Definir acciones
        //planngoap();
        PlanAndExecute();


    }


    private void Update()
    {
        _fsm.Update();


    }

   
    private void PlanAndExecute()
    {
        var initialState = new GState { worldState = new WorldState { playerHP = 30, distance = 10, hasWeapon = false, weapon = "none" } };
        var goalState = new GState { worldState = new WorldState { playerHP = 0, distance = 0, hasWeapon = true, weapon = "espada" } };
        var actions = new List<GAction>{
                                    new GAction("Buscar jugador", 3,
                                         s => s.worldState.distance > 0,
                                         s => { var ns = s.worldState.Clone(); ns.distance -= 5; return new GState { worldState = ns }; }),

                                    new GAction("Ataque con arma", 5,
                                         s => s.worldState.weapon == "espada" && s.worldState.distance < 3,
                                         s => { var ns = s.worldState.Clone(); ns.playerHP -= 10; return new GState { worldState = ns }; }),
                                    new GAction("Obtener arma",1,
                                    s => s.worldState.weapon == "none",
                                    s => { var ns = s.worldState.Clone(); ns.weapon = "espada"; return new GState { worldState = ns }; }),
                                    new GAction("Descansar", 15,
                                        s => s.worldState.playerHP < 50,
                                        s => { var ns = s.worldState.Clone(); ns.playerHP += 20; return new GState { worldState = ns }; }),
                                          };


        var plan = Goap.Execute(initialState, goalState,
            s => s.worldState.playerHP >= goalState.worldState.playerHP
                 && s.worldState.distance <= goalState.worldState.distance
                 && s.worldState.weapon == goalState.worldState.weapon,
            s => Math.Abs(goalState.worldState.playerHP - s.worldState.playerHP) + Math.Abs(goalState.worldState.distance - s.worldState.distance),
            actions);

        //var from = new GState();
        //   from.state["isPlayerInSight"] = IsPlayerInSight();
        //   from.state["Weapon"] = GetWeapon();
        //   from.state["DistanciaPlayer"] = GetDistanceToPlayer();
        //   from.state["Fatiga"] = GetFatigue();
        //   from.state["isPlayerAlive"] = true;

        //   var to = new GState();
        //   to.state["isPlayerAlive"] = false;

        //  var planner = new GPlanner();
          planner.OnPlanCompleted += OnPlanCompleted;
          planner.OnCantPlan += OnCantPlan;

          //planner.Run(from, to, actions, StartCoroutine);
    }

    public bool IsPlayerInSight()
    {
        //tira raycast o algo así
        return false;
    }

    public string GetWeapon()
    {
        //if (_state.Get<string>("Weapon") == "HasWeapon")
        //{
        //    _Gun.enabled = true;
        //    return "HasWeapon";
        //}

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

    }

 

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

    
}

