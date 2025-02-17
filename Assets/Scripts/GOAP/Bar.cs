using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    public int initial;
    protected int _actual;

    public int ActualAmount => _actual;

    private void OnEnable()
    {
        _actual = initial;
    }

    public virtual void TakeAmount(int damage)
    {
        _actual -= damage;
    }
}
