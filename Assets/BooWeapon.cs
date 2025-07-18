using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooWeapon : MonoBehaviour
{
    [SerializeField]
    int _dmg;

    public MeshRenderer _meshRenderer;
    public Collider _collider;
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
        _collider = GetComponent<Collider>();
        _collider.enabled = false;
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<FirstPersonPlayer>() != null)
        {
            Debug.Log("dmg");
            collision.collider.GetComponent<FirstPersonPlayer>().TakeDamage(_dmg);
           
        }

     

    }

    private void OnTriggerEnter(Collider other)
    {
        if (GetComponent<FirstPersonPlayer>() != null)
        {
            Debug.Log("dmg");
            GetComponent<FirstPersonPlayer>().TakeDamage(_dmg);

        }
    }
}
