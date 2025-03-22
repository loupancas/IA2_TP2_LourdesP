using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

//A ESTRELLA VISTO EN CLASE
public class AStar<T>
{
    public event Action<IEnumerable<T>> OnPathCompleted;
    //public event Action<List<T>> OnPathCompleted;
    public event System.Action OnCantCalculate;
    public float maxFrameTime = 0.016f; // Tiempo máximo por frame (60 FPS)

    public IEnumerator Run(T start, Func<T, bool> isGoal, Func<T, IEnumerable<WeightedNode<T>>> explode, Func<T, float> getHeuristic)
    {
        var queue = new PriorityQueue<T>();
        var distances = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        var visited = new HashSet<T>();

        distances[start] = 0;
        queue.Enqueue(new WeightedNode<T>(start, 0));
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        while (!queue.IsEmpty)
        {
            var dequeued = queue.Dequeue();
            visited.Add(dequeued.Element);

            if (isGoal(dequeued.Element))
            {
                var path = CommonUtils.CreatePath(parents, dequeued.Element);
                OnPathCompleted?.Invoke(path);
                yield break;
            }

            var toEnqueue = explode(dequeued.Element);

            foreach (var transition in toEnqueue)
            {
                var neighbour = transition.Element;
                var neighbourToDequeuedDistance = transition.Weight;

                var startToNeighbourDistance = distances.ContainsKey(neighbour) ? distances[neighbour] : float.MaxValue;
                var startToDequeuedDistance = distances[dequeued.Element];

                var newDistance = startToDequeuedDistance + neighbourToDequeuedDistance;

                if (!visited.Contains(neighbour) && startToNeighbourDistance > newDistance)
                {
                    distances[neighbour] = newDistance;
                    parents[neighbour] = dequeued.Element;
                    queue.Enqueue(new WeightedNode<T>(neighbour, newDistance + getHeuristic(neighbour)));
                }

                if (stopwatch.ElapsedMilliseconds >= maxFrameTime * 1000)
                {
                    UnityEngine.Debug.Log("Tiempo del frame excedido, cediendo control del hilo.");
                    yield return null;
                    stopwatch.Restart();
                }
            }
        }

        OnCantCalculate?.Invoke();
    }
}




public class  AEstrella<T> where T : class
{

    public class WeightedNode
    {
        public T Element { get; }
        public float Weight { get; }

        public WeightedNode(T element, float weight)
        {
            Element = element;
            Weight = weight;
        }
    }
    public static IEnumerable<T> Go(
        T from,
        T to,
        Func<T, T, float> h,                // Current, Goal -> Heuristic cost
        Func<T, bool> satisfies,            // Current -> Satisfies
        Func<T, IEnumerable<WeightedNode>> expand // Current -> (Endpoint, Cost)[]
    )
    {
        var open = new List<T> { from };
        var closed = new HashSet<T>();
        var gs = new Dictionary<T, float> { [from] = 0 };
        var fs = new Dictionary<T, float> { [from] = h(from, to) };
        var previous = new Dictionary<T, T>();

        while (open.Count > 0)
        {
            var candidate = open.OrderBy(x => fs[x]).First();
            if (satisfies(candidate))
            {
                return GeneratePath(candidate, previous);
            }

            open.Remove(candidate);
            closed.Add(candidate);

            var neighbors = expand(candidate);
            if (neighbors == null || !neighbors.Any())
                continue;

            var gCandidate = gs[candidate];

            foreach (var neighbor in neighbors)
            {
                if (closed.Contains(neighbor.Element))
                    continue;

                var gNeighbor = gCandidate + neighbor.Weight;
                if (!open.Contains(neighbor.Element))
                {
                    open.Add(neighbor.Element);
                }

                if (gNeighbor >= gs.GetValueOrDefault(neighbor.Element, float.MaxValue))
                    continue;

                previous[neighbor.Element] = candidate;
                gs[neighbor.Element] = gNeighbor;
                fs[neighbor.Element] = gNeighbor + h(neighbor.Element, to);
            }
        }

        return null;

    }
    private static IEnumerable<T> GeneratePath(T current, Dictionary<T, T> previous)
    {
        var path = new Stack<T>();
        while (current != null)
        {
            path.Push(current);
            previous.TryGetValue(current, out current);
        }
        return path;
    }
}