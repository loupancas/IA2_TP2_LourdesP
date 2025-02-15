using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Planner
{
    public event Action<IEnumerable<ActionGoap>> OnPlanCompleted;
    public event System.Action OnCantPlan;

    private const int _WATCHDOG_MAX = 200; // Límite de iteraciones
    private const int _DEPTH_LIMIT = 20;   // Profundidad máxima para evitar loops largos

    private int _watchdog;

    public void Run(WorldState from, WorldState to, IEnumerable<ActionGoap> actions, Func<IEnumerator, Coroutine> startCoroutine)
    {
        _watchdog = _WATCHDOG_MAX;

        var astar = new AStar<WorldState>();
        astar.OnPathCompleted += OnPathCompleted;
        astar.OnCantCalculate += OnCantCalculate;

        var astarEnumerator = astar.Run(
            from,
            state => Satisfies(state, to),
            node => Explode(node, actions, ref _watchdog),
            state => GetHeuristic(state, to)
        );

        startCoroutine(astarEnumerator);
    }

    private void OnPathCompleted(List<WorldState> path)
    {
        if (path == null || path.Count == 0)
        {
            OnCantPlan?.Invoke();
            return;
        }

        List<ActionGoap> plan = new List<ActionGoap>();

        for (int i = 1; i < path.Count; i++)
        {
            var action = path[i - 1].GeneratingAction;
            if (action != null)
                plan.Add(action);
        }

        OnPlanCompleted?.Invoke(plan);
    }

    private void OnCantCalculate()
    {
        OnCantPlan?.Invoke();
    }

    private IEnumerable<Node<WorldState>> Explode(Node<WorldState> node, IEnumerable<ActionGoap> actions, ref int watchdog)
    {
        if (watchdog-- <= 0)
        {
            Debug.LogWarning("GOAP Watchdog activado: límite de iteraciones alcanzado");
            return Enumerable.Empty<Node<WorldState>>();
        }

        if (node.Depth > _DEPTH_LIMIT)
        {
            Debug.LogWarning("GOAP alcanzó profundidad máxima, evitando loops");
            return Enumerable.Empty<Node<WorldState>>();
        }

        List<Node<WorldState>> newNodes = new List<Node<WorldState>>();

        foreach (var action in actions)
        {
            if (action.CanExecute(node.Value))
            {
                WorldState newState = node.Value.Clone();
                action.ApplyEffect(newState);
                newState.GeneratingAction = action;

                // 🚨 Evitar estados repetidos 🚨
                if (HasLoop(node, newState))
                {
                    Debug.LogWarning($"GOAP evitó un loop con acción: {action.Name}");
                    continue;
                }

                newNodes.Add(new Node<WorldState>(newState, node, action.Cost));
            }
        }

        return newNodes;
    }

    private bool HasLoop(Node<WorldState> currentNode, WorldState newState)
    {
        Node<WorldState> node = currentNode;

        while (node != null)
        {
            if (node.Value.Equals(newState))
                return true;

            node = node.Parent;
        }

        return false;
    }

    private bool Satisfies(WorldState current, WorldState goal)
    {
        return goal.All(pair => current.Contains(pair.Key) && current.Get<object>(pair.Key).Equals(pair.Value));
    }

    private float GetHeuristic(WorldState current, WorldState goal)
    {
        return goal.Count(pair => !current.Contains(pair.Key) || !current.Get<object>(pair.Key).Equals(pair.Value));
    }
}
