/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Cheers.BehaviourTree
{
    [CreateAssetMenu(fileName = "TreeData", menuName = "BehaviourTree Data", order = 0)]
    public class BehaviourTreeData : ScriptableObject, ISerializationCallbackReceiver
    {
        public Node rootNode;
        [SerializeField, HideInInspector] string _serializedData;

        void SaveTree()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        [See]
        public void SaveAsset()
        {
            Serialize();
            SaveTree();
            PrettyLog.Log("<color=green>{0} is saved. Data size: {1}</color>", name, _serializedData.Length);
        }

        [See]
        public void LoadAsset()
        {
            Deserialize();
            PrettyLog.Log("<color=maroon>{0} is loaded. Data size: {1}</color>", name, _serializedData.Length);
        }

        [See]
        public void Serialize()
        {
            if (rootNode == null)
            {
                _serializedData = string.Empty;
            }
            else
            {
                _serializedData = rootNode.Serialize();
            }
        }

        [See]
        public void SerializeAndCopy()
        {
            Serialize();
            GUIUtility.systemCopyBuffer = _serializedData;
        }

        [See]
        public void Deserialize()
        {
            rootNode = Node.Deserialize(_serializedData);
        }

        [See]
        public void DeserializeFromClipboard()
        {
            var data = GUIUtility.systemCopyBuffer;
            try
            {
                _serializedData = data;
                Deserialize();
                PrettyLog.Log("<color=maroon>{0} is loaded. Data size: {1}</color>", name, _serializedData.Length);
            }
            catch (JsonReaderException ex)
            {
                PrettyLog.Error("<color=red>Load failed: Invalid behaviour tree data</color>\n<color=blue>Exception:</color>\n{0}\n\n<color=blue>Data</color>\n{1}", ex, data);
            }
        }

        public void OnBeforeSerialize()
        {
            Serialize();
        }

        public void OnAfterDeserialize()
        {
            Deserialize();
        }
    }
}
