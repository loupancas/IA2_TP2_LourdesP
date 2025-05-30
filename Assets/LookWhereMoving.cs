using UnityEngine;

[RequireComponent(typeof(Entidad))]
public class LookWhereMoving : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public Entidad entidad;
    private Vector3 lastPosition;
    

    void Start()
    {

        entidad = GetComponent<Entidad>();
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 moveDir = currentPosition - lastPosition;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            // Solo consideramos la dirección horizontal para evitar inclinaciones en Y
            moveDir.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        lastPosition = currentPosition;
    }
}