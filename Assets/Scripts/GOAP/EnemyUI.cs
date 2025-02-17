using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyUI : MonoBehaviour
{
    private BaseEnemy _lifeBar, _fatigueBar;
    public Image lifeUI;
    public Image fatigueUI;
    
    private int actualLife;
    private int actualFatigue;

    private void Awake()
    {
        StartCoroutine(LifeCor());
        StartCoroutine(FatigueCor());

    }

    IEnumerator LifeCor()
    {
        yield return new WaitForSeconds(0.1f);
        _lifeBar = BaseEnemy.Instance;
        _lifeBar.OnLifeChange += ChangeLifeUI;
        actualLife = _lifeBar.ActualLife;
       
    }

    IEnumerator FatigueCor()
    {
        yield return new WaitForSeconds(0.1f);
        _fatigueBar = BaseEnemy.Instance;
        _fatigueBar.OnFatigueChange += ChangeFatigueUI;
        actualFatigue = _fatigueBar.ActualFatigue;
    }


    private void ChangeLifeUI()
    {
        lifeUI.fillAmount = (float)actualLife / _lifeBar.initialLife;

    }

    private void ChangeFatigueUI()
    {
        fatigueUI.fillAmount = (float)actualFatigue / _fatigueBar.initialFatigue;
    }


}
