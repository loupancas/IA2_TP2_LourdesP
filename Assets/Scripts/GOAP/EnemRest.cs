using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class EnemRest : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    int _fatigue;
    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
        {
            Debug.Log("Transitioning to Idle from EnemyRest");
            return Transitions[StateTransitions.ToIdle];
        }
           

        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Entering EnemyRest");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }

    private void Update()
    {
        //Rest();
    }

    private void Rest()
    {
        Debug.Log("Resting");
        BaseEnemy.Instance.Fatigarse(-1);
    }

    public override void UpdateLoop()
    {
        if(_fatigue == 0f)
        {
            Debug.Log("EnemyRest finished");
            _stateFinished = true;
        }
           
        
    }
}