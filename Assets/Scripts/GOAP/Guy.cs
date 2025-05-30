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
    public static Guy Instance { get; private set; }

    private EventFSM<ActionEntity> _fsm;
    public Item _target;
    private Item _pastafrola;
    EnemyMovement _enemyMovement;
    private Entidad _ent;
    private Entidad _police;
    public int llave = 0;
    IEnumerable<Tuple<ActionEntity, Item>> _plan;
    private bool isAnimating = false;
    [SerializeField] public Animator _animator;
    public bool canMove = true;
    public Door door;
    public Item _door;
    public bool CanMove() => canMove;
    public bool animationplayed=false;

    public IEnumerator WaitForAnimationToEnd(string animationName, Action onComplete)
    {
        // Esperamos un frame para asegurarnos que la animación empezó
        yield return null;

        // Esperamos hasta que el animator esté en el estado correcto
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            yield return null;
        }

        // Ahora esperamos a que la animación termine
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // Esperamos el tiempo restante de la animación (considerando el tiempo ya transcurrido)
        yield return new WaitForSeconds(animationLength * (1f - stateInfo.normalizedTime));

        onComplete?.Invoke();
    }

    private void PerformAttack(Entidad us, Item other)
    {
        if (other != _target) return;

        var mace = _ent.items.FirstOrDefault(it => it.type == ItemType.Mace);
        if (mace)
        {
           
                other.Kill();
                if (other.type == ItemType.Door)
                    Destroy(_ent.Removeitem(mace).gameObject);
                canMove = true;
                _fsm.Feed(ActionEntity.NextStep);
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
        }
    }

    private void Matar(Entidad us, Item other)
    {
        if (other != _target) return;

        var mace = _ent.items.FirstOrDefault(it => it.type == ItemType.Mace);
        if (mace)
        {
            
                other.Kill();
                Destroy(_ent.Removeitem(mace).gameObject);
                canMove = true;
                _fsm.Feed(ActionEntity.NextStep);
            
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
        }
    }

    private void PerformSobornar(Entidad us, Item other)
    {
        Debug.Log("test 1");

        if (other != _target) return;

        var money = _ent.items.FirstOrDefault(it => it.type == ItemType.Money);
        var key = _police.items.FirstOrDefault(it => it.type == ItemType.Key);
        Debug.Log("test 2");

        if (money != null && key != null )
        {
            _police.DropItem(key);
            _ent.DropItem(money);
            _ent.AddItem(key);
            _police.AddItem(money);
            Debug.Log("test 3");

            Debug.Log("Acepto dinero");
            llave = 1;

            canMove = false;
            Debug.Log("test 4");
            if (animationplayed == false)
            {

                StartCoroutine(DelayedAnimation("pick_up", 0.5f, () => {
                canMove = true;
                Debug.Log("test 5");
                    animationplayed = true;

                    _fsm.Feed(ActionEntity.NextStep);

            }));
            }
            else { return; }
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
            Debug.Log("PerformSobornar failed");
        }
    }
    private void PerformPickUp(Entidad us, Item other)
    {
     
        other = _pastafrola;

        if (other != _target) return;

        canMove = false;
        _ent.AddItem(other);

        StartCoroutine(DelayedAnimation("pick_up", 0.5f, () => {


            canMove = true;
            _enemyMovement.Empezar();
          
            _fsm.Feed(ActionEntity.NextStep);

        }));
       
    }
    
    public void PerformOpen(Entidad us, Item other)
    {
        other = _door;

        if (other != _target) return;

        var key = _ent.items.FirstOrDefault(it => it.type == ItemType.Key);

        Door doorComponent = other.GetComponent<Door>();

        Debug.Log("test 1");
        if (key != null && doorComponent.open == false)
        {
            Debug.Log("test 2");

            canMove = false;

            StartCoroutine(DelayedAnimation("open", 0.5f, () => {
                

                door.GetComponent<Door>().Deactivate();

                canMove = true;
                doorComponent.open = true;
                _fsm.Feed(ActionEntity.NextStep);

            }));
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
            Debug.Log("PerformOpen failed");
        }
    }


    private void PerformPickUpM(Entidad us, Item other)
    {
        if (other != _target) return;

        //canMove = false;
        // _animator.SetTrigger("pick_up");
        /*StartCoroutine(WaitForCurrentAnimationToEnd(() =>
        {
            _ent.AddItem(other);
            canMove = true;
            _fsm.Feed(ActionEntity.NextStep);
        }));*/
    }

    private void PerformLoot(Entidad us, Item other)
    {
        if (other != _target) return;

        if (_police.alive == "muerto")
        {
            // canMove = false;
            // _animator.SetTrigger("pick_up");
            /*StartCoroutine(WaitForCurrentAnimationToEnd(() =>
            {
                _ent.AddItem(other);
                canMove = true;
                _fsm.Feed(ActionEntity.NextStep);
            }));*/
        }
        else
        {
            _fsm.Feed(ActionEntity.FailedStep);
        }
    }

    private void NextStep(Entidad ent, Waypoint wp, bool reached)
    {
        _fsm.Feed(ActionEntity.NextStep);
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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

        matar.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += Matar; };
        matar.OnExit += a => _ent.OnHitItem -= Matar;

        kill.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformAttack; };
        kill.OnExit += a => _ent.OnHitItem -= PerformAttack;

        sobornar.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformSobornar; };
        sobornar.OnExit += a => _ent.OnHitItem -= PerformSobornar;

        breaking.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformAttack; };
        breaking.OnExit += a => _ent.OnHitItem -= PerformAttack;

        failStep.OnEnter += a => { _ent.Stop(); Debug.Log("Plan failed"); };

        loot.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformLoot; };
        loot.OnExit += a => _ent.OnHitItem -= PerformLoot;

        pickup.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUp; };
        pickup.OnExit += a => _ent.OnHitItem -= PerformPickUp;

        pickupm.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUpM; };
        pickupm.OnExit += a => _ent.OnHitItem -= PerformPickUpM;

        open.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformOpen; };
        open.OnExit += a => _ent.OnHitItem -= PerformOpen;

        bridgeStep.OnEnter += a =>
        {
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

    public IEnumerator DelayedAnimation(string animationName, float delayedSeconds, Action onComplete = null)
    {
       

        yield return StartCoroutine(Guy.Instance.WaitForAnimationToEnd(animationName, () =>
        {
            onComplete?.Invoke();
        }));
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