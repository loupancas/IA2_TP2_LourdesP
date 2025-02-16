using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;


public class EnemyAproach : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    float _distanceToPlayer;
    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
            return Transitions[StateTransitions.ToIdle];

        return this;
    }

    public override void UpdateLoop()
    {
        if (_distanceToPlayer <= 1.5f) _stateFinished = true;
    }
}
