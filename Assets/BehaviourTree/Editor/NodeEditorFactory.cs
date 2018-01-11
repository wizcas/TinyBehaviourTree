/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Cheers.BehaviourTree.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomNodeEditorAttribute : Attribute
    {
        public Type nodeType;
        public CustomNodeEditorAttribute(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }

    public static class NodeEditorFactory
    {
        static Dictionary<Type, NodeEditor> _mapNodeToEditor = new Dictionary<Type, NodeEditor>();
        static NodeEditorFactory()
        {
            LoadNodeEditorTypes();
        }

        static void LoadNodeEditorTypes()
        {
            _mapNodeToEditor.Clear();
            var editorTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(NodeEditor)) && !t.IsAbstract);
            if (editorTypes == null) return;
            foreach (var type in editorTypes)
            {
                var attr = type.GetAttribute<CustomNodeEditorAttribute>(false);
                if (attr == null) continue;
                _mapNodeToEditor[attr.nodeType] = (NodeEditor)Activator.CreateInstance(type);
            }
        }

        public static NodeEditor GetEditor(Type nodeType)
        {
            NodeEditor result;
            if (!_mapNodeToEditor.TryGetValue(nodeType, out result))
            {
                return null;
            }
            return result;
        }
    }

    [PrettyLog.Provider("NodeEditor", ModuleColor = "magenta")]
    public abstract class NodeEditor
    {
        bool ValidateType(Node node)
        {
            var nodeType = node.GetType();
            var selfType = GetType();
            var attr = selfType.GetAttribute<CustomNodeEditorAttribute>(false);
            if (attr == null)
            {
                PrettyLog.Error("{0} has no CustomNodeEditor attribute", selfType);
                return false;
            }
            if (nodeType != attr.nodeType)
            {
                PrettyLog.Error("{0} can only be used as {1}'s editor ({2} given)", selfType, attr.nodeType, nodeType);
                return false;
            }
            return true;
        }

        public void OnGUI(Rect rect, Node node)
        {
            if (!ValidateType(node)) return;
            DrawHeader(rect, node);
        }
        
        void DrawHeader(Rect rect, Node node)
        {
            GUI.backgroundColor = ColorHelper.ByObject(node.DisplayName);
            GUI.Box(rect, string.Format("[{0}]{1}", node.id, node.name));
        }
    }
}