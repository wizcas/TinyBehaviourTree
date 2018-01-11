/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cheers.BehaviourTree
{
    public class BTLogger
    {
        #region Static
        const string INDENT = "<color=grey>--</color>";
        public static bool Enabled = false;

        public static void BeginFrame(Node rootNode)
        {
            if (!Enabled)
                return;
            FindLogger(rootNode).DoBeginFrame(rootNode);
        }
        public static void EndFrame(Node rootNode)
        {
            if (!Enabled)
                return;
            FindLogger(rootNode).DoEndFrame();
        }
        public static void Log(Node node, string fmt, params object[] args)
        {
            if (!Enabled)
                return;
            FindLogger(node).DoLog(node, fmt, args);
        }

        static BTLogger FindLogger(Node node)
        {
            BTLogger logger;
            if (!mapNodeLoggers.TryGetValue(node, out logger))
            {
                if (node.parent != null)
                {
                    logger = FindLogger(node.parent);
                }
                if (logger == null)
                {
                    logger = new BTLogger();
                    mapNodeLoggers[node] = logger;
                }
            }
            return logger;
        }

        public static string PrintLastLog(Node node)
        {
            var logger = FindLogger(node);
            if (logger._frameLogs.Count == 0)
                return "(No BehaviourTree logs. Do you enabled the BTLogger?)";
            return logger._frameLogs[logger._frameLogs.Count - 1];
        }

        static Dictionary<Node, BTLogger> mapNodeLoggers = new Dictionary<Node, BTLogger>();

        #endregion

        #region Instanced
        List<string> _frameLogs = new List<string>();
        StringBuilder _frameSb;

        void DoBeginFrame(Node node)
        {
            if (_frameSb != null) DoEndFrame();

            _frameSb = new StringBuilder(string.Format("[===== {0} =====]\n", node.name));
        }

        string DoEndFrame()
        {
            var frameLog = _frameSb.ToString();
            _frameLogs.Add(frameLog);
            _frameSb = null;
            return frameLog;
        }

        string PrintIndent(Node node)
        {
            var indent = "";
            if (node.parent != null)
            {
                indent += INDENT + PrintIndent(node.parent);
            }
            return indent;
        }

        void DoLog(Node node, string fmt, params object[] args)
        {
            var log = string.Format("{5}<b><color=#{0}>[{1}]({2}) {3}</color></b> {4}",
                ColorUtility.ToHtmlStringRGB(node.EditorDarkColor), node.GetType().Name, node.id, node.name, string.Format(fmt, args), PrintIndent(node));
            _frameSb.AppendLine(log);
        }
        #endregion
    }
}