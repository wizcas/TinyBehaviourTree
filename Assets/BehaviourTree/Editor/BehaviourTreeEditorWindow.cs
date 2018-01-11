/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cheers.BehaviourTree.Editor
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        bool _hasTarget;
        bool _showFrameResult;
        BehaviourTree _tree;
        Vector2 _pan;
        BehaviourTreeData _treeData;
        GUIStyle _wndStyle;

        static Texture2D BackgroundTex
        {
            get
            {
                return Resources.Load<Texture2D>("EditorGridBackground");
            }
        }
        static Texture2D NodeBackgroundTex
        {
            get { return Resources.Load<Texture2D>("NodeEditorBackground"); }
        }

        Rect CanvasRect
        {
            get
            {
                return new Rect(0, 16, position.width, position.height - 16);
            }
        }

        Rect CanvasLocalRect
        {
            get
            {
                return new Rect(Vector2.zero, CanvasRect.size);
            }
        }

        [MenuItem("Window/Behaviour Tree Editor")]
        static void ShowEditor()
        {
            BehaviourTreeEditorWindow editor = GetWindow<BehaviourTreeEditorWindow>("Behaviour Tree");
            editor.Init();
        }

        public void Init()
        {
        }

        void CheckTreeAvailablility()
        {
            _treeData = null;
            if (Selection.activeObject is BehaviourTreeData)
            {
                _treeData = Selection.activeObject as BehaviourTreeData;
            }
            else if (Selection.activeGameObject != null)
            {
                _tree = Selection.activeGameObject.GetComponent<BehaviourTree>();
                if (_tree != null)
                {
                    _treeData = _tree.treeData;
                    _showFrameResult = true;
                    UnregisterTreeUpdateCallback();
                    _tree.onTreeUpdated += OnTreeUpdated;
                }
            }
            _hasTarget = _treeData != null;
        }

        void OnTreeUpdated()
        {
            Repaint();
        }

        void UnregisterTreeUpdateCallback()
        {
            if (_tree != null)
            {
                _tree.onTreeUpdated -= OnTreeUpdated;
            }
        }

        private void OnSelectionChange()
        {
            UnregisterTreeUpdateCallback();
            CheckTreeAvailablility();
        }

        private void OnEnable()
        {
            CheckTreeAvailablility();
        }

        private void OnDisable()
        {
            UnregisterTreeUpdateCallback();
        }

        private void OnFocus()
        {
            CheckTreeAvailablility();
        }

        void OnGUI()
        {
            MakeWndStyle();
            DrawBackground();
            DrawTree();
            DrawToolbar();
            Pan();
        }

        void MakeWndStyle()
        {
            _wndStyle = new GUIStyle();
            _wndStyle.border = new RectOffset(6, 6, 34, 1);
            _wndStyle.padding = new RectOffset(5, 5, 5, 5);
            _wndStyle.richText = true;
            _wndStyle.alignment = TextAnchor.UpperCenter;
            _wndStyle.normal.background = NodeBackgroundTex;
            _wndStyle.normal.textColor = ColorHelper.ByWeb("#222");
            _wndStyle.focused.background = NodeBackgroundTex;
            _wndStyle.hover.background = NodeBackgroundTex;
            _wndStyle.active.background = NodeBackgroundTex;
        }

        void Pan()
        {
            var ev = Event.current;
            if (_hasTarget && ev.type == EventType.MouseDrag && ev.button == 2)
            {
                _pan += ev.delta;
                Repaint();
            }
        }

        void DrawBackground()
        {
            var texWidth = BackgroundTex.width;
            var texHeight = BackgroundTex.height;
            var uvX = CanvasRect.width / texWidth;
            var uvY = CanvasRect.height / texHeight;
            var uvRect = new Rect(
                -_pan.x / texWidth,
                _pan.y / texWidth - uvY,
                uvX,
                uvY);
            GUI.DrawTextureWithTexCoords(CanvasRect, BackgroundTex, uvRect);
        }

        void DrawToolbar()
        {
            // toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginDisabledGroup(!_hasTarget);

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                _treeData.SaveAsset();
            }
            if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                _treeData.LoadAsset();
            }
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                _treeData.rootNode.Clear();
            }
            if (GUILayout.Button("Load Clipboard", EditorStyles.toolbarButton))
            {
                _treeData.DeserializeFromClipboard();
            }
            if (GUILayout.Button("Print Last Log", EditorStyles.toolbarButton))
            {
                PrettyLog.Log(BTLogger.PrintLastLog(_treeData.rootNode));
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Pan To (0,0)", EditorStyles.toolbarButton))
            {
                _pan = Vector2.zero + CanvasRect.size * .5f;
            }

            GUILayout.FlexibleSpace();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        void DrawTree()
        {

            if (!_hasTarget)
            {
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = ColorHelper.ByRGBA(200, 200, 200, 150);
                var rect = CanvasRect;
                rect.width = 300;
                rect.height = 100;
                rect.x = (CanvasRect.width - rect.width) * .5f;
                rect.y = (CanvasRect.height - rect.height) * .5f;
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                };
                GUI.Box(rect, "选中的对象上没有行为树\n<i><size=14>当然也就没法编辑了</size></i>", style);
                GUI.backgroundColor = oldBg;
                return;
            }

            if (_treeData == null) return;

            GUI.BeginClip(CanvasRect);
            // draw windows
            BeginWindows();
            DrawNodeRecrusively(_treeData.rootNode, null, 0, 0, _tree._frameResult);
            EndWindows();
            GUI.EndClip();
        }

        Dictionary<int, Node> mapNodes = new Dictionary<int, Node>();

        Rect GetNodeWnd(Node node)
        {
            var wndRect = node._editorRect;
            wndRect.position += _pan;
            return wndRect;
        }
        void SetNodeRect(Node node, Rect nodeWnd)
        {
            node._editorRect = nodeWnd;
            node._editorRect.position -= _pan;
        }

        Rect DrawNodeRecrusively(Node node, Node parent, int siblingIndex, int depth, NodeResult? frameResult)
        {
            if (node == null)
                return Rect.zero;

            // Get node's rect
            mapNodes[node.id] = node;
            if (node._editorRect == Rect.zero)
            {
                node._editorRect = ComputeSuggestedNodeRect(parent, siblingIndex, depth == 0);
            }
            var wnd = GetNodeWnd(node);
            // Draw Window
            var oldbg = GUI.backgroundColor;
            GUI.backgroundColor = node.EditorColor;
            wnd = GUI.Window(node.id, wnd, DrawNodeWindow,
                new GUIContent(string.Format("<i><color=#333>{0}</color></i>\n({1}) {2}", node.GetType().Name, node.id, node.name)),
                _wndStyle);
            GUI.backgroundColor = oldbg;
            // Set new rect back to node
            SetNodeRect(node, wnd);

            // Check if this node accessed. If so, draw its result status
            IEnumerable<NodeResult> childResults = null;
            if (_showFrameResult && frameResult.HasValue && frameResult.Value.node != null)
            {
                childResults = frameResult.Value.childResults;
                var resultRect = new Rect(wnd.center.x, wnd.min.y - 19, 70, 20);
                var style = new GUIStyle("box");
                style.normal.textColor = Color.white;
                oldbg = GUI.backgroundColor;
                GUI.backgroundColor = frameResult.Value.StatusColor;
                GUI.Box(resultRect, frameResult.Value.state.ToString(), style);
                GUI.backgroundColor = oldbg;
            }
            // Draw linking curves
            var index = 0;
            foreach (var child in node.children)
            {
                var childFrameResult = childResults == null ? null : (NodeResult?)childResults.FirstOrDefault(r => r.node == child);
                bool hasChildResult = childFrameResult.HasValue && childFrameResult.Value.node != null;
                var childWnd = DrawNodeRecrusively(child, node, index, depth + 1, childFrameResult);
                DrawNodeCurve(wnd, childWnd, hasChildResult ? Color.yellow : Color.white, hasChildResult ? 4 : 2);
                index++;
            }
            return wnd;
        }
        const float depthSpacing = 50;
        const float siblingSpacing = 50;
        readonly Vector2 defaultSize = new Vector2(150, 100);
        Rect ComputeSuggestedNodeRect(Node parentNode, int index, bool isRoot)
        {
            var rect = new Rect(Vector2.zero, defaultSize);
            if (isRoot) return rect;

            var parentRect = parentNode._editorRect;
            var siblingCount = parentNode.children.Count;

            rect.y = parentRect.yMax + depthSpacing;
            var totalWidth = siblingCount * defaultSize.x + (siblingCount - 1) * siblingSpacing;
            var startOffsetX = parentRect.x + parentRect.width * .5f - totalWidth * .5f;
            rect.x = startOffsetX + index * (defaultSize.x + siblingSpacing);
            return rect;
        }

        void DrawNodeWindow(int id)
        {
            var node = mapNodes[id];
            var wnd = GetNodeWnd(node);
            var editorRect = new Rect(5, 38, wnd.size.x - 10, wnd.size.y - 43);
            var editorLocalRect = new Rect(Vector2.zero, editorRect.size);
            GUI.BeginClip(editorRect);
            GUI.Box(editorLocalRect, "");

            GUI.EndClip();

            if (Event.current.button == 0)
            {
                GUI.DragWindow();
            }
        }

        void DrawNodeCurve(Rect start, Rect end, Color color, float width)
        {
            Vector3 startPos = new Vector3(start.center.x, start.max.y, 0);
            Vector3 endPos = new Vector3(end.center.x, end.y, 0);
            Vector3 startTan = startPos + Vector3.up * 50;
            Vector3 endTan = endPos + Vector3.down * 50;
            Color shadowCol = new Color(0, 0, 0, 0.2f);
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, width);
        }

    }
}