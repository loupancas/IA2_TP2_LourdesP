using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossWeapon : MonoBehaviour
{
    MeshRenderer _Gun;
    [SerializeField] private GameObject weaponObject;
    void Start()
    {
        
    }

   
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
       
            _Gun = weaponObject.GetComponent<MeshRenderer>();
            _Gun.enabled = false;
        
    }
}
