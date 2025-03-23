using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;


public class Entidad : MonoBehaviour
{
    #region VARIABLES
    public TextMesh lblNumber, lblId;
    public Transform body, inventory;
    public string initialId;
    public Color initialColor;

    public event Action<Entidad> OnHitFloor = delegate { };
    public event Action<Entidad, Transform> OnHitWall = delegate { };
    public event Action<Entidad, Item> OnHitItem = delegate { };
    public event Action<Entidad, Waypoint, bool> OnReachDestination = delegate { };

    public List<Item> initialItems;

    public List<Item> _items = new List<Item>();
    Vector3 _vel;
    bool _onFloor;
    string _label;
    int _number;
    Color _color;
    string _alive;
    public float speed = 10f;
    Transform _transformPosition;
    Waypoint _gizmoRealTarget;
    IEnumerable<Waypoint> _gizmoPath;

    #region GETTERS & SETTERS
    public IEnumerable<Item> items { get { return _items; } }

    public string label
    {
        get { return _label; }
        set
        {
            if (value == null || value.Length == 0)
            {
                _label = null;
                lblId.text = "";
            }
            else
            {
                _label = value;
                lblId.text = "\u2190" + value;
            }
        }
    }

    public int number
    {
        get { return _number; }
        set
        {
            _number = value;
            lblNumber.text = value.ToString();
        }
    }

    public Color color
    {
        get { return _color; }
        set
        {
            _color = value;
            Paint(value);
        }
    }


    public String alive
    {
        get { return _alive; }
        set
        {
            _alive = value;

        }
    }


    #endregion

    #endregion

    void Awake()
    {
        if (_items == null)
        {
            _items = new List<Item>();
        }
        _vel = Vector3.zero;
        _onFloor = false;
        label = initialId;
        number = 99;
        _alive = "vivo";
        _transformPosition = GetComponent<Transform>();
    }

    void Start()
    {
        color = initialColor;

        foreach (var it in initialItems)
            AddItem(Instantiate(it));
    }

    #region MOVEMENT & COLLISION
    void FixedUpdate()
    {
        transform.Translate(Time.fixedDeltaTime * _vel * speed);
    }

    public void Jump()
    {
        if (_onFloor)
        {
            _onFloor = false;
            GetComponent<Rigidbody>().AddForce(Vector3.up * 3f, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "Floor")
        {
            _onFloor = true;
            OnHitFloor(this);
        }
        else if (col.collider.tag == "Wall")
        {
            OnHitWall(this, col.collider.transform);

        }
        //else if (col.collider.tag == "Door")
        //{
        //    var door = col.collider.GetComponent<Door>();
        //    if (door.open == false)
        //    {
        //        door.Open();
        //    }
        //}
        else
        {
            var item = col.collider.GetComponentInParent<Item>();
            if (item && item.transform.parent != inventory)
                OnHitItem(this, item);
        }



        if (col.collider.tag == "Police")
        {
            Life();
        }
    }

    public void Life()
    {
        alive = "muerto";
    }

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        var e = other.GetComponent<Entidad>();
        if (e != null && e != this)
        {
            Debug.Log(e.name + " hit " + name);
            ;
        }
    }
    #endregion

    #region ITEM MANAGEMENT
    public void AddItem(Item item)
    {


        _items.Add(item);
        Debug.Log($"Item {item.name} added to {gameObject.name}'s inventory.");

        item.OnInventoryAdd();
        item.transform.parent = inventory;

        RefreshItemPositions();
    }

    public Item Removeitem(Item item)
    {

        _items.Remove(item);
        item.OnInventoryRemove();
        item.transform.parent = null;
        RefreshItemPositions();
        return item;

    }

    public Item DropItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
            item.OnInventoryRemove();
            item.transform.parent = null;
            item.transform.position = transform.position + Vector3.up;
            RefreshItemPositions();
            return item;
        }
        return null;
    }



    public IEnumerable<Item> RemoveAllitems()
    {
        var ret = _items;
        foreach (var item in items)
        {
            item.OnInventoryRemove();
        }
        _items = new List<Item>();
        RefreshItemPositions();
        return ret;
    }

    void RefreshItemPositions()
    {
        const float Dist = 1.25f;
        for (int i = 0; i < _items.Count; i++)
        {
            var phi = (i + 0.5f) * Mathf.PI / (_items.Count);
            _items[i].transform.localPosition = new Vector3(-Mathf.Cos(phi) * Dist, Mathf.Sin(phi) * Dist, 0f);
        }
    }
    #endregion

    Vector3 FloorPos(MonoBehaviour b)
    {
        return FloorPos(b.transform.position);
    }
    Vector3 FloorPos(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    Coroutine _navCR;
    public void GoTo(Vector3 destination)
    {
        Debug.Log("GoTo" + destination);

        _navCR = StartCoroutine(Navigate(destination));
    }

    public void Stop()
    {
        if (_navCR != null) StopCoroutine(_navCR);
        _vel = Vector3.zero;
    }

    protected virtual IEnumerator Navigate(Vector3 destination)
    {
        var srcWp = Navigation.instance.NearestTo(transform.position);
        Debug.Log("srcWp" + srcWp);
        var dstWp = Navigation.instance.NearestTo(destination);
        Debug.Log("dstWp" + dstWp);

        _gizmoRealTarget = dstWp;
        Waypoint reachedDst = srcWp;

        if (srcWp != dstWp)
        {
            var path = _gizmoPath = AEstrella<Waypoint>.Go(
                  srcWp
                , dstWp
                , (wa, wb) => Vector3.Distance(wa.transform.position, wb.transform.position)
                , w => w == dstWp
                , w =>
                    //w.nearbyItems.Any(it => it.type == ItemType.Door)
                    //? null
                    //:
                    w.adyacent
                    //.Where(a => a.nearbyItems.All(it => it.type != ItemType.Door))
                    .Select(a => new AEstrella<Waypoint>.WeightedNode(a, Vector3.Distance(a.transform.position, w.transform.position)))
            );
            if (path != null)
            {
                foreach (var next in path.Select(w => FloorPos(w)))
                {

                    while ((next - FloorPos(this)).sqrMagnitude >= 0.05f)
                    {
                        _vel = (next - FloorPos(this)).normalized;
                        yield return null;
                    }
                    //_vel = (next - FloorPos(this)).normalized;
                    //yield return new WaitUntil(() => (next - FloorPos(this)).sqrMagnitude < 0.05f);
                }
            }
            reachedDst = path.Last();
        }

        if (reachedDst == dstWp)
        {
            _vel = (FloorPos(destination) - FloorPos(this)).normalized;
            yield return new WaitUntil(() => (FloorPos(destination) - FloorPos(this)).sqrMagnitude < 0.05f);
        }

        _vel = Vector3.zero;
        OnReachDestination(this, reachedDst, reachedDst == dstWp);
    }

    void Paint(Color color)
    {
        foreach (Transform xf in body)
            xf.GetComponent<Renderer>().material.color = color;
        lblNumber.color = new Color(1f - color.r, 1f - color.g, 1f - color.b);
    }

    void OnDrawGizmos()
    {
        if (_gizmoPath == null)
            return;

        Gizmos.color = color;
        var points = _gizmoPath.Select(w => FloorPos(w));
        Vector3 last = points.First();
        foreach (var p in points.Skip(1))
        {
            Gizmos.DrawLine(p + Vector3.up, last + Vector3.up);
            last = p;
        }
        if (_gizmoRealTarget != null)
            Gizmos.DrawCube(_gizmoRealTarget.transform.position + Vector3.up * 1f, Vector3.one * 0.3f);
    }
}
