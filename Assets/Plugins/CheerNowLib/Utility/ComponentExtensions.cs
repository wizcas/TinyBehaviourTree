/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ComponentExtensions
{
    public static void DestroyChildren (this Transform component)
    {
        foreach (Transform trans in component) {
            if (Application.isPlaying) {
                Object.Destroy(trans.gameObject);
            }
        }
    }

    public static T[] GetComponentsInAllChildren<T> (this Component parent, bool includeSelf = false)
        where T: Component
    {
        List<T> ret = new List<T>();
        if (includeSelf) {
            var c = parent.GetComponent<T>();
            if (c != null)
                ret.Add(c);
        }
        
        foreach (Transform child in parent.transform) {
            ret.AddRange(child.GetComponentsInAllChildren<T>(true));
        }
        return ret.ToArray();
    }
}
