using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;

public class EnemyGetWeapon : MonoBaseState
{
    private AStar<Node> _aStar;
    private List<Node> _path;
    private int _currentPathIndex;
    public float speed = 3f;
    public Transform weapon;
    public float chaseDistance = 10f;
    public float updatePathInterval = 1f; // Intervalo para recalcular el camino
    public float maxFrameTime = 0.016f; // Tiempo máximo por frame (60 FPS), ajustable desde el Inspector
    private bool gotWeapon = false;
    private Coroutine _pathfindingCoroutine;
    private GAgent _gAgent;
    bool _stateFinished;
    string _weapon;
    private GameObject weaponObject;
    MeshRenderer _Gun;
    string _hasWeapon;
    void Start()
    {
        _aStar = new AStar<Node>();
        _aStar.OnPathCompleted += GetPath;
        _aStar.OnCantCalculate += PathNotFound;
        _aStar.maxFrameTime = maxFrameTime; // Ajuste del tiempo máximo por frame
        StartCoroutine(UpdatePathRoutine());
    }

    private void Update()
    {
        //Debug.Log("EnemyGetWeapon Update");
        MoveAlongPath();
    }

    private IEnumerator UpdatePathRoutine()
    {
        //while (true)
        //{
            // Recalcular la ruta si el jugador está dentro del rango de persecución
            if (Vector3.Distance(transform.position, weapon.position) < chaseDistance)
            {
                gotWeapon = false;
                Node startNode = FindClosestNode(transform.position);
                Node endNode = FindClosestNode(weapon.position);
                if (_pathfindingCoroutine != null)
                {
                    StopCoroutine(_pathfindingCoroutine);
                }
                _pathfindingCoroutine = StartCoroutine(_aStar.Run(startNode, node => node == endNode, Explode, GetHeuristic));
            }
            //else
            //{
            //    isChasing = false;
            //}
            yield return new WaitForSeconds(updatePathInterval); // Esperar antes de recalcular la ruta
        //}
    }



    private Node FindClosestNode(Vector3 position)
    {
        Node[] nodes = FindObjectsOfType<Node>();
        Node closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (Node node in nodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < closestDistance)
            {
                closestNode = node;
                closestDistance = distance;
            }
        }

        return closestNode;
    }

    private IEnumerable<WeightedNode<Node>> Explode(Node node)
    {
        return node.neighbour.Select(neighbour => new WeightedNode<Node>(neighbour, 1));
    }

    private float GetHeuristic(Node node)
    {
        return Vector3.Distance(node.transform.position, weapon.position);
    }

    private void GetPath(IEnumerable<Node> path)
    {
        _path = new List<Node>(path);
        _currentPathIndex = 0;
    }

    private void PathNotFound()
    {
        Debug.Log("Path not found");
        gotWeapon = false;
    }

    private void MoveAlongPath()
    {
        if (_path == null || _currentPathIndex >= _path.Count)
            return;

        Node targetNode = _path[_currentPathIndex];
        Vector3 targetPosition = targetNode.transform.position;

        // Rotar hacia el objetivo
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);

        // Mover hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _currentPathIndex++;
        }

        Debug.Log("EnemyGetWeapon MoveAlongPath");

        // _gAgent._state.Set("Weapon","HasWeapon");



    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BossWeapon"))
        {
            _Gun = weaponObject.GetComponent<MeshRenderer>();
            _hasWeapon = _gAgent.GetWeapon();
        }
    }


    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Entering EnemyGetWeapon");
        base.Enter(from, transitionParameters);
        _gAgent = GetComponent<GAgent>();

    }
    public override void UpdateLoop()
    {
        if (gotWeapon == true)
        {
            Debug.Log("EnemyGetWeapon finished");
            _stateFinished = true;
        }
           
    }

    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
        {
            Debug.Log("Transitioning to Idle from EnemyGetWeapon");
            return Transitions[StateTransitions.ToIdle];
        }

        return this;
    }
}