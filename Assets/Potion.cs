/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Potion : MonoBehaviour
{
    public float speed = 15;
    public float tossAngle = 20f;
    public float plantAngle = 0f;
    [SerializeField] SpriteRenderer _renderer;
    
    public void Break(MonoBehaviour upon)
    {
        SpriteExploder.Explode(this);
        AfterBreak(upon);

        Destroy(gameObject);
    }

    public static Potion Make(int id, Vector3 pos, bool isActive)
    {
        //var potion = Pooly.Spawn<Potion>(PoolName.Potion, pos, Quaternion.identity, null);
        Potion potion = null;
        if (potion == null) return null;
        potion.gameObject.SetActive(isActive);
        return potion;
    }

    void AfterBreak(MonoBehaviour upon)
    {
        
    }
    
    public void Toss()
    {
        var dir = Quaternion.Euler(0, 0, tossAngle)
            * Vector2.right;
        GetComponent<Rigidbody2D>().AddForce(dir * speed, ForceMode2D.Impulse);
        PrettyLog.Log("toss");
    }
}