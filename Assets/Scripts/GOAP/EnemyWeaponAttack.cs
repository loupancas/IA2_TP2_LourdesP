using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;



public class EnemyWeaponAttack : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    string _weapon;
    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
            return Transitions[StateTransitions.ToIdle];

        return this;
    }

    public void WeaponAttack()
    {
        _weapon = "HasWeapon";
        BaseEnemy.Instance.Fatigarse(6);
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }

    public override void UpdateLoop()
    {
        if (_weapon== "HasWeapon") _stateFinished = true;
    }
}