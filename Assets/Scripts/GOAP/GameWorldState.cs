using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GameWorldState
{
    public float health = 100;
    public int gold = 0;
    public bool isAlive = true;
    public string weapon = "none";

    public GameWorldState Clone()
    {
        return (GameWorldState)this.MemberwiseClone();
    }
}

public class ActionGoap
{
    public string name;
    public Func<GameWorldState, bool> precondition;
    public Action<GameWorldState> effect;
    public float cost;
    public Vector3 targetPosition;

    public ActionGoap(string name, Func<GameWorldState, bool> precondition, Action<GameWorldState> effect, float cost, Vector3 targetPosition)
    {
        this.name = name;
        this.precondition = precondition;
        this.effect = effect;
        this.cost = cost;
        this.targetPosition = targetPosition;
    }
}

public class GOAPAgent : MonoBehaviour
{
    public List<ActionGoap> availableActions;
    public GameWorldState worldState = new GameWorldState();
    public NavMeshAgent agent;
    private Queue<ActionGoap> actionQueue;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        availableActions = new List<ActionGoap> {
            new ActionGoap("Pick Gold", w => w.gold < 10, w => w.gold += 5, 1, new Vector3(5, 0, 5)),
            new ActionGoap("Heal", w => w.health < 50, w => w.health += 30, 2, new Vector3(10, 0, 10)),
            new ActionGoap("Attack", w => w.weapon == "sword", w => w.isAlive = false, 3, new Vector3(15, 0, 15))
        };
        PlanAndExecute();
    }

    void PlanAndExecute()
    {
        var plan = Planner.Plan(worldState.Clone(), availableActions);
        if (plan != null)
        {
            actionQueue = new Queue<ActionGoap>(plan);
            ExecuteNextAction();
        }
    }

    void ExecuteNextAction()
    {
        if (actionQueue.Count == 0) return;
        ActionGoap action = actionQueue.Dequeue();
        agent.SetDestination(action.targetPosition);
        StartCoroutine(WaitForArrival(() => {
            action.effect(worldState);
            ExecuteNextAction();
        }));
    }

    System.Collections.IEnumerator WaitForArrival(System.Action callback)
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        callback?.Invoke();
    }
}

public static class Planner
{
    public static List<ActionGoap> Plan(GameWorldState initialState, List<ActionGoap> actions)
    {
        List<ActionGoap> validActions = actions.Where(a => a.precondition(initialState)).OrderBy(a => a.cost).ToList();
        return validActions.Count > 0 ? validActions : null;
    }
}
