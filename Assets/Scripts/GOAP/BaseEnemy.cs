using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseEnemy : MonoBehaviour
{
    public int initialLife;
    private int _actualLife;
    public int ActualLife => _actualLife;
    public int initialFatigue;
    protected int _actualFatigue;
    public int ActualFatigue => _actualFatigue;
   
    public delegate void UpdateLifeBar();
    public event UpdateLifeBar OnLifeChange;
    public delegate void UpdateFatigueBar();
    public event UpdateFatigueBar OnFatigueChange;
    public static BaseEnemy Instance { get; private set; }

    protected void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _actualFatigue = initialFatigue;
        _actualLife = initialLife;
    }

  

    public void TakeDamage(int damage)
    {
        _actualLife -= damage;
        _actualLife = Mathf.Clamp(_actualLife, 0, initialLife);
        OnLifeChange?.Invoke();
        if (_actualLife <= 0)
        {
            Morir();
        }
    }

    public void Fatigarse(int fatigue)
    {
        _actualFatigue -= fatigue;
        _actualFatigue = Mathf.Clamp(_actualFatigue, 0, initialFatigue)  ;
        OnFatigueChange?.Invoke();
        if (_actualFatigue <= 0)
        {
            Fatigado();
        }
    }

    public void Morir()
    {
       //muerto
    }

    public void Fatigado()
    {
        //fatigado
    }
}
