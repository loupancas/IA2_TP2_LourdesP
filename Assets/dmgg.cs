using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dmgg : MonoBehaviour
{
    public int _dmg;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
}
