/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using UnityEditor;
using UnityEngine;

namespace Cheers
{
    [CustomPropertyDrawer(typeof(Help))]
    public class HelpDrawer : DecoratorDrawer
    {
        static EditorLabelStyle _labelStyle = new EditorLabelStyle(EditorLabelStyle.Default) {
            alignment = TextAnchor.MiddleLeft,
            textColor = ColorHelper.ByWeb("#222222"),
            backgroundColor = ColorHelper.ByRGBA(200, 200, 200, 200),
            padding = new RectOffset(10, 10, 3, 3)
        };
        static Texture2D referIcon = EditorHelper.LoadEditorTexture("refer.png");

        GUIStyle _guiStyle = _labelStyle.GetGUIStyle();
        Vector2 _helpSize;

        Help attr {
            get { return (Help)attribute; }
        }

        GUIContent content {
            get {
                return new GUIContent(attr.Text);
            }
        }

        public override float GetHeight ()
        {
            _helpSize = CalcHelpSize();
            var spacing = EditorGUIUtility.standardVerticalSpacing * 2;
            return attr.IsExpanded ? _helpSize.y + spacing : 0;
        }

        public override void OnGUI (Rect position)
        {
            //Draw Toggle
            var toggleRect = position;
            toggleRect.width = 14;
            toggleRect.height = 14;
            toggleRect.x -= toggleRect.width;
            var e = Event.current;
            if (toggleRect.Contains(e.mousePosition)) {
                if (e.type == EventType.MouseUp) {
                    attr.IsExpanded = !attr.IsExpanded;
                }
                if (EditorWindow.focusedWindow != null && 
                    EditorWindow.focusedWindow.titleContent.text == "Inspector") {
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            Texture2D tex = attr.IsExpanded ? EditorHelper.icons.infoHighlight : EditorHelper.icons.info;
            GUI.DrawTexture(toggleRect, tex);

            // Draw help
            var expandRect = EditorGUI.IndentedRect(position);
            expandRect.height = _helpSize.y;
            if (attr.IsExpanded) {
                DrawLabel(expandRect);
            }
        }

        void DrawLabel (Rect position)
        {
            var rect = position;
            GUI.BeginGroup(rect);
            var drawPos = new Rect(0, 0, rect.width, rect.height);
            drawPos.xMin += 10f;
            var referIcon = EditorHelper.LoadEditorTexture("refer.png");
            var referRect = drawPos;
            referRect.width = referIcon.width;
            referRect.yMin = referRect.height - referIcon.height;
            GUI.DrawTexture(referRect, referIcon);
            drawPos.xMin += referRect.width + 1f;
            EditorDrawer.Label(drawPos, content, _labelStyle);
            GUI.EndGroup();
        }

        Vector2 CalcHelpSize ()
        {
            var w = EditorGUIUtility.currentViewWidth
                    - EditorStyles.inspectorDefaultMargins.padding.horizontal
                    - referIcon.width - _guiStyle.padding.horizontal - _guiStyle.margin.horizontal;
            var h = _guiStyle.CalcHeight(content, w);
            return new Vector2(w, h);
        }
    }
}