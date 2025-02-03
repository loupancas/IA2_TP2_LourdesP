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

public class GOAPAction
{
    public string name;
    public Func<GameWorldState, bool> precondition;
    public Action<GameWorldState> effect;
    public float cost;
    public Vector3 targetPosition;

    public GOAPAction(string name, Func<GameWorldState, bool> precondition, Action<GameWorldState> effect, float cost, Vector3 targetPosition)
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
    public List<GOAPAction> availableActions;
    public GameWorldState worldState = new GameWorldState();
    public NavMeshAgent agent;
    private Queue<GOAPAction> actionQueue;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        availableActions = new List<GOAPAction> {
            new GOAPAction("Pick Gold", w => w.gold < 10, w => w.gold += 5, 1, new Vector3(5, 0, 5)),
            new GOAPAction("Heal", w => w.health < 50, w => w.health += 30, 2, new Vector3(10, 0, 10)),
            new GOAPAction("Attack", w => w.weapon == "sword", w => w.isAlive = false, 3, new Vector3(15, 0, 15))
        };
        PlanAndExecute();
    }

    void PlanAndExecute()
    {
        var plan = GOAPPlanner.Plan(worldState.Clone(), availableActions);
        if (plan != null)
        {
            actionQueue = new Queue<GOAPAction>(plan);
            ExecuteNextAction();
        }
    }

    void ExecuteNextAction()
    {
        if (actionQueue.Count == 0) return;
        GOAPAction action = actionQueue.Dequeue();
        agent.SetDestination(action.targetPosition);
        StartCoroutine(WaitForArrival(() => {
            action.effect(worldState);
            ExecuteNextAction();
        }));
    }

    System.Collections.IEnumerator WaitForArrival(Action callback)
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        callback?.Invoke();
    }
}

public static class GOAPPlanner
{
    public static List<GOAPAction> Plan(GameWorldState initialState, List<GOAPAction> actions)
    {
        List<GOAPAction> validActions = actions.Where(a => a.precondition(initialState)).OrderBy(a => a.cost).ToList();
        return validActions.Count > 0 ? validActions : null;
    }
}
