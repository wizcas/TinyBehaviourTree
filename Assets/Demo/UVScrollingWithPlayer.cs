/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UVScrollingWithPlayer : MonoBehaviour 
{
    public PlayerMovement playerMover;
    public float scaleToSpeed = 600f;
    Material _mat;
    Vector2 _offset;
    private void Start()
    {
        _mat = GetComponent<SpriteRenderer>().material;
        _offset = _mat.GetTextureOffset("_MainTex");
    }
    private void Update()
    {
        _mat.SetTextureOffset("_MainTex", _offset);
        _offset += playerMover.FrameDeltaPos / scaleToSpeed;
    }
}
