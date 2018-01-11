﻿/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cheers.BehaviourTree
{

    [Serializable]
    public abstract class ControlNode : Node
    {
        public override bool IsLeaf
        {
            get
            {
                return false;
            }
        }
    }

    [Serializable]
    public abstract class SelectorNode : ControlNode
    {
        [JsonIgnore]
        Node _runningNode;
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#c7739f");
            }
        }

        public override NodeResult Update(Blackboard snapshot)
        {
            var result = new NodeResult(this);
            List<NodeResult> childResults = new List<NodeResult>();
            if (_runningNode != null)
            {
                // If the previously selected node is still running, we don't do another selecting
                result.state = UpdateChildNode(_runningNode, snapshot, childResults);
                if (result.state == NodeState.Running)
                {
                    result.childResults = childResults.ToArray();
                    return result;
                }
                _runningNode = null;
            }
            // Select another node when no previously selected node is running
            _runningNode = Select(snapshot, _runningNode);
            if (_runningNode == null)
            {
                result.state = NodeState.Finished;
            }
            else
            {
                // update this selector node's state with latest updated children's state
                result.state = UpdateChildNode(_runningNode, snapshot, childResults);                
            }
            if (result.state != NodeState.Running)
            {
                _runningNode = null;
            }
            result.childResults = childResults.ToArray();
            return result;
        }

        NodeState UpdateChildNode(Node node, Blackboard snapshot, List<NodeResult> childResults)
        {
            var nodeResult = node.Update(snapshot);
            childResults.Add(nodeResult);
            return nodeResult.state;
        }

        protected abstract Node Select(Blackboard snapshot, Node ignoredNode);
    }

    [Serializable]
    public class PrioritySelectorNode : SelectorNode
    {
        protected override Node Select(Blackboard snapshot, Node ignoreNode)
        {
            foreach (var child in children)
            {
                if (child == ignoreNode) continue;
                if (child.IsMatch(snapshot))
                    return child;
            }
            return null;
        }
    }
    [SerializeField]
    public class LastFirstSelectorNode : SelectorNode
    {
        [JsonIgnore]
        Node _lastUsedNode;

        protected override Node Select(Blackboard snapshot, Node ignoreNode)
        {
            if (_lastUsedNode != null && _lastUsedNode.IsMatch(snapshot))
            {
                return _lastUsedNode;
            }

            foreach (var child in children)
            {
                if (child == ignoreNode) continue;
                if (child != _lastUsedNode && child.IsMatch(snapshot))
                {
                    _lastUsedNode = child;
                    return child;
                }
            }
            return null;
        }
    }
    [Serializable]
    public class WeightedSelectorNode : SelectorNode
    {
        public float[] weights;

        public void SetWeights(float[] weights)
        {
            this.weights = weights;
        }

        public void SetNodeWeight(int childIndex, float weight)
        {
            if (childIndex >= weights.Length)
            {
                Array.Resize(ref weights, childIndex + 1);
            }
            weights[childIndex] = weight;
        }

        public void SetNodeWeight(Node child, float weight)
        {
            var childIndex = children.IndexOf(child);
            if (childIndex < 0)
            {
                PrettyLog.Error("Set weight error: '{0}' is not a child of '{1}'", child, this);
                return;
            }
            SetNodeWeight(childIndex, weight);
        }

        protected override Node Select(Blackboard snapshot, Node ignoreNode)
        {
            if (children.Count == 0)
                return null;

            // initialize the weights with same values if not correctly set
            if (weights == null || weights.Length == 0 || Array.TrueForAll(weights, w => w == 0))
            {
                if (weights == null || weights.Length == 0)
                {
                    weights = new float[children.Count];
                }
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] = 1;
                }
            }

            var arrCdf = new float[weights.Length];
            for (int i = 0; i < arrCdf.Length; i++)
            {
                if (i == 0)
                    arrCdf[i] = weights[i];
                else
                {
                    arrCdf[i] = arrCdf[i - 1] + weights[i];
                }
            }
            var rnd = UnityEngine.Random.Range(0, arrCdf[arrCdf.Length - 1]);
            int min = 0, max = arrCdf.Length - 1;
            var index = BinarySearchInRanges(arrCdf, rnd, min, max);

            if (index < 0 || index >= children.Count)
            {
                PrettyLog.Error("Random weighted sampling error: result index is {0}", index);
            }

            return children[index];
        }

        /// <summary>
        /// Binary search the float array for the index of the float range that 'val' lies in
        /// The val lies in the range of arr[i] if:
        /// <![CDATA[
        /// arr[i - 1] < val <= arr[i]
        /// or
        /// 0 <= val <= arr[1]
        /// ]]>
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="val"></param>
        /// <param name="minIndex"></param>
        /// <param name="maxIndex"></param>
        /// <returns>The element index whose range holds 'val'</returns>
        public static int BinarySearchInRanges(float[] arr, float val, int minIndex, int maxIndex)
        {
            if (arr == null || arr.Length == 0) return -1;
            if (arr.Length == 1) return 0;

            int midIndex = minIndex + (maxIndex - minIndex) / 2;
            var midCdf = arr[midIndex];
            if (val > midCdf)
            {
                minIndex = midIndex + 1;
            }
            else
            {
                maxIndex = midIndex;
            }
            if (maxIndex == minIndex)
                return minIndex;

            return BinarySearchInRanges(arr, val, minIndex, maxIndex);
        }
    }

    [Serializable]
    public class SequenceNode : ControlNode
    {
        [JsonIgnore]
        public Queue<Node> runningNodes = new Queue<Node>();
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#e9ae83");
            }
        }

        public override NodeResult Update(Blackboard snapshot)
        {
            var result = new NodeResult(this);
            var childResults = new List<NodeResult>();

            // if the sequence is empty, entering this node means that child nodes should be queued and play
            if (runningNodes.Count == 0)
            {
                FillQueue(snapshot);
            }

            while (runningNodes.Count > 0)
            {
                var headNode = runningNodes.Peek();
                var childResult = headNode.Update(snapshot);
                childResults.Add(childResult);
                if (childResult.state == NodeState.Running)
                {
                    // the loop breaks and result returned if the head node is still running
                    result.state = NodeState.Running;
                    result.childResults = childResults.ToArray();
                    return result;
                }
                // following nodes should be proceeded as many as possible in a single frame
                runningNodes.Dequeue();
            }
            // returns exit result when the whole sequence is finished
            result.state = NodeState.Finished;
            result.childResults = childResults.ToArray();
            return result;
        }

        void FillQueue(Blackboard snapshot)
        {
            foreach (var child in children)
            {
                if (child.IsMatch(snapshot))
                {
                    runningNodes.Enqueue(child);
                }
            }
        }
    }

    [Serializable]
    public class ParallelNode : ControlNode
    {
        public enum Operator
        {
            AND,
            OR
        }
        /// <summary>
        /// With what operator should the children's status be combined
        /// AND means this node exits when all children are exited
        /// OR means this node exits when any child is exited
        /// </summary>
        [JsonIgnore]
        public Operator statusOperator;
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#8cb679");
            }
        }

        public override NodeResult Update(Blackboard snapshot)
        {
            var result = new NodeResult(this);
            var childResults = new List<NodeResult>();
            var isRunning = statusOperator == Operator.AND ? true : false;
            foreach (var child in children)
            {
                if (child.IsMatch(snapshot))
                {
                    var childResult = child.Update(snapshot);
                    childResults.Add(childResult);
                    var isChildRunning = childResult.state == NodeState.Running;
                    switch (statusOperator)
                    {
                        case Operator.AND:
                            isRunning = isRunning && isChildRunning;
                            break;
                        case Operator.OR:
                            isRunning = isRunning || isChildRunning;
                            break;
                    }
                }
            }
            result.state = isRunning ? NodeState.Running : NodeState.Finished;
            result.childResults = childResults.ToArray();
            return result;
        }
    }

}
