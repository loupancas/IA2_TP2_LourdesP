using FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}