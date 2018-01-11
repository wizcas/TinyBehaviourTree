/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheers
{
    /// <summary>
    /// 非自动布局版本
    /// </summary>
    public static class EditorDrawer
    {
        public static void Label (Rect position, GUIContent content, EditorLabelStyle style)
        {
            #if UNITY_EDITOR
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = style.backgroundColor;
            var labelStyle = style.GetGUIStyle();
            GUI.Box(position, content, labelStyle);
            GUI.backgroundColor = oldBg;
            #endif
        }

        public static void Label (Rect position, string text, EditorLabelStyle style)
        {
            Label(position, new GUIContent(text), style);
        }

        public static void Label (Rect position, GUIContent content)
        {
            Label(position, content, EditorLabelStyle.Default);
        }

        public static void Label (Rect position, string text)
        {
            Label(position, new GUIContent(text), EditorLabelStyle.Default);
        }
    }

    /// <summary>
    /// 自动布局版本
    /// </summary>
    public static class EditorDrawerLayout
    {
        public static void Label (GUIContent content, EditorLabelStyle style)
        {
            #if UNITY_EDITOR
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = style.backgroundColor;
            var labelStyle = style.GetGUIStyle();
            GUILayout.Box(content, labelStyle);
            GUI.backgroundColor = oldBg;
            #endif
        }

        public static void Label (string text, EditorLabelStyle style)
        {
            Label(new GUIContent(text), style);
        }

        public static void Label (GUIContent content)
        {
            Label(content, EditorLabelStyle.Default);
        }

        public static void Label (string text)
        {
            Label(new GUIContent(text), EditorLabelStyle.Default);
        }
    }
}