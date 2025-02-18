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


        //// Ejemplo de condición para cambiar al estado de movimiento
        //if (ShouldMove())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToMovement];
        //}
        //// Ejemplo de condición para cambiar al estado de ataque AOA
        //else if (ShouldAOAAttack())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToAOAAttack];
        //}
        //// Ejemplo de condición para cambiar al estado de ataque con arma
        //else if (ShouldWeaponAttack())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToWeaponAttack];
        //}
        //// Ejemplo de condición para cambiar al estado de aproximación
        //else if (ShouldAproach())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToAproach];
        //}
        //// Ejemplo de condición para cambiar al estado de descanso
        //else if (ShouldRest())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToRest];
        //}

        //// Si ninguna condición se cumple, permanece en el estado actual
        //return this;


    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        base.Enter(from, transitionParameters);
        //_idleTime = constTime + Random.Range(variantTimeMin, variantTimeMax);
       Debug.Log("EnemyIdle");
        _gAgent = GetComponent<GAgent>();
    }


    public override void UpdateLoop()
    {
        _idleTime -= Time.deltaTime;
    }



    //private bool ShouldMove()
    //{
    //    // Define la lógica para determinar si el agente debe moverse
    //    // Por ejemplo, si el agente detecta un objetivo en su rango de visión
    //    return Vector3.Distance(_gAgent.transform.position, _gAgent.target.position) > _gAgent.minDistanceToTarget;
    //}

    //private bool ShouldAOAAttack()
    //{
    //    // Define la lógica para determinar si el agente debe realizar un ataque AOA
    //    // Por ejemplo, si el agente está lo suficientemente cerca del objetivo
    //    return Vector3.Distance(_gAgent.transform.position, _gAgent.target.position) < _gAgent.attackRange;
    //}

    //private bool ShouldWeaponAttack()
    //{
    //    // Define la lógica para determinar si el agente debe realizar un ataque con arma
    //    // Por ejemplo, si el agente tiene un arma y está en rango de ataque
    //    return _gAgent.HasWeapon && Vector3.Distance(_gAgent.transform.position, _gAgent.target.position) < _gAgent.weaponRange;
    //}

    //private bool ShouldAproach()
    //{
    //    // Define la lógica para determinar si el agente debe aproximarse al objetivo
    //    // Por ejemplo, si el agente necesita acercarse para atacar
    //    return Vector3.Distance(_gAgent.transform.position, _gAgent.target.position) > _gAgent.attackRange;
    //}

    //private bool ShouldRest()
    //{
    //    // Define la lógica para determinar si el agente debe descansar
    //    // Por ejemplo, si el agente está fatigado
    //    return _gAgent.energyLevel < _gAgent.minEnergyLevel;
    //}
}
