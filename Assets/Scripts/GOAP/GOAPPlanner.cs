using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;
using U = Utility;

public class GOAPPlanner:MonoBehaviour
{
    private readonly List<Tuple<Vector3, Vector3>> _debugRayList = new List<Tuple<Vector3, Vector3>>();
    public event Action<IEnumerable<GOAPAction>> OnPlanCompleted;
    public event Action OnCantPlan;

    private const int _WATCHDOG_MAX = 200;

    private int _watchdog;

    private void Start()
    {
        StartCoroutine(Plan());
    }

    private void Check(Dictionary<string, object> state, ItemType type)
    {

        var items = Navigation.instance.AllItems();
        var inventories = Navigation.instance.AllInventories();
        var floorItems = items.Except(inventories);//devuelve una coleccion como la primera pero removiendo los que estan en la segunda
        var item = floorItems.FirstOrDefault(x => x.type == type);
        var here = transform.position;
        state["accessible" + type.ToString()] = item != null && Navigation.instance.Reachable(here, item.transform.position, _debugRayList);

        var inv = inventories.Any(x => x.type == type);
        state["otherHas" + type.ToString()] = inv;

        if (inv == true)
        {
            state["has" + type.ToString()] = false;
        }

        state["dead" + type.ToString()] = false;



    }

    private IEnumerator Plan()
    {
        yield return new WaitForSeconds(0.3f);

        var observedState = new Dictionary<string, object>();

        var nav = Navigation.instance;//Consigo los items
        var floorItems = nav.AllItems();
        var inventory = nav.AllInventories();
        var everything = nav.AllItems().Union(nav.AllInventories());// .Union() une 2 colecciones sin agregar duplicados(eso incluye duplicados en la misma coleccion)

        //Chequeo los booleanos para cada Item, generando mi modelo de mundo (mi diccionario de bools) en ObservedState
        Check(observedState, ItemType.Key);
        Check(observedState, ItemType.Mace);
        Check(observedState, ItemType.PastaFrola);
        Check(observedState, ItemType.Door);
        Check(observedState, ItemType.Entity);


        //estado inicial
        GOAPState initial = new GOAPState();
        initial.worldState = new WorldState()
        {
            values = new Dictionary<string, object>(),

        };


        initial.worldState.values = observedState; //le asigno los valores actuales, conseguidos antes

        initial.worldState.values["money"] = 10;
        initial.worldState.values["entity"] = "vivo";
        initial.worldState.values["doorOpen"] = false;
        initial.worldState.values["hasKey"] = 0;
        initial.worldState.values["hasMace"] = false;
        initial.worldState.values["hasPastaFrola"] = false;


        #region opcional
        //foreach (var item in initial.worldState.values)
        //{
        //    Debug.Log(item.Key + " ---> " + item.Value);
        //}
        #endregion

        //esto es opcional no es necesario buscar un nodo que cumpla perfectamente las condiciones
        GOAPState goal = new GOAPState();
        //goal.values["has" + ItemType.Key.ToString()] = true;
        goal.worldState.values["has" + ItemType.PastaFrola.ToString()] = true;
        //goal.worldState.values["hasPastaFrola"] = true;
        //goal.worldState.door=false;

        //goal.worldState.values["has" + ItemType.Key.ToString()] = false;
        //goal.values["has"+ ItemType.Mace.ToString()] = true;
        //goal.worldState.values["entity" + ItemType.Entity.ToString()] = "vivo";
        #region heuristica
        //crear la heuristica personalizada
        Func<GOAPState, float> heuristc = (curr) =>
        {
            int count = 0; //contador
                           //string key = "has" + ItemType.PastaFrola.ToString(); //key 
                           //if (!curr.worldState.values.ContainsKey(key) || !curr.worldState.values[key])
            if (((string)curr.worldState.values["entity"] == "muerto"))
                count++;
            //if ((int)curr.worldState.values["money"] <= 5)
            //    count++;
            return count;
        };
        //esto es el reemplazo de goal donde se pide que cumpla con las condiciones pasadas
        Func<GOAPState, bool> objectice = (curr) =>
        {
            string key = "has" + ItemType.PastaFrola.ToString();
            //return curr.worldState.values.ContainsKey(key) && curr.worldState.values["has" + ItemType.PastaFrola.ToString()]
            //       && curr.worldState.playerHP > 45 ;
            return (bool)curr.worldState.values["doorOpen"] == true && curr.worldState.values.ContainsKey(key) && (bool)curr.worldState.values["has" + ItemType.PastaFrola.ToString()] == true;
            //&& (int)curr.worldState.values["money"] > 5;
        };
        #endregion


        #region opcional
        var actDict = new Dictionary<string, ActionEntity>() {
			  //{ "Kill"	, ActionEntity.Kill }
			 { "Pickup", ActionEntity.PickUp }
            , { "PickupM", ActionEntity.PickUpM }
            , { "Open"  , ActionEntity.Open }
            , { "Loot", ActionEntity.Loot }
            , { "Breaking", ActionEntity.Breaking }
            , { "Sobornar", ActionEntity.Sobornar }
                        , { "Matar", ActionEntity.Matar }
        };

        #endregion
        var actions = CreatePossibleActionsList();

        //var plan = Goap.Execute(initial, null, objectice, heuristc, actions);

        if (plan == null)
            Debug.Log("Couldn't plan");

        else
        {
            Debug.Log("Plan found:");

            GetComponent<Guy>().ExecutePlan(
                plan
                .Select(a =>
                {
                    Item i2 = everything.FirstOrDefault(i => i.type == a.item);
                    Debug.Log(i2);
                    if (actDict.ContainsKey(a.Name) && i2 != null)
                    {
                        Debug.Log(a.Name);
                        Debug.Log(i2.type);
                        return Tuple.Create(actDict[a.Name], i2);
                    }
                    else
                    {
                        return null;
                    }
                }).Where(a => a != null)
                .ToList()
            );


        }

    }

    private List<GOAPAction> CreatePossibleActionsList()
    {
        return new List<GOAPAction>()
        {

            new GOAPAction("Open")
                .SetCost(2f)
                .SetItem(ItemType.Door)
                .Pre("has"+ ItemType.Key.ToString(),1)
                .Pre("dead"+ ItemType.Door.ToString(),false)
                .Pre("accessible"+ ItemType.Door.ToString(),true)
                .Pre("doorOpen",false)
                .Effect("has"+ ItemType.Key.ToString(),0f)
                .Effect("doorOpen", true)
                .Effect("dead"+ ItemType.Door.ToString(),true)
                .Effect("accessible" + ItemType.PastaFrola.ToString(),true)

                ,new GOAPAction("Breaking")
                .SetCost(4f)
                .SetItem(ItemType.Door)
                .Pre("has"+ ItemType.Mace.ToString(),true)
                .Pre("dead"+ ItemType.Door.ToString(),false)
                .Pre("doorOpen",false)
                .Effect("has"+ ItemType.Mace.ToString(),false)
                .Effect("doorOpen",true)
                .Effect("dead"+ ItemType.Door.ToString(),true)

                .Effect("accessible"+ItemType.PastaFrola.ToString(),true)

                 , new GOAPAction("Pickup")
                .SetCost(5f)					//La frola es prioritaria!
                .SetItem(ItemType.PastaFrola)
                .Pre("doorOpen", true)
                .Pre("dead"+ ItemType.Door.ToString(),true)
                .Pre("accessible"+ItemType.PastaFrola.ToString(), true)


              .Effect("accessible" + ItemType.PastaFrola.ToString(), false)
                .Effect("has"+ItemType.PastaFrola.ToString(), true)

                ,new GOAPAction("Loot")
                .SetCost(2f)
                .SetItem(ItemType.Key)
                .Pre("otherHas"+ ItemType.Key.ToString(), true)
                .Pre("entity", "muerto")
                .Effect("has"+ ItemType.Key.ToString(),1)
                .Effect("otherHas"+ ItemType.Key.ToString(),false)

                ,new GOAPAction("Matar")
                .SetCost(4f)
                .SetItem(ItemType.Entity)
                .Pre("entity", "vivo")
                .Pre("accessible"+ ItemType.Entity.ToString(), true)
                .Pre("has"+ ItemType.Mace.ToString(), true)
                .Pre("has"+ ItemType.Key.ToString(), 0)
                .Effect("accessible"+ ItemType.Entity.ToString(), false)
                .Effect("has"+ ItemType.Mace.ToString(), false)
                .Effect("accesible"+ ItemType.Key.ToString(), true)
                .Effect("entity", "muerto")

                ,new GOAPAction("Sobornar")
                .SetCost(2f)
                .SetItem(ItemType.Entity)
                .Pre("money", 10)
                .Pre("has"+ ItemType.Key.ToString(), 0)
                .Pre("hasMace", false)
                .Pre("accessible"+ ItemType.Entity.ToString(), true)
                .Pre("otherHas"+ ItemType.Key.ToString(), true)
                .Pre("entity", "vivo")
                .Effect("money", 0)
                .Effect("otherHas"+ ItemType.Key.ToString(), false)
                .Effect("has"+ ItemType.Key.ToString(), 1)
                .Effect("accessible"+ ItemType.Entity.ToString(), false)
                .Effect("accessible"+ItemType.Door.ToString(), true)


            , new GOAPAction("PickupM")
                .SetCost(1f)
                .SetItem(ItemType.Mace)
                .Pre("deadMace", false)
                .Pre("otherHasMace", false)
                .Pre("accessibleMace", true)
                .Pre("hasMace", false)
                 .Pre("doorOpen", false)
                 .Pre("dead" + ItemType.Door.ToString(), false)
                .Pre("has" + ItemType.Key.ToString(), 0)
               .Effect("accessible" + ItemType.Mace.ToString(), false)
               .Effect("has" + ItemType.Mace.ToString(), true)


     };

    }


    public void Run(GOAPState from, GOAPState to, IEnumerable<GOAPAction> actions,
                    Func<IEnumerator, Coroutine> startCoroutine)
    {
        _watchdog = _WATCHDOG_MAX;

        var astar = new AStar<GOAPState>();
        astar.OnPathCompleted += OnPathCompleted;
        astar.OnCantCalculate += OnCantCalculate;

        var astarEnumerator = astar.Run(from,
                                        state => Satisfies(state, to),
                                        node => Explode(node, actions, ref _watchdog),
                                        state => GetHeuristic(state, to));

        startCoroutine(astarEnumerator);
    }

    //public static FiniteStateMachine ConfigureFSM(IEnumerable<GOAPAction> plan, Func<IEnumerator, Coroutine> startCoroutine)
    //{
    //    var prevState = plan.First().linkedState;

    //    var fsm = new FiniteStateMachine(prevState, startCoroutine);

    //    foreach (var action in plan.Skip(1))
    //    {
    //        if (prevState == action.linkedState) continue;
    //        fsm.AddTransition("On" + action.linkedState.Name, prevState, action.linkedState);

    //        prevState = action.linkedState;
    //    }

    //    return fsm;
    //}

    private void OnPathCompleted(IEnumerable<GOAPState> sequence)
    {
        foreach (var act in sequence.Skip(1))
        {
            Debug.Log(act);
        }

        Debug.Log("WATCHDOG " + _watchdog);

        var plan = sequence.Skip(1).Select(x => x.generatingAction);

        OnPlanCompleted?.Invoke(plan);
    }

    private void OnCantCalculate()
    {
        OnCantPlan?.Invoke();
    }

    private static float GetHeuristic(GOAPState from, GOAPState goal) => goal.values.Count(kv => !kv.In(from.values));
    private static bool Satisfies(GOAPState state, GOAPState to) => to.values.All(kv => kv.In(state.values));

    private static IEnumerable<WeightedNode<GOAPState>> Explode(GOAPState node, IEnumerable<GOAPAction> actions,
                                                                ref int watchdog)
    {
        if (watchdog == 0) return Enumerable.Empty<WeightedNode<GOAPState>>();
        watchdog--;

        return actions.Where(action => action.preconditions.All(kv => node.worldState.values.ContainsKey(kv.Key)))
                      .Aggregate(new List<WeightedNode<GOAPState>>(), (possibleList, action) => {
                          var newState = new GOAPState(node);
                          //newState.values.UpdateWith(action.effects);
                          newState.generatingAction = action;
                          newState.step = node.step + 1;

                          possibleList.Add(new WeightedNode<GOAPState>(newState, action.cost));
                          return possibleList;
                      });
    }
}
