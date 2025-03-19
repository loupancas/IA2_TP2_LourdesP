using FSM;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class EnemyMovement : MonoBaseState
{
    private AStar<Node> _aStar;
    private List<Node> _path;
    private int _currentPathIndex;
    public float speed = 3f;
    public Transform player;
    public float chaseDistance = 10f;
    public float updatePathInterval = 1f; // Intervalo para recalcular el camino
    public float maxFrameTime = 0.016f; // Tiempo máximo por frame (60 FPS), ajustable desde el Inspector
    private bool isChasing = false;
    private Coroutine _pathfindingCoroutine;
    private Guy guy;
    bool _stateFinished;
    void Start()
    {
        _aStar = new AStar<Node>();
        _aStar.OnPathCompleted += GetPath;
        _aStar.OnCantCalculate += PathNotFound;
        _aStar.maxFrameTime = maxFrameTime; // Ajuste del tiempo máximo por frame
        StartCoroutine(UpdatePathRoutine());
    }

    private IEnumerator UpdatePathRoutine()
    {
        while (true)
        {
            // Recalcular la ruta si el jugador está dentro del rango de persecución
            if (Vector3.Distance(transform.position, player.position) < chaseDistance)
            {
                isChasing = true;
                Node startNode = FindClosestNode(transform.position);
                Node endNode = FindClosestNode(player.position);
                if (_pathfindingCoroutine != null)
                {
                    StopCoroutine(_pathfindingCoroutine);
                }
                _pathfindingCoroutine = StartCoroutine(_aStar.Run(startNode, node => node == endNode, Explode, GetHeuristic));
            }
            else
            {
                isChasing = false;
            }
            yield return new WaitForSeconds(updatePathInterval); // Esperar antes de recalcular la ruta
        }
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
        return Vector3.Distance(node.transform.position, player.position);
    }

    private void GetPath(IEnumerable<Node> path)
    {
        _path = new List<Node>(path);
        _currentPathIndex = 0;
    }

    private void PathNotFound()
    {
        Debug.Log("Path not found");
        isChasing = false;
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

        //if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        //{
        //    _currentPathIndex++;
        //}


        //_gAgent._state.Set("DistanciaPlayer", Vector3.Distance(transform.position, player.position));

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _currentPathIndex++;

            if (_currentPathIndex >= _path.Count)
            {
                _stateFinished = true; // Marcar estado como finalizado
            }
        }

    }

    public override void UpdateLoop()
    {
        if (Vector3.Distance(transform.position, player.position) < chaseDistance)
        {
            _stateFinished = true;
        }

        if (isChasing)
        {
            MoveAlongPath();
        }
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("EnemyMovement");
        base.Enter(from, transitionParameters);
        guy = GetComponent<Guy>();

    }

    public override Dictionary<string, object> Exit(IState to)
    {
      
        _stateFinished = false;
        return base.Exit(to);
    }


    public override IState ProcessInput()
    {
        if (_stateFinished && Transitions.ContainsKey(StateTransitions.ToIdle))
            return Transitions[StateTransitions.ToIdle];

        return this;


        //// Ejemplo de condición para cambiar al estado de ataque AOA
        //if (ShouldAOAAttack())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToAOAAttack];
        //}
        //// Ejemplo de condición para cambiar al estado de ataque con arma
        //else if (ShouldWeaponAttack())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToWeaponAttack];
        //}
        //// Ejemplo de condición para cambiar al estado de descanso
        //else if (ShouldRest())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToRest];
        //}
        //// Ejemplo de condición para cambiar al estado de idle
        //else if (ShouldIdle())
        //{
        //    return _gAgent.stateTransitions[StateTransitions.ToIdle];
        //}

        //// Si ninguna condición se cumple, permanece en el estado actual
        //return this;
    }


    //public void UpdateLoop()
    //{
    //    // Lógica de movimiento hacia el objetivo
    //    MoveTowardsTarget();
    //}

    //private void MoveTowardsTarget()
    //{
    //    if (_target != null)
    //    {
    //        // Mueve al agente hacia el objetivo
    //        _gAgent.transform.position = Vector3.MoveTowards(_gAgent.transform.position, _target.position, _gAgent.moveSpeed * Time.deltaTime);
    //    }
    //}

    //private bool ShouldAOAAttack()
    //{
    //    // Define la lógica para determinar si el agente debe realizar un ataque AOA
    //    return Vector3.Distance(_gAgent.transform.position, _target.position) < _gAgent.attackRange;
    //}

    //private bool ShouldWeaponAttack()
    //{
    //    // Define la lógica para determinar si el agente debe realizar un ataque con arma
    //    return _gAgent.HasWeapon && Vector3.Distance(_gAgent.transform.position, _target.position) < _gAgent.weaponRange;
    //}

    //private bool ShouldRest()
    //{
    //    // Define la lógica para determinar si el agente debe descansar
    //    return _gAgent.energyLevel < _gAgent.minEnergyLevel;
    //}

    //private bool ShouldIdle()
    //{
    //    // Define la lógica para determinar si el agente debe cambiar al estado idle
    //    return Vector3.Distance(_gAgent.transform.position, _target.position) <= _gAgent.minDistanceToTarget;
    //}

   /* •	Enter: Inicializa el objetivo de movimiento(_target) cuando el estado se activa.
•	Exit: Puede incluir lógica de limpieza o finalización cuando el estado se desactiva.
•	ProcessInput: Evalúa las condiciones para cambiar a otros estados (ShouldAOAAttack, ShouldWeaponAttack, ShouldRest, ShouldIdle). Si se cumple alguna condición, retorna el nuevo estado correspondiente.
•	UpdateLoop: Contiene la lógica de movimiento hacia el objetivo, utilizando MoveTowardsTarget.
   */
}
