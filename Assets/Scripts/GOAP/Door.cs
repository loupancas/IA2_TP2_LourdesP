using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    bool doorOpen;
    Rigidbody rb;
    public bool hasBeenOpened = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        doorOpen = false;
    }
    void OnCollisionEnter(Collision collision)
    {/*
        if (collision.gameObject.GetComponent<Guy>() != null)
        {
            Guy.Instance.canMove = false;

            StartCoroutine(Guy.Instance.DelayedAnimation("open", 2f, () =>
            {
                Deactivate();
                Guy.Instance.canMove = true;
            }));
        }*/
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public bool open
    {
        get { return doorOpen; }
        set { doorOpen = value; }
    }
}
