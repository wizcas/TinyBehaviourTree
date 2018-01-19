/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cheers.BehaviourTree
{
    public enum NodeState
    {
        Invalid = 0,
        Running,
        Finished
    }
    public enum NodeRunningState
    {
        Invalid = 0,
        Running,
        Finish
    }

    [Serializable]
    public struct NodeResult
    {
        public Node node;
        public NodeResult[] childResults;

        public NodeState State
        {
            get { return node == null ? NodeState.Invalid : node.state; }
        }

        public NodeResult(Node node)
        {
            this.node = node;
            childResults = null;
        }

        public NodeResult? FindResult(Func<NodeResult, bool> predicate)
        {
            if (predicate(this))
                return this;

            if(childResults != null)
            {
                foreach(var cr in childResults)
                {
                    var ret = cr.FindResult(predicate);
                    if (ret.HasValue) return cr;
                }
            }
            return null;
        }


        #region Formatting & Outputing
        public string StatusColorWebString
        {
            get
            {
                switch (State)
                {
                    case NodeState.Running:
                        return "#1a7010";
                    case NodeState.Finished:
                        return "#8f1919";
                    default:
                        return "grey";
                }
            }
        }

        public Color StatusColor
        {
            get
            {
                return ColorHelper.ByWeb(StatusColorWebString);
            }
        }

        public string PrettyStatus
        {
            get
            {
                var colorFmt = "<color={0}>{1}</color>";
                return string.Format(colorFmt, StatusColorWebString, State);
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0}: {1}", node == null ? "(No node)" : node.DisplayName, PrettyStatus);
            if(childResults != null)
            {
                foreach(var cr in childResults)
                {
                    sb.AppendFormat("\n\t{0}", cr.ToString());
                }
            }
            return sb.ToString();
        }
        #endregion
    }

    public interface IOrder
    {

    }
    
    [Serializable]
    public abstract class Node
    {
        static int NODE_COUNT = 0;

        public int id;
        public string __type;        
        public Precondition precondition;
        public string name;
        [NonSerialized]
        public Node parent;
        public List<Node> children = new List<Node>();

        #region Runtime Variables
        [JsonIgnore]
        public NodeState state { get; private set; }
        #endregion

        #region Editor Use
        public Rect _editorRect;

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                return string.Format("[{0}]({1}) {2}", GetType().Name, id, name);
            }
        }
        [JsonIgnore]
        public abstract bool IsLeaf { get; }
        [JsonIgnore]
        public abstract Color EditorColor { get; }
        [JsonIgnore]
        public Color EditorDarkColor
        {
            get {
                float h, s, v;
                Color.RGBToHSV(EditorColor, out h, out s, out v);
                v -= .15f;
                return Color.HSVToRGB(h, s, v);
            }
        }
        #endregion

        protected static T MakeNode<T>(string name, Precondition precondition) where T: Node, new()
        {
            var node = new T
            {
                name = name,
                precondition = precondition
            };
            if (node.precondition != null)
            {
                node.precondition.node = node;
            }
            return node;
        }

        public Node()
        {
            id = NODE_COUNT++;
        }

        public void UpdateParent(Node parent)
        {
            this.parent = parent;
            foreach(var child in children)
            {
                if(child.parent != this)
                {
                    child.UpdateParent(this);
                }
            }
        }

        public Node AddNodes(params Node[] newChildren)
        {
            if (IsLeaf)
            {
                PrettyLog.Error("{0} is a leaf node that can't hold any child", DisplayName);
            }
            if (newChildren == null || newChildren.Length == 0) return this;

            for (int i = 0; i < newChildren.Length; i++)
            {
                var newChild = newChildren[i];
                newChild.UpdateParent(this);
                children.Add(newChild);
            }
            return this;
        }

        public void RemoveNode(Node child, bool destroyChildren = false)
        {
            var succ = children.Remove(child);
            if (succ && destroyChildren)
                child.Clear();
        }

        public void Clear(bool destroyChildren = true)
        {
            if (destroyChildren)
            {
                foreach (var child in children)
                {
                    if (child == null) continue;
                    child.Clear();
                }
            }
            children.Clear();
        }

        protected void SetState(NodeState newState, Blackboard snapshot)
        {
            if (newState == state) return;
            var oldState = state;
            state = newState;
            if (state == NodeState.Running) Enter(snapshot);
            else if (oldState == NodeState.Running) Leave(snapshot);
        }

        public virtual NodeResult Update(Blackboard snapshot)
        {
            if (state != NodeState.Running)
                SetState(NodeState.Running, snapshot);
            return new NodeResult(this);
        }
        public virtual void Enter(Blackboard snapshot) {}
        public virtual void Leave(Blackboard snapshot) {}

        public bool IsMatch(Blackboard snapshot)
        {            
            var ret = precondition == null || precondition.IsMatch(snapshot);
            BTLogger.Log(this, "IsMatch {0}? {1}", precondition, ret.Pretty());
            // exit running node if precondition is not valid anymore
            if (!ret) SetState(NodeState.Finished, snapshot);
            return ret;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        static JsonSerializerSettings JsonSettings
        {
            get
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };
                settings.Converters.Add(new RectJsonConverter());
                return settings;
            }
        }

        public string Serialize()
        {
            var data = JsonConvert.SerializeObject(this, JsonSettings);
            //PrettyLog.Log("[Serialized Size (json)] {0}", data.Length);
            return data;
        }

        public static Node Deserialize(string data)
        {
            var node = JsonConvert.DeserializeObject<Node>(data, JsonSettings);
            if (node != null)
            {
                node.UpdateParent(null);
            }
            //PrettyLog.Log("Deserialized. Root: {0}, Data Size: {1}", node, data.Length);
            return node;
        }
    }
}

public class RectJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Rect);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var result = new Rect();
        if (reader.TokenType != JsonToken.Null)
        {
            var jo = JObject.Load(reader);
            result.x = jo["x"].Value<float>();
            result.y = jo["y"].Value<float>();
            result.width = jo["width"].Value<float>();
            result.height = jo["height"].Value<float>();
        }
        return result;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        if (value.GetType() == typeof(Rect))
        {
            var rect = (Rect)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(rect.x);
            writer.WritePropertyName("y");
            writer.WriteValue(rect.y);
            writer.WritePropertyName("width");
            writer.WriteValue(rect.width);
            writer.WritePropertyName("height");
            writer.WriteValue(rect.height);
            writer.WriteEndObject();
        }
        else
        {
            // Should never get here
            writer.WriteNull();
        }
    }
}