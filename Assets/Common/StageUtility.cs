/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StageUtility
{
    /// <summary>
    /// 在测试位置上垂直查找地平面的信息，包括Ground层和Bridge层
    /// </summary>
    /// <param name="testPos"></param>
    /// <returns></returns>
    public static RaycastHit2D FindGroundSurface(Vector2 testPos)
    {
        var raycastPos = testPos + Vector2.up * 2;
        //Debug.DrawLine(raycastPos, raycastPos + Vector2.down * 2.1f, Color.magenta, 1f);
        return Physics2D.Raycast(raycastPos, Vector2.down, 2.1f, LayerMask.GetMask(Surface.Ground.ToString(), Surface.Bridge.ToString()));
    }
    /// <summary>
    /// 在测试位置上垂直查找地面位置坐标，包括Ground层和Bridge层
    /// </summary>
    /// <param name="testPos">测试位置</param>
    /// <param name="failedValue">没有找到时返回的失败值，若设置为null（默认）则返回 VectorEx.NaN2 </param>
    /// <returns>测试位置垂直方向上的地面位置；若没找到则返回指定的失败值，或Vector2的无效值NaN（若未设置失败值）（</returns>
    public static Vector2 FindGroundPosition(Vector2 testPos, Vector2? failedValue = null)
    {
        var hit = FindGroundSurface(testPos);
        if (hit.transform == null)
            return failedValue.HasValue ? failedValue.Value : VectorEx.NaN2;
        return hit.point;
    }

    public static RaycastHit2D FindWallSurface(Vector2 testPos, bool isRight)
    {
        var raycastPos = testPos;
        //Debug.DrawRay(raycastPos, Vector2.right, Color.gray, 1f);
        return Physics2D.Raycast(raycastPos, isRight ? Vector2.right : Vector2.left, .1f, LayerMask.GetMask(Surface.Ground.ToString(), Surface.Bridge.ToString()));
    }
}
