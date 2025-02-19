using FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GPlanner
{
    public event Action<IEnumerable<GAction>> OnPlanCompleted;
    public event Action OnCantPlan;

    private const int _WATCHDOG_MAX = 200;

    private int _watchdog;


    public void Run(GState from, GState to, IEnumerable<GAction> actions,
                    Func<IEnumerator, Coroutine> startCoroutine)
    {
        _watchdog = _WATCHDOG_MAX;

        var astar = new AStar<GState>();
        astar.OnPathCompleted += OnPathCompleted;
        astar.OnCantCalculate += OnCantCalculate;

        var astarEnumerator = astar.Run(from,
                                        state => Satisfies(state, to),
                                        node => Explode(node, actions, ref _watchdog),
                                        state => GetHeuristic(state, to));

        startCoroutine(astarEnumerator);
    }

    public static FiniteStateMachine ConfigureFSM(IEnumerable<GAction> plan, Func<IEnumerator, Coroutine> startCoroutine)
    {
        var prevState = plan.First().linkedState;

        var fsm = new FiniteStateMachine(prevState, startCoroutine);

        foreach (var action in plan.Skip(1))
        {
            if (prevState == action.linkedState) continue;
            fsm.AddTransition("On" + action.linkedState.Name, prevState, action.linkedState);

            prevState = action.linkedState;
        }

        return fsm;
    }

    private void OnPathCompleted(IEnumerable<GState> sequence)
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

    private static float GetHeuristic(GState from, GState goal) => goal.state.Count(kv => !kv.In(from.state));
    private static bool Satisfies(GState state, GState to) => to.state.All(kv => kv.In(state.state));

    private static IEnumerable<WeightedNode<GState>> Explode(GState node, IEnumerable<GAction> actions,
                                                                ref int watchdog)
    {
        if (watchdog == 0) return Enumerable.Empty<WeightedNode<GState>>();
        watchdog--;

        return actions.Where(action => action.preconditions.All(kv => kv.In(node.state)))
                      .Aggregate(new List<WeightedNode<GState>>(), (possibleList, action) =>
                      {
                          var newState = new GState(node);
                          newState.state.UpdateWith(action.effects);
                          newState.generatingAction = action;
                          newState.step = node.step + 1;

                          possibleList.Add(new WeightedNode<GState>(newState, action.Cost));
                          return possibleList;
                      });
    }

    private void Check(Dictionary<string, bool> state, ItemType type)
    {

        var items = Navigation.instance.AllItems();
        var inventories = Navigation.instance.AllInventories();
        var floorItems = items.Except(inventories);//devuelve una coleccion como la primera pero removiendo los que estan en la segunda
        var item = floorItems.FirstOrDefault(x => x.type == type);
        var here = transform.position;
        state["accessible" + type.ToString()] = item != null && Navigation.instance.Reachable(here, item.transform.position, _debugRayList);

        var inv = inventories.Any(x => x.type == type);
        state["otherHas" + type.ToString()] = inv;

        state["dead" + type.ToString()] = false;
    }

    private IEnumerator Plan()
    {
        yield return new WaitForSeconds(0.2f);
        //si no se usan objetos modulares se puede eliminar
        var observedState = new Dictionary<string, bool>();

        var nav = Navigation.instance;//Consigo los items
        var floorItems = nav.AllItems();
        var inventory = nav.AllInventories();
        var everything = nav.AllItems().Union(nav.AllInventories());// .Union() une 2 colecciones sin agregar duplicados(eso incluye duplicados en la misma coleccion)

        //Chequeo los booleanos para cada Item, generando mi modelo de mundo (mi diccionario de bools) en ObservedState
        Check(observedState, ItemType.Key);
        Check(observedState, ItemType.Entity);
        Check(observedState, ItemType.Mace);
        Check(observedState, ItemType.PastaFrola);
        Check(observedState, ItemType.Door);
        //si no se usan objetos modulares se puede eliminar


        //estado inicial
        GState initial = new GState();
        initial.worldState = new WorldState()
        {
            //estos valores se pueden pasar a mano pero deben coordinar con el estado del mundo actual , lo ideal es que se consigan el estados de todas las variables proceduralmente pero no es obligatorio
            playerHP = 88,
            //values = new Dictionary<string, bool>()//eliminr 
        };

        //initial.worldState.values = observedState; //le asigno los valores actuales, conseguidos antes
        //initial.worldState.values["doorOpen"] = false; //agrego el bool "doorOpen"


        var actions = CreatePossibleActionsList();


        #region opcional
        //foreach (var item in initial.worldState.values)
        //{
        //    Debug.Log(item.Key + " ---> " + item.Value);
        //}
        #endregion

        //esto es opcional no es necesario buscar un nodo que cumpla perfectamente las condiciones
        GState goal = new GState();
        //goal.values["has" + ItemType.Key.ToString()] = true;
        goal.worldState.values["has" + ItemType.PastaFrola.ToString()] = true;
        //goal.values["has"+ ItemType.Mace.ToString()] = true;
        //goal.values["dead" + ItemType.Entity.ToString()] = true;}

        //crear la heuristica personalizada
        Func<GState, float> heuristc = (curr) =>
        {
            int count = 0;
            string key = "has" + ItemType.PastaFrola.ToString();
            //if (!curr.worldState.values.ContainsKey(key) || !curr.worldState.values[key])
                count++;
            if (curr.worldState.playerHP <= 45)
                count++;
            return count;
        };
        //esto es el reemplazo de goal donde se pide que cumpla con las condiciones pasadas
        Func<GState, bool> objectice = (curr) =>
        {
            string key = "has" + ItemType.PastaFrola.ToString();
            return curr.worldState.values.ContainsKey(key) && curr.worldState.values["has" + ItemType.PastaFrola.ToString()]
                   && curr.worldState.playerHP > 45;
        };



        #region opcional
        //var actDict = new Dictionary<string, ActionEntity>() {
        //      { "Kill"  , ActionEntity.Kill }
        //    , { "Pickup", ActionEntity.PickUp }
        //    , { "Open"  , ActionEntity.Open }
        //};

        #endregion

        var plan = Goap.Execute(initial, null, objectice, heuristc, actions);

        if (plan == null)
            Debug.Log("Couldn't plan");
        else
        {

            //hacer un debug for each de todas las acciones para ver si se pudo completar el plan
            GetComponent<Guy>().ExecutePlan(
                plan
                .Select(a =>
                {
                    Item i2 = everything.FirstOrDefault(i => i.type == a.item);
                    if (actDict.ContainsKey(a.Name) && i2 != null)
                    {
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


    private List<GAction> CreatePossibleActionsList()
    {
        return new List<GAction>()
        {
              new GAction("Kill",1f)
                //.SetCost(1f)
                .SetItem(ItemType.Entity) //si no uso items esto lo puedo quitar
                //no usar mas de un pre con las lambdas (.Pre(x=>x)..Pre(x=>x).) se van a pisar
                .Pre((gS)=>
                {
                    //Esto es un ejemplo exajerado, podriamos tener una convinacion, y en las precondiciones tener un diccionbario, como antes y aca chequear

                    //agrego las precondiciones en base a las variables de gs.worldstate
                    return gS.worldState.values.ContainsKey("dead"+ ItemType.Entity.ToString()) &&
                           gS.worldState.values.ContainsKey("accessible"+ ItemType.Entity.ToString()) &&
                           gS.worldState.values.ContainsKey("has"+ ItemType.Mace.ToString()) &&
                           //!gS.worldState.values["dead"+ ItemType.Entity.ToString()] &&
                           //gS.worldState.values["accessible"+ ItemType.Entity.ToString()] &&
                           //gS.worldState.values["has"+ ItemType.Mace.ToString()] &&


                           //lo pedido es completarlo de la siguiente manera sin depender del diccionario de values (excepto cuando se usen los items)
                           gS.worldState.playerHP > 50;
                })
                //Ejemplo de setteo de Effect
                .Effect((gS) =>
                    {
                        gS.worldState.values["dead"+ ItemType.Entity.ToString()] = true;
                        gS.worldState.values["accessible"+ ItemType.Key.ToString()] = true;
                        return gS;
                    }
                )

            , new GAction("Loot",1f)
                //.SetCost(1f)
                .SetItem(ItemType.Key)
                .Pre("otherHas"+ ItemType.Key.ToString(), true)
                .Pre("dead"+ ItemType.Entity.ToString(), true)

                .Effect("accessible"+ ItemType.Key.ToString(), true)
                .Effect("otherHas"+ ItemType.Key.ToString(), false)

            , new GAction("Pickup",2f)
                //.SetCost(2f)
                .SetItem(ItemType.Mace)
                .Pre("dead"+ ItemType.Mace.ToString(), false)
                .Pre("otherHas"+ ItemType.Mace.ToString(), false)
                .Pre("accessible"+ ItemType.Mace.ToString(), true)

                .Effect("accessible"+ ItemType.Mace.ToString(), false)
                .Effect("has"+ ItemType.Mace.ToString(), true)

            , new GAction("Pickup",2f)
                //.SetCost(2f)
                .SetItem(ItemType.Key)
//                .Pre("deadKey", false)
//                .Pre("otherHasKey", false)
                .Pre("accessible"+ ItemType.Key.ToString(), true)

                .Effect("accessible"+ ItemType.Key.ToString(), false)
                .Effect("has"+ ItemType.Key.ToString(), true)

            , new GAction("Pickup",5f)
                //.SetCost(5f)					//La frola es prioritaria!
                .SetItem(ItemType.PastaFrola)
                .Pre("dead"+ ItemType.PastaFrola.ToString(), false)
                .Pre("otherHas"+ ItemType.PastaFrola.ToString(), false)
                .Pre("accessible"+ ItemType.PastaFrola.ToString(), true)
                //.Pre("hasKey",true)

                .Effect("accessible"+ ItemType.PastaFrola.ToString(), false)
                .Effect("has"+ ItemType.PastaFrola.ToString(), true)

            , new GAction("Open",3f)
                //.SetCost(3f)
                .SetItem(ItemType.Door)
                .Pre("dead"+ ItemType.Door.ToString(), false)
                .Pre("has"+ ItemType.Key.ToString(), true)

                .Effect("has"+ ItemType.Key.ToString(), false)
                .Effect("doorOpen", true)
                .Effect("dead"+ ItemType.Key.ToString(), true)
                .Effect("accessible"+ ItemType.PastaFrola.ToString(), true)

                , new GAction("Kill",20f)
                //.SetCost(20f)
                .SetItem(ItemType.Door)
                .Pre("dead"+ ItemType.Door.ToString(), false)
                .Pre("has"+ ItemType.Mace.ToString(), true)

                .Effect("doorOpen", true)
                .Effect("has"+ ItemType.Mace.ToString(), false)
                .Effect("dead"+ ItemType.Mace.ToString(), true)
                .Effect("dead"+ ItemType.Door.ToString(), true)
                .Effect("accessible"+ ItemType.PastaFrola.ToString(), true)


        };
    }

}