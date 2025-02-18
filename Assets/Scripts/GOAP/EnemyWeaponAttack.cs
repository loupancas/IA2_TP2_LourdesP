using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;



public class EnemyWeaponAttack : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    string _weapon;
    public Transform player;
    bool canAttack;
    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
            return Transitions[StateTransitions.ToIdle];

        return this;
    }

    private void Update()
    {
        //Attack();
    }

    public void WeaponAttack()
    {
        FirstPersonPlayer.instance.TakeDamage(5);
        BaseEnemy.Instance.Fatigarse(5);
    }

    private void Attack()
    {
        //_gAgent._state.Set("DistanciaPlayer", Vector3.Distance(transform.position, player.position));

        if (Vector3.Distance(transform.position, player.position) < _gAgent._closeView)
        {
            WeaponAttack();
            canAttack = true;

        }
    }


    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("EnemyWeaponAttack");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }

    public override void UpdateLoop()
    {
        if (canAttack == true) _stateFinished = true;
    }
}