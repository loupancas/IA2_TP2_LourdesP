using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using IA2;
public enum ActionEntity
{
    Pickup,
    PickUpM,
    NextStep,
    FailedStep,
    Open,
    Matar,
    Loot,
    Sobornar,
    Breaking,
    Success
}

public class Guy : MonoBehaviour
{
    private EventFSM<ActionEntity> _fsm;
    private Item _target;
    private Item _pastafrola;
    EnemyMovement _enemyMovement;
    private Entidad _ent;
    private Entidad _police;
    public int llave = 0;
    Item _door;
    IEnumerable<Tuple<ActionEntity, Item>> _plan;



    private void PerformSobornar(Entidad us, Item other)
    {

        if (other != _target) return;
        var money = _ent.items.FirstOrDefault(it => it.type == ItemType.Money);
        var key = _police.items.FirstOrDefault(it => it.type == ItemType.Key);
        if (money != null && key != null)
        {
            _police.DropItem(key);
            _ent.DropItem(money);
            _ent.AddItem(key);
            _police.AddItem(money);
            Debug.Log("Acepto dinero");
            llave = 1;
            _fsm.Feed(ActionEntity.NextStep);
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
            Debug.Log("PerformSobornar failed");
        }
    }


    private void PerformAttack(Entidad us, Item other)
    {
        Debug.Log("PerformAttack", other.gameObject);
        if (other != _target) return;

        var mace = _ent.items.FirstOrDefault(it => it.type == ItemType.Mace);
        if (mace)
        {
            other.Kill();
            if (other.type == ItemType.Door)
                Destroy(_ent.Removeitem(mace).gameObject);
            _fsm.Feed(ActionEntity.NextStep);
        }
        else
            _fsm.Feed(ActionEntity.FailedStep);
        Debug.Log("PerformAttack failed");
    }

    private void Matar(Entidad us, Item other)
    {
        if (other != _target) return;
        var mace = _ent.items.FirstOrDefault(it => it.type == ItemType.Mace);
        if (mace)
        {
            other.Kill();
            Destroy(_ent.Removeitem(mace).gameObject);

            _fsm.Feed(ActionEntity.NextStep);
        }
        else
            _fsm.Feed(ActionEntity.FailedStep);
        Debug.Log("Matar failed");
    }

    private void PerformOpen(Entidad us, Item other)
    {
        Debug.Log("PerformOpen", other.gameObject);
        other = _door;
        if (other != _target) return;


        var key = _ent.items.FirstOrDefault(it => it.type == ItemType.Key);
        Door doorComponent = other.GetComponent<Door>();

        if (key != null && doorComponent.open == false)
        {

            Debug.Log("Open" + other.gameObject);
            doorComponent.Open();
            Destroy(_ent.Removeitem(key).gameObject);
            _fsm.Feed(ActionEntity.NextStep);

        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
            Debug.Log("PerformOpen failed");

        }

        //_fsm.Feed(ActionEntity.NextStep);

    }

    private void PerformPickUp(Entidad us, Item other)
    {
        Debug.Log("PerformPickUp called");
        Debug.Log("PerformPickUp" + other.gameObject);
        other=_pastafrola;
        if (other != _target) return;

        _ent.AddItem(other);

        _enemyMovement.Empezar();
        Debug.Log("PerformPickUp" + other.gameObject);

    }

    private void PerformPickUpM(Entidad us, Item other)
    {
        if (other != _target) return;

        _ent.AddItem(other);
        Debug.Log("PerformPickUpM" + other.gameObject);
        _fsm.Feed(ActionEntity.NextStep);

    }


    private void PerformLoot(Entidad us, Item other)
    {
        if (other != _target) return;
        if (_police.alive == "muerto")
        {
            _ent.AddItem(other);
            _fsm.Feed(ActionEntity.NextStep);
        }
        else
            _fsm.Feed(ActionEntity.FailedStep);
    }

    private void NextStep(Entidad ent, Waypoint wp, bool reached)
    {
        _fsm.Feed(ActionEntity.NextStep);
    }

    private void Awake()
    {
        _ent = GetComponent<Entidad>();
        _police = GameObject.Find("Police").GetComponent<Entidad>();
        _door = GameObject.Find("DoorLocked").GetComponent<Item>();
        _enemyMovement = GameObject.Find("BooBoss").GetComponent<EnemyMovement>();
        _pastafrola = GameObject.Find("PastaFrola").GetComponent<Item>();
        var any = new State<ActionEntity>("any");

        var idle = new State<ActionEntity>("idle");
        var bridgeStep = new State<ActionEntity>("planStep");
        var failStep = new State<ActionEntity>("failStep");
        var kill = new State<ActionEntity>("kill");
        var breaking = new State<ActionEntity>("breaking");
        var pickup = new State<ActionEntity>("pickup");
        var pickupm = new State<ActionEntity>("pickupM");
        var loot = new State<ActionEntity>("loot");
        var sobornar = new State<ActionEntity>("sobornar");
        var open = new State<ActionEntity>("open");
        var success = new State<ActionEntity>("success");
        var matar = new State<ActionEntity>("matar");

        matar.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += Matar; };
        matar.OnExit += a => _ent.OnHitItem -= Matar;

        kill.OnEnter += a => {
            _ent.GoTo(_target.transform.position);
            _ent.OnHitItem += PerformAttack;
        };

        kill.OnExit += a => _ent.OnHitItem -= PerformAttack;

        sobornar.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformSobornar; }; 
        sobornar.OnExit += a => _ent.OnHitItem -= PerformSobornar;

        breaking.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformAttack; };
        breaking.OnExit += a => _ent.OnHitItem -= PerformAttack;

        failStep.OnEnter += a => { _ent.Stop(); Debug.Log("Plan failed"); };

        loot.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformLoot; };
        loot.OnExit += a => _ent.OnHitItem -= PerformLoot;

        pickup.OnEnter += a => {
            Debug.Log("entering pickup");
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUp; };
            Debug.Log("exit pickup");
        pickup.OnExit += a => _ent.OnHitItem -= PerformPickUp;

        pickupm.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUpM; };
        pickupm.OnExit += a => _ent.OnHitItem -= PerformPickUpM;

        open.OnEnter += a => {
            _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformOpen; };
        open.OnExit += a => _ent.OnHitItem -= PerformOpen;

        bridgeStep.OnEnter += a => {
            var step = _plan.FirstOrDefault();
            if (step != null)
            {

                _plan = _plan.Skip(1);
                var oldTarget = _target;
                _target = step.Item2;
                if (!_fsm.Feed(step.Item1))
                    _target = oldTarget;
            }
            else
            {
                _fsm.Feed(ActionEntity.Success);
            }
        };

        success.OnEnter += a => { Debug.Log("Success"); };
        success.OnUpdate += () => { _ent.Jump(); };

        StateConfigurer.Create(any)
            .SetTransition(ActionEntity.NextStep, bridgeStep)
            .SetTransition(ActionEntity.FailedStep, idle)
            .Done();

        StateConfigurer.Create(bridgeStep)
            .SetTransition(ActionEntity.Breaking, breaking)
            .SetTransition(ActionEntity.Pickup, pickup)
            .SetTransition(ActionEntity.PickUpM, pickupm)
            .SetTransition(ActionEntity.Open, open)
            .SetTransition(ActionEntity.Sobornar, sobornar)
            .SetTransition(ActionEntity.Success, success)
            .SetTransition(ActionEntity.Matar, matar)
            .SetTransition(ActionEntity.Loot, loot)
            .Done();

        _fsm = new EventFSM<ActionEntity>(idle, any);
    }

    public void ExecutePlan(List<Tuple<ActionEntity, Item>> plan)
    {
        _plan = plan;
        _fsm.Feed(ActionEntity.NextStep);
    }

    private void Update()
    {
        _fsm.Update();
    }
}
