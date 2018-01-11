/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Cheers
{
    public struct EditorLabelStyle
    {
        const string LabelTextureName = "label.png";

        public int fontSize;
        public Color textColor;
        public Color backgroundColor;
        public RectOffset margin;
        public RectOffset padding;
        public TextAnchor alignment;
        public bool richText;

        public static Texture2D bgTexture {
            get {
                return EditorHelper.LoadEditorTexture(LabelTextureName);
            }
        }

        public static readonly Color DefaultBgColor = new Color(42 / 255f, 192 / 255f, 217 / 255f, .5f);

        public static EditorLabelStyle Default {
            get {
                return new EditorLabelStyle {
                    fontSize = 11,
                    textColor = Color.black,
                    backgroundColor = DefaultBgColor,
                    margin = new RectOffset(),
                    padding = new RectOffset(3, 3, 3, 3),
                    alignment = TextAnchor.MiddleCenter,
                    richText = true
                };
            }
        }

        public EditorLabelStyle (EditorLabelStyle src)
        {
            fontSize = src.fontSize;
            textColor = src.textColor;
            backgroundColor = src.backgroundColor;
            margin = new RectOffset(src.margin.left, src.margin.right, src.margin.top, src.margin.bottom);
            padding = new RectOffset(src.padding.left, src.padding.right, src.padding.top, src.padding.bottom);
            alignment = src.alignment;
            richText = src.richText;
        }


        public GUIStyle GetGUIStyle ()
        {
            var ret = new GUIStyle {
                normal = {
                    background = bgTexture,
                    textColor = textColor
                },
                margin = margin,
                padding = padding,
                alignment = alignment,
                border = new RectOffset(3, 3, 3, 3),
                fontSize = fontSize,
                wordWrap = true
            };
            return ret;
        }
    }
}