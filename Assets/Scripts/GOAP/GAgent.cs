using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IA2;
using System.Linq;

public enum ActionEntity
{
    Kill,
    PickUp,
    NextStep,
    FailedStep,
    Open,
    Success
}



public class GAgent :  BaseEnemy
{
    private EventFSM<ActionEntity> _fsm;
    private Item _target;

    private NewEntity _ent;

    IEnumerable<Tuple<ActionEntity, Item>> _plan;

    public float _viewRadius = 5f;
    public float _closeView = 1.5f;

    private void PerformAttack(NewEntity us, Item other)
    {
        Debug.Log("PerformAttack", other.gameObject);
        if (other != _target) return;

        var espada = _ent.items.FirstOrDefault(it => it.type == ItemType.Espada);
        if (espada)
        {
            other.Kill();
            if (other.type == ItemType.Door)
                Destroy(_ent.Removeitem(espada).gameObject);
            _fsm.Feed(ActionEntity.NextStep);
        }
        else
            _fsm.Feed(ActionEntity.FailedStep);
    }

    private void PerformOpen(NewEntity us, Item other)
    {
        if (other != _target) return;

        var key = _ent.items.FirstOrDefault(it => it.type == ItemType.Key);
        var door = other.GetComponent<Door>();
        if (door && key)
        {
            door.Open();
            Destroy(_ent.Removeitem(key).gameObject);
            _fsm.Feed(ActionEntity.NextStep);
        }
        else
            _fsm.Feed(ActionEntity.FailedStep);
    }

    private void PerformPickUp(NewEntity us, Item other)
    {
        if (other != _target) return;

        _ent.AddItem(other);
        _fsm.Feed(ActionEntity.NextStep);
    }

    private void NextStep(NewEntity ent, Waypoint wp, bool reached)
    {
        _fsm.Feed(ActionEntity.NextStep);
    }

    private new void Awake()
    {
        base.Awake();

        _ent = GetComponent<NewEntity>();

        var any = new State<ActionEntity>("any");

        var idle = new State<ActionEntity>("idle");
        var bridgeStep = new State<ActionEntity>("planStep");
        var failStep = new State<ActionEntity>("failStep");
        var kill = new State<ActionEntity>("kill");
        var pickup = new State<ActionEntity>("pickup");
        var open = new State<ActionEntity>("open");
        var success = new State<ActionEntity>("success");

        kill.OnEnter += a => {
            _ent.GoTo(_target.transform.position);
            _ent.OnHitItem += PerformAttack;
        };

        kill.OnExit += a => _ent.OnHitItem -= PerformAttack;

        failStep.OnEnter += a => { _ent.Stop(); Debug.Log("Plan failed"); };

        pickup.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUp; };
        pickup.OnExit += a => _ent.OnHitItem -= PerformPickUp;

        open.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformOpen; };
        open.OnExit += a => _ent.OnHitItem -= PerformOpen;

        bridgeStep.OnEnter += a => {
            var step = _plan.FirstOrDefault();
            if (step != null)
            {

                _plan = _plan.Skip(1);
                var oldTarget = _target;
                _target = step.Item2;
                if (!_fsm.Feed(step.Item1))
                    _target = oldTarget;
            }
            else
            {
                _fsm.Feed(ActionEntity.Success);
            }
        };

        success.OnEnter += a => { Debug.Log("Success"); };
        success.OnUpdate += () => { _ent.Jump(); };

        StateConfigurer.Create(any)
            .SetTransition(ActionEntity.NextStep, bridgeStep)
            .SetTransition(ActionEntity.FailedStep, idle)
            .Done();

        StateConfigurer.Create(bridgeStep)
            .SetTransition(ActionEntity.Kill, kill)
            .SetTransition(ActionEntity.PickUp, pickup)
            .SetTransition(ActionEntity.Open, open)
            .SetTransition(ActionEntity.Success, success)
            .Done();

        _fsm = new EventFSM<ActionEntity>(idle, any);
    }

    public void ExecutePlan(List<Tuple<ActionEntity, Item>> plan)
    {
        _plan = plan;
        _fsm.Feed(ActionEntity.NextStep);
    }

    private void Update()
    {
        //Never forget
        _fsm.Update();
    }

    #region
    //public GState _state;
    //private List<GAction> actions = new List<GAction>();
    //private Queue<GAction> actionQueue;
    //private GPlanner planner = new GPlanner();

    //[SerializeField] private EnemyMovement _enemyMovement;
    //[SerializeField] private EnemyIdle _enemyIdle;
    //[SerializeField] private EnemyWeaponAttack _enemyAttackWeapon;
    //[SerializeField] private EnemRest _enemyRest;

    //public string _weapon = "none";
    //public float _distanceToPlayer = 5f;
    //public float _viewRadius = 5f;
    //public float _closeView = 1.5f;
    //MeshRenderer _Gun;
    //[SerializeField] private GameObject weaponObject;
    //private new void Awake()
    //{
    //    base.Awake();
    //    _Gun = weaponObject.GetComponent<MeshRenderer>();
    //    _Gun.enabled = false;

    //    _fsm = new FiniteStateMachine(_enemyIdle, StartCoroutine);
    //    _state = new GState();
    //    //Idle
    //    _fsm.AddTransition(StateTransitions.ToPursuit, _enemyIdle, _enemyMovement);
    //    _fsm.AddTransition(StateTransitions.ToAttack, _enemyIdle, _enemyAttackWeapon);
    //    _fsm.AddTransition(StateTransitions.ToGetWeapon, _enemyIdle, _enemyMovement);
    //    _fsm.AddTransition(StateTransitions.ToRest, _enemyIdle, _enemyRest);

    //    //Move
    //    _fsm.AddTransition(StateTransitions.ToIdle, _enemyMovement, _enemyIdle);

    //    //AttackWeapon
    //    _fsm.AddTransition(StateTransitions.ToIdle, _enemyAttackWeapon, _enemyIdle);
    //    //_fsm.AddTransition(StateTransitions.ToAttack, _enemyAttackWeapon, _enemyAttackWeapon);

    //    //Rest
    //    _fsm.AddTransition(StateTransitions.ToIdle, _enemyRest, _enemyIdle);
    //    _fsm.AddTransition(StateTransitions.ToPursuit, _enemyRest, _enemyMovement);




    //    _fsm.Active= true;
    //}


    //void Start()
    //{

    //    // **Estado inicial**

    //    // **Definir acciones**

    //    // Definir acciones
    //    //planngoap();
    //    PlanAndExecute();


    //}


    //private void Update()
    //{
    //    _fsm.Update();


    //}



    private void PlanAndExecute()
    {
        //var initialState = new GState { worldState = new WorldState { playerHP = 30, distance = 10, hasWeapon = false, weapon = "none" } };
        //var goalState = new GState { worldState = new WorldState { playerHP = 0, distance = 0, hasWeapon = true, weapon = "espada" } };
        //var actions = new List<GAction>{
        //                            new GAction("Buscar jugador", 3,
        //                                 s => s.worldState.distance > 0,
        //                                 s => { var ns = s.worldState.Clone(); ns.distance -= 5; return new GState { worldState = ns }; }),

        //                            new GAction("Ataque con arma", 5,
        //                                 s => s.worldState.weapon == "espada" && s.worldState.distance < 3,
        //                                 s => { var ns = s.worldState.Clone(); ns.playerHP -= 10; return new GState { worldState = ns }; }),
        //                            new GAction("Obtener arma",1,
        //                            s => s.worldState.weapon == "none",
        //                            s => { var ns = s.worldState.Clone(); ns.weapon = "espada"; return new GState { worldState = ns }; }),
        //                            new GAction("Descansar", 15,
        //                                s => s.worldState.playerHP < 50,
        //                                s => { var ns = s.worldState.Clone(); ns.playerHP += 20; return new GState { worldState = ns }; }),
        //                                  };


        //var plan = Goap.Execute(initialState, goalState,
        //    s => s.worldState.playerHP >= goalState.worldState.playerHP
        //         && s.worldState.distance <= goalState.worldState.distance
        //         && s.worldState.weapon == goalState.worldState.weapon,
        //    s => Math.Abs(goalState.worldState.playerHP - s.worldState.playerHP) + Math.Abs(goalState.worldState.distance - s.worldState.distance),
        //    actions);

        //var from = new GState();
        //   from.state["isPlayerInSight"] = IsPlayerInSight();
        //   from.state["Weapon"] = GetWeapon();
        //   from.state["DistanciaPlayer"] = GetDistanceToPlayer();
        //   from.state["Fatiga"] = GetFatigue();
        //   from.state["isPlayerAlive"] = true;

        //   var to = new GState();
        //   to.state["isPlayerAlive"] = false;

        //  var planner = new GPlanner();
          //planner.OnPlanCompleted += OnPlanCompleted;
          //planner.OnCantPlan += OnCantPlan;

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


    //private void OnPlanCompleted(IEnumerable<GAction> plan)
    //{
    //    _fsm = GPlanner.ConfigureFSM(plan, StartCoroutine);
    //    _fsm.Active = true;

    //}

 

    private void OnCantPlan()
    {
        //TODO: debuggeamos para ver por qué no pudo planear y encontrar como hacer para que no pase nunca mas
      


    }
    #endregion
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.DrawWireSphere(transform.position, _closeView);

       
    }

    
}

