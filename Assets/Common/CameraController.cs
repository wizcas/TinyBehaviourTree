/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{
    public Transform target;
    public Vector3 offset2Target = VectorEx.NaN3;
    public float panSpeed = 5;

    private void Start()
    {
        if (target != null)
        {
            if (VectorEx.IsNaN(offset2Target))
            {
                SetOffsetAsCurrent();
            }
            MoveCameraInPosition();
        }
    }

    private void Update()
    {
        if (target != null)
        {
            var nextPos = target.position + offset2Target;
            transform.position = Vector3.Lerp(transform.position, nextPos, panSpeed * Time.deltaTime);
        }
    }

    [See]
    void SetOffsetAsCurrent()
    {
        offset2Target = transform.position - target.position;
    }

    [See]
    void MoveCameraInPosition()
    {
        transform.position = target.position + offset2Target;
    }
}
