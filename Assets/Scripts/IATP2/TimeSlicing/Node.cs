using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbour = new List<Node>();
    public int heuristic = 1;
    public bool isPath;
    public float detectionRadius = 5.0f; // Radio ajustable desde el Inspector

    private void Awake()
    {
        // Debug.Log("Nodo " + gameObject.name + " despertando. Posición: " + transform.position);
        neighbour = Physics.OverlapSphere(transform.position, detectionRadius)
            .Select(x => x.GetComponent<Node>())
            .Where(x => x != null)
            .Where(x => x.gameObject != gameObject)
            .ToList();

        foreach (var n in neighbour)
        {
            // Debug.Log(gameObject.name + " tiene vecino: " + n.gameObject.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isPath ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.blue;
        foreach (var neighbourNode in neighbour)
        {
            Gizmos.DrawLine(transform.position, neighbourNode.transform.position);
        }
    }
}
