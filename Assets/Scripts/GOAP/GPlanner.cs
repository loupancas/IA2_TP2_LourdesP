using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPlanner : MonoBehaviour
{
    public event Action<IEnumerable<GAction>> OnPlanCompleted;
    public event Action OnCantPlan;


    private readonly List<Tuple<Vector3, Vector3>> _debugRayList = new List<Tuple<Vector3, Vector3>>();

    private void Start()
    {
        StartCoroutine(Plan());
    }



    //private static float GetHeuristic(GState from, GState goal) => goal.state.Count(kv => !kv.In(from.state));
    //private static bool Satisfies(GState state, GState to) => to.state.All(kv => kv.In(state.state));



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
        //Check(observedState, ItemType.Key);
        //Check(observedState, ItemType.NewEntity);
        //Check(observedState, ItemType.Cuchillo);
        //Check(observedState, ItemType.Espada);
        //Check(observedState, ItemType.Door);
        //si no se usan objetos modulares se puede eliminar


        //estado inicial
        GState initial = new GState();
        initial.worldState = new WorldState()
        {
            //estos valores se pueden pasar a mano pero deben coordinar con el estado del mundo actual , lo ideal es que se consigan el estados de todas las variables proceduralmente pero no es obligatorio
            playerHP = 88,
            weapon = "none",
            hasWeapon = false,
            distance = 10,
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
        goal.worldState = new WorldState()
        {
            playerHP = 0,
            weapon = "espada",
            hasWeapon = true,
            distance = 2,
            //values = new Dictionary<string, bool>()
        };
        //goal.values["has" + ItemType.Key.ToString()] = true;
        //goal.worldState.values["has" + ItemType.Cuchillo.ToString()] = true;
        //goal.values["has"+ ItemType.Mace.ToString()] = true;
        //goal.values["dead" + ItemType.Entity.ToString()] = true;}

        //crear la heuristica personalizada
        Func<GState, float> heuristc = (curr) =>
        {
            int count = 0;
            string key = "has" + ItemType.Cuchillo.ToString();
            //if (!curr.worldState.values.ContainsKey(key) || !curr.worldState.values[key])
                count++;
            if (curr.worldState.playerHP <= 45)
                count++;
            return count;
        };
        //esto es el reemplazo de goal donde se pide que cumpla con las condiciones pasadas
        Func<GState, bool> objectice = (curr) =>
        {
            string key = "has" + ItemType.Cuchillo.ToString();
            return curr != null;



            //curr.worldState.values.ContainsKey(key) && curr.worldState.values["has" + ItemType.Cuchillo.ToString()]
            //       && curr.worldState.playerHP > 45;
        };



        #region opcional
        var actDict = new Dictionary<string, ActionEntity>() {
              { "Kill"  , ActionEntity.Kill }
            , { "Pickup", ActionEntity.PickUp }
            , { "Open"  , ActionEntity.Open }
        };

        #endregion

        var plan = Goap.Execute(initial, null, objectice, heuristc, actions);

        if (plan == null)
            Debug.Log("Couldn't plan");
        else
        {

            //hacer un debug for each de todas las acciones para ver si se pudo completar el plan
            GetComponent<GAgent>().ExecutePlan(
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
                          //new GAction("Kill",1f, s => s.worldState.playerHP < 50,
                          //s => { var ns = s.worldState.Clone(); ns.playerHP += 20; return new GState { worldState = ns }; })
                          // ,new GAction("Atacar", 3,
                          //s => s.worldState.weapon == "espada" && s.worldState.distance < 3,
                          //s => { var ns = s.worldState.Clone(); ns.playerHP -= 10; return new GState { worldState = ns }; })
                          //   ,new GAction("Tomar Espada", 1,
                          //s => s.worldState.weapon == "none",
                          //s => { var ns = s.worldState.Clone(); ns.weapon = "espada"; return new GState { worldState = ns }; })

                           new GAction("TomarCuchillo")
                            .SetCosts(1f)
                            .SetItem(ItemType.Cuchillo)
                            .Pre("Weapon"+ ItemType.Cuchillo.ToString(), "noWeapon")
                            .Pre("accessible"+ ItemType.Cuchillo.ToString(), true)

                            .Effect("Weapon"+ ItemType.Cuchillo.ToString(), "Cuchillo")

                            // , new GAction("Pickup")
                            //.SetCosts(2f)
                            //.SetItem(ItemType.Key)
                            //.Pre("Key", false)
                            //.Pre("otherHasKey", true)
                            //.Pre("accessible"+ ItemType.Key.ToString(), false)

                            //.Effect("accessible"+ ItemType.Key.ToString(), true)
                            //.Effect("has"+ ItemType.Key.ToString(), true)
                            
                            //, new GAction("Kill")
                            //.SetCosts(20f)
                            //.SetItem(ItemType.NewEntity)
                            //.Pre("dead"+ ItemType.NewEntity.ToString(), false)

                            //.Effect("dead"+ ItemType.NewEntity.ToString(), true)
                         
                            //,new GAction("Open")
                            //.SetCosts(3f)
                            //.SetItem(ItemType.Door)

            //            , new GAction("Pickup",2f)
            //                //.SetCost(2f)
            //                .SetItem(ItemType.Mace)
            //                .Pre("dead"+ ItemType.Mace.ToString(), false)
            //                .Pre("otherHas"+ ItemType.Mace.ToString(), false)
            //                .Pre("accessible"+ ItemType.Mace.ToString(), true)

            //                .Effect("accessible"+ ItemType.Mace.ToString(), false)
            //                .Effect("has"+ ItemType.Mace.ToString(), true)

            //            , new GAction("Pickup",2f)
            //                //.SetCost(2f)
            //                .SetItem(ItemType.Key)
            ////                .Pre("deadKey", false)
            ////                .Pre("otherHasKey", false)
            //                .Pre("accessible"+ ItemType.Key.ToString(), true)

            //                .Effect("accessible"+ ItemType.Key.ToString(), false)
            //                .Effect("has"+ ItemType.Key.ToString(), true)

            //            , new GAction("Pickup",5f)
            //                //.SetCost(5f)					//La frola es prioritaria!
            //                .SetItem(ItemType.PastaFrola)
            //                .Pre("dead"+ ItemType.PastaFrola.ToString(), false)
            //                .Pre("otherHas"+ ItemType.PastaFrola.ToString(), false)
            //                .Pre("accessible"+ ItemType.PastaFrola.ToString(), true)
            //                //.Pre("hasKey",true)

            //                .Effect("accessible"+ ItemType.PastaFrola.ToString(), false)
            //                .Effect("has"+ ItemType.PastaFrola.ToString(), true)

            //            , new GAction("Open",3f)
            //                //.SetCost(3f)
            //                .SetItem(ItemType.Door)
            //                .Pre("dead"+ ItemType.Door.ToString(), false)
            //                .Pre("has"+ ItemType.Key.ToString(), true)

            //                .Effect("has"+ ItemType.Key.ToString(), false)
            //                .Effect("doorOpen", true)
            //                .Effect("dead"+ ItemType.Key.ToString(), true)
            //                .Effect("accessible"+ ItemType.PastaFrola.ToString(), true)

            //                , new GAction("Kill",20f)
            //                //.SetCost(20f)
            //                .SetItem(ItemType.Door)
            //                .Pre("dead"+ ItemType.Door.ToString(), false)
            //                .Pre("has"+ ItemType.Mace.ToString(), true)

            //                .Effect("doorOpen", true)
            //                .Effect("has"+ ItemType.Mace.ToString(), false)
            //                .Effect("dead"+ ItemType.Mace.ToString(), true)
            //                .Effect("dead"+ ItemType.Door.ToString(), true)
            //                .Effect("accessible"+ ItemType.PastaFrola.ToString(), true)


        };
    }
}