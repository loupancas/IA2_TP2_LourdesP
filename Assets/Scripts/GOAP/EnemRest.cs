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
            return Transitions[StateTransitions.ToIdle];

        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("EnemyRest");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }

    private void Update()
    {
        Rest();
    }

    private void Rest()
    {
       BaseEnemy.Instance.Fatigarse(-1);
    }

    public override void UpdateLoop()
    {
        if(_fatigue == 0f) _stateFinished = true;
        
    }
}