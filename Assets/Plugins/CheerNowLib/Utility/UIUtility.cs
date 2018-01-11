/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIUtility
{
    #region Compoents
    public static List<Graphic> GetAllGraphics(Transform root)
    {
        List<Graphic> list = new List<Graphic>();
        list.AddRange(root.GetComponents<Graphic>());
        foreach (Transform child in root)
        {
            list.AddRange(GetAllGraphics(child));
        }
        return list;
    }

    public static float[] GetOriginalAlphas(Graphic[] graphics)
    {
        var originalAlphas = new float[graphics.Length];
        for (int i = 0; i < originalAlphas.Length; i++)
        {
            originalAlphas[i] = graphics[i].color.a;
        }
        return originalAlphas;
    }
    #endregion

    #region UI Rect related
    /// <summary>
    /// 将from的坐标转换成to的坐标
    /// </summary>
    /// <returns>给to使用的AnchoredPosition</returns>
    /// <param name="from">要转换谁的坐标</param>
    /// <param name="to">要转换成谁的坐标</param>
    public static Vector2 CalcAnchoredPosOfRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 fromPivotOffset = new Vector2(
            from.rect.width * (.5f - from.pivot.x),
            from.rect.height * (.5f - from.pivot.y)
        );
        Vector2 toPivotOffset = new Vector2(
            to.rect.width * (.5f - to.pivot.x),
            to.rect.height * (.5f - to.pivot.y)
        );
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
        var toParent = to.parent as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(toParent, screenP, null, out localPoint);
        return localPoint + fromPivotOffset - toPivotOffset;
    }
    #endregion

    #region Event System

    /// <summary>
    /// 获取当前鼠标位置下的UI射线检测结果
    /// </summary>
    /// <returns>位于当前鼠标位置下的所有UI元素的射线检测结果</returns>
    public static RaycastResult[] RaycastUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.ToArray();
    }
    /// <summary>
    /// 检测当前是否在场景中某GameObject的子级UI上（例如检测是否在大地图某城市的信息UI上）
    /// </summary>
    /// <param name="parentGo">所属的GameObject</param>
    /// <param name="callback">若在指定GameObject下的UI上则返回TRUE，否则返回FALSE</param>
    /// <param name="isTopOnly">是否只检测排在顶层的对象</param>
    public static bool IsOnGameObjectUI(GameObject parentGo, bool isTopOnly)
    {
        var results = RaycastUI();
        if (results.Length == 0)
            return false;
        if (isTopOnly)
        {
            return IsHitOfParent(results[0], parentGo);
        }
        else
        {
            foreach (var result in results)
            {
                if (IsHitOfParent(result, parentGo))
                {
                    return true;
                }
            }
            return false;
        }
    }
    public static bool IsHitOfParent(RaycastResult hit, GameObject parentGo)
    {
        return hit.isValid && hit.gameObject.transform.IsChildOf(parentGo.transform);
    }
    #endregion

    #region Text Animation
    /// <summary>
    /// 以每个数字跳动的时长为准播放数字动画
    /// </summary>
    /// <returns>总动画时长</returns>
    /// <param name="digitSprite">数字控件</param>
    /// <param name="value">要更新到的值</param>
    /// <param name="color">渐变色</param>
    /// <param name="duraton">最终动画时长</param> 
    /// <param name="onComplete">动画完成时的回调</param>
    /// <param name="tickDuration">数字+1或-1的耗时</param>
    /// <param name="maxDuration">整个动画最长耗时，为了避免动画太长</param>
    public static Tween PlayNumberAnimationByTicks(Text text, long value, Color color,
        out float duration,
        System.Action onComplete = null,
        float tickDuration = .01f, float maxDuration = 2f)
    {
        text.color = color;
        long curVal;
        if (!long.TryParse(text.text, out curVal))
        {
            curVal = 0;
        }
        var delta = value - curVal;
        if (delta == 0)
        {
            text.text = value.ToString();
            duration = 0;
            return null;
        }

        duration = Mathf.Abs(delta) * tickDuration;
        duration = Mathf.Min(duration, maxDuration);

        return PlayNumberAnim(text, value, duration, onComplete);
    }

    /// <summary>
    /// 按固定时长播放数字动画
    /// </summary>
    /// <returns>总动画时长，等于fixedDuration</returns>
    /// <param name="digitSprite">数字控件</param>
    /// <param name="value">要更新到的值</param>
    /// <param name="color">渐变色</param>
    /// <param name="onComplete">动画完成时的回调</param>
    /// <param name="fixedDuration">总动画时长</param>
    public static Tween PlayNumberAnimationInFixedTime(Text text, long value, Color color,
        out float duration,
        System.Action onComplete = null, float fixedDuration = 2f)
    {
        text.color = color;
        duration = fixedDuration;
        return PlayNumberAnim(text, value, fixedDuration, onComplete);
    }

    static Tween PlayNumberAnim(Text text, long endValue, float duration, System.Action onComplete)
    {
        return DOTween.To(() =>
        {
            long val;
            if (long.TryParse(text.text, out val))
                return val;
            return 0L;
        }, v => text.text = v.ToString(), endValue, duration)
            .OnComplete(() =>
            {
                text.text = endValue.ToString();
                if (onComplete != null)
                    onComplete();
            });
    }
    #endregion
}