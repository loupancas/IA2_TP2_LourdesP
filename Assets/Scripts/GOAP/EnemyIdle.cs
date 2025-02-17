using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


public class EnemyIdle: MonoBaseState
{
    private GAgent _gAgent;
    private float _idleTime;
    public override IState ProcessInput()
    {
        if (Transitions.ContainsKey(StateTransitions.ToPursuit)) return Transitions[StateTransitions.ToPursuit];
        if (Transitions.ContainsKey(StateTransitions.ToAOA)) return Transitions[StateTransitions.ToAOA];
        if (Transitions.ContainsKey(StateTransitions.ToAttack)) return Transitions[StateTransitions.ToAttack];
        if (Transitions.ContainsKey(StateTransitions.ToRest)) return Transitions[StateTransitions.ToRest];
        if (Transitions.ContainsKey(StateTransitions.ToGetWeapon)) return Transitions[StateTransitions.ToGetWeapon];
        if (Transitions.ContainsKey(StateTransitions.ToAproach)) return Transitions[StateTransitions.ToAproach];


        if (_gAgent.GetDistanceToPlayer() < _gAgent._viewRadius)
        {
            return Transitions.ContainsKey(StateTransitions.ToPursuit) ? Transitions[StateTransitions.ToPursuit] : this;
        }

        if (_gAgent.GetFatigue() >= 6 && Transitions.ContainsKey(StateTransitions.ToRest))
        {
            return Transitions[StateTransitions.ToRest];
        }

        if (_gAgent.GetWeapon() == "none" && Transitions.ContainsKey(StateTransitions.ToGetWeapon))
        {
            return Transitions[StateTransitions.ToGetWeapon];
        }

        return this;

        
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        base.Enter(from, transitionParameters);
        //_idleTime = constTime + Random.Range(variantTimeMin, variantTimeMax);
        print("Idle");
        _gAgent = GetComponent<GAgent>();
    }


    public override void UpdateLoop()
    {
        _idleTime -= Time.deltaTime;
    }
}
