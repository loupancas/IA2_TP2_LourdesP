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


    public void AOAAttack()
    {
        BaseEnemy.Instance.Fatigarse(6);
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }
    public override void UpdateLoop()
    {
        if (_distanceToPlayer <= 3f) _stateFinished = true;
    }
}