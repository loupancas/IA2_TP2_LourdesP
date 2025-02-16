using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class EnemyAOAAttack : MonoBaseState
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
        if (_distanceToPlayer <= 3f) _stateFinished = true;
    }
}