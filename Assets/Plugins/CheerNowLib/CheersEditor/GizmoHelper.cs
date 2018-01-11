/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cheers
{
    public static class GizmoHelper
    {
        const int smallFontSize = 9;
        const int normalFontSize = 14;
        const int largeFontSize = 16;

        static EditorLabelStyle defaultLabelStyle;
        readonly static Color defaultTextColor = Color.black;

        #if UNITY_EDITOR
        static GizmoHelper ()
        {
            defaultLabelStyle = new EditorLabelStyle(EditorLabelStyle.Default) {
                fontSize = normalFontSize,
                textColor = defaultTextColor
            };
        }
        #endif

        static GUIStyle SetLabelSize (GUIStyle guiStyle, LabelSize size)
        {
#if UNITY_EDITOR
            switch (size) {
            case LabelSize.Small:
                guiStyle.fontSize = smallFontSize;
                break;
            case LabelSize.Large:
                guiStyle.fontSize = largeFontSize;
                break;
            default:
                guiStyle.fontSize = normalFontSize;
                break;
            }
            return guiStyle;
#else
            return null;
#endif
        }

        public static void Label (Vector3 position, string text, Color? bgColor = null, Color? textColor = null, LabelSize size = LabelSize.Normal)
        {
#if UNITY_EDITOR
            var style = new EditorLabelStyle(defaultLabelStyle);
            if (bgColor.HasValue)
                style.backgroundColor = bgColor.Value;
            if (textColor.HasValue)
                style.textColor = textColor.Value;
            Label(position, text, style, size);
#endif
        }

        public static void Label (Vector3 position, string text, EditorLabelStyle style, LabelSize size = LabelSize.Normal)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(text))
                return;

            var oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = style.backgroundColor;
            GUIStyle guiStyle = SetLabelSize(style.GetGUIStyle(), size);
            Handles.Label(position, text, guiStyle);
            GUI.backgroundColor = oldBgColor;
#endif
        }

        public static void ArrowEnd (Vector3 pos, float size = .2f)
        {
            Gizmos.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size);
        }

        public static void Arrow (Vector3 pos, ArrowDirection dir, float size = .3f)
        {
            //    Vector3 upper = pos + new Vector3(size * (int)side, 0f, size);
            //    Vector3 lower = pos + new Vector3(size * (int)side, 0f, -size);

            Vector3 p0 = Vector3.zero, p1 = Vector3.zero;
            switch (dir) {
            case ArrowDirection.Up:
                p0 = new Vector3(-size, 0f, -size);
                p1 = new Vector3(size, 0f, -size);
                break;
            case ArrowDirection.Down:
                p0 = new Vector3(-size, 0f, size);
                p1 = new Vector3(size, 0f, size);
                break;
            case ArrowDirection.Left:
                p0 = new Vector3(size, 0f, -size);
                p1 = new Vector3(size, 0f, size);
                break;
            case ArrowDirection.Right:
                p0 = new Vector3(-size, 0f, -size);
                p1 = new Vector3(-size, 0f, size);
                break;
            }
            Gizmos.DrawLine(pos, pos + p0);
            Gizmos.DrawLine(pos, pos + p1);
        }

        public static Vector3 OffsetBySceneViewPos (Vector3 worldPoint, Vector3 offsetInScreenCoordinates)
        {
            Camera camera;
#if UNITY_EDITOR
            camera = SceneView.currentDrawingSceneView.camera;
#else
        camera = Camera.main;
#endif
            return camera.ScreenToWorldPoint(camera.WorldToScreenPoint(worldPoint) + offsetInScreenCoordinates);
        }

        public enum LabelSize
        {
            Small,
            Normal,
            Large
        }

        public enum Plane
        {
            X,
            Y,
            Z
        }

        public enum ArrowDirection
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}