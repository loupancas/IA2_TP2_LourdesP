using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    private BaseEnemy _lifeBar, _fatigueBar;
    public Image lifeUI;
    public Image fatigueUI;
    
    private int actualLife;
    private int actualFatigue;

    private void Awake()
    {
        //if (_lifeBar == null)
        //{
        //    _lifeBar = BaseEnemy.Instance;
        //}
        //if (_fatigueBar == null)
        //{
        //    _fatigueBar = BaseEnemy.Instance;
        //}
        //actualLife =BaseEnemy.Instance.ActualLife;
        //actualFatigue = BaseEnemy.Instance.ActualFatigue;
        //StartCoroutine(LifeCor());
        //StartCoroutine(FatigueCor());

    }

    private void Start()
    {
        
        StartCoroutine(LifeCor());
        StartCoroutine(FatigueCor());
    }




    IEnumerator LifeCor()
    {
        yield return new WaitForSeconds(0.2f);
        _lifeBar = BaseEnemy.Instance;
        if (_lifeBar != null)
        {
            _lifeBar.OnLifeChange += ChangeLifeUI;
            actualLife = _lifeBar.ActualLife;
        }
        else
        {
            Debug.LogError("BaseEnemy instance is null in LifeCor");
        }



    }

    IEnumerator FatigueCor()
    {
        yield return new WaitForSeconds(0.2f);
        _fatigueBar = BaseEnemy.Instance;
        if (_fatigueBar != null)
        {
            _fatigueBar.OnFatigueChange += ChangeFatigueUI;
            actualFatigue = _fatigueBar.ActualFatigue;
        }
        else
        {
            Debug.Log("BaseEnemy instance is null in FatigueCor");
        }
    }


    private void ChangeLifeUI()
    {
        if (_lifeBar != null)
        {
            actualLife = _lifeBar.ActualLife;
            lifeUI.fillAmount = (float)actualLife / _lifeBar.initialLife;
        }

    }

    private void ChangeFatigueUI()
    {
        if (_fatigueBar != null)
        {
            actualFatigue = _fatigueBar.ActualFatigue;
            fatigueUI.fillAmount = (float)actualFatigue / _fatigueBar.initialFatigue;
        }
    }


}
