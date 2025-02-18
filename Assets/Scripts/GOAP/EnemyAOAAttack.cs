using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class EnemyAOAAttack : MonoBaseState
{
    private GAgent _gAgent;
    bool _stateFinished;
    bool canAttack;
    float _distanceToPlayer;
    public Transform player;
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

    private void Attack()
    {

        if (Vector3.Distance(transform.position, player.position) < _gAgent._viewRadius)
        {
            AOAAttack();
            canAttack = true;

        }
    }

    public void AOAAttack()
    {
        FirstPersonPlayer.instance.TakeDamage(10);
        BaseEnemy.Instance.Fatigarse(10);
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("EnemyAOAAttack");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }
    public override void UpdateLoop()
    {
        if (canAttack == true) _stateFinished = true;
    }
}