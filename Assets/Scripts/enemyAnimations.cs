using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAnimations : MonoBehaviour
{
   

    public Animator animator;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            animator.SetTrigger("attack");
        if (Input.GetKeyDown(KeyCode.Alpha2))
            animator.SetTrigger("2");
        if (Input.GetKeyDown(KeyCode.Alpha3))
            animator.SetTrigger("3");
        if (Input.GetKeyDown(KeyCode.Alpha4))
            animator.SetTrigger("4");
        if (Input.GetKeyDown(KeyCode.Alpha5))
            animator.SetTrigger("5");
    }
}

