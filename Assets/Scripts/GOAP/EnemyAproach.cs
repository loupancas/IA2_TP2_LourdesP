using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;


public class EnemyAproach : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    float _distanceToPlayer;
    public Transform player;
    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
        {
            Debug.Log("Transitioning to Idle from EnemyAproach");
            return Transitions[StateTransitions.ToIdle];
        }
           

        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Entering EnemyAproach");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }

    public override void UpdateLoop()
    {
        if (Vector3.Distance(transform.position, player.position) <= 1.5f)
        {
            Debug.Log("EnemyAproach finished");
            _stateFinished = true;
        }
           
    }
}
