/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public abstract class SelectorNode<T> : SelectorNode where T : SelectorNode, new()
    {
        public static T New(string name, Precondition precondition = null)
        {
            return Node.MakeNode<T>(name, precondition);
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
            var result = base.Update(snapshot);
            List<NodeResult> childResults = new List<NodeResult>();
            NodeState childResult = NodeState.Invalid;
            if (_runningNode != null)
            {
                // If the previously selected node is still running, we don't do another selecting
                childResult = UpdateChildNode(_runningNode, snapshot, childResults, false);
            }
            else
            {
                // Select another node when no previously selected node is running
                _runningNode = Select(snapshot, _runningNode);
                if (_runningNode == null)
                {
                    SetState(NodeState.Finished, snapshot);
                }
                else
                {
                    // update this selector node's state with latest updated children's state
                    childResult = UpdateChildNode(_runningNode, snapshot, childResults, true);
                }
            }
            if (childResult != NodeState.Running)
            {
                _runningNode = null;
            }
            SetState(childResult, snapshot);
            result.childResults = childResults.ToArray();
            return result;
        }

        NodeState UpdateChildNode(Node node, Blackboard snapshot, List<NodeResult> childResults, bool beginRunning)
        {
            var nodeResult = node.Update(snapshot);
            childResults.Add(nodeResult);
            return nodeResult.State;
        }

        protected abstract Node Select(Blackboard snapshot, Node ignoredNode);
    }

    [Serializable]
    public class PrioritySelectorNode : SelectorNode<PrioritySelectorNode>
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
    public class LastFirstSelectorNode : SelectorNode<LastFirstSelectorNode>
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
    public class WeightedSelectorNode : SelectorNode<WeightedSelectorNode>
    {
        public float[] weights;

        public static WeightedSelectorNode New(string name, float[] weights, Precondition precondition = null)
        {
            var node = Node.MakeNode<WeightedSelectorNode>(name, precondition);
            node.SetWeights(weights);
            return node;
        }

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
        public Queue<QueuedNode> runningNodes = new Queue<QueuedNode>();
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#e9ae83");
            }
        }
        public static SequenceNode New(string name, Precondition precondition = null)
        {
            return Node.MakeNode<SequenceNode>(name, precondition);
        }
        public override NodeResult Update(Blackboard snapshot)
        {
            var result = base.Update(snapshot);
            var childResults = new List<NodeResult>();

            // if the sequence is empty, entering this node means that child nodes should be queued and play
            if (runningNodes.Count == 0)
            {
                FillQueue(snapshot);
            }

            while (runningNodes.Count > 0)
            {
                var headNode = runningNodes.Peek();
                // If this node is accessed by the first time, call its Enter() method
                if (headNode.beginRunning)
                {
                    headNode.node.Enter(snapshot);
                    headNode.beginRunning = false;
                }
                var childResult = headNode.node.Update(snapshot);
                childResults.Add(childResult);
                if (childResult.State == NodeState.Running)
                {
                    // the loop breaks and result returned if the head node is still running
                    result.childResults = childResults.ToArray();
                    return result;
                }
                // queued nodes should be proceeded as many as possible in a single frame
                // so dequeue the head and proceed the next queued node
                var finishedNode = runningNodes.Dequeue();
                finishedNode.node.Leave(snapshot);
            }
            // returns exit result when the whole sequence is finished
            SetState(NodeState.Finished, snapshot);
            result.childResults = childResults.ToArray();
            return result;
        }

        void FillQueue(Blackboard snapshot)
        {
            foreach (var child in children)
            {
                if (child.IsMatch(snapshot))
                {
                    runningNodes.Enqueue(new QueuedNode(child));
                }
            }
        }

        public class QueuedNode
        {
            public bool beginRunning;
            public Node node;

            public QueuedNode(Node node)
            {
                this.node = node;
                beginRunning = true;
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
        public Operator statusOperator;
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#8cb679");
            }
        }

        NodeResult[] _prevFrameChildResults;

        public static ParallelNode New(string name, Operator op, Precondition precondition = null)
        {
            var node = Node.MakeNode<ParallelNode>(name, precondition);
            node.statusOperator = op;
            return node;
        }

        public override NodeResult Update(Blackboard snapshot)
        {
            var result = base.Update(snapshot);
            var childResults = new List<NodeResult>();
            var isRunning = statusOperator == Operator.AND ? true : false;
            foreach (var child in children)
            {
                if (child.IsMatch(snapshot))
                {
                    if (IsChildBeginRunning(child))
                        child.Enter(snapshot);

                    var childResult = child.Update(snapshot);
                    childResults.Add(childResult);
                    var isChildRunning = childResult.State == NodeState.Running;

                    if (!isChildRunning)
                        child.Leave(snapshot);
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
            result.childResults = childResults.ToArray();
            if (isRunning)
            {
                _prevFrameChildResults = result.childResults;
            }
            else
            {
                _prevFrameChildResults = null;
            }
            SetState(isRunning ? NodeState.Running : NodeState.Finished, snapshot);
            return result;
        }

        bool IsChildBeginRunning(Node child)
        {
            if (_prevFrameChildResults == null || _prevFrameChildResults.Length == 0) return true;

            var prevResult = _prevFrameChildResults.FirstOrDefault(cr => cr.node == child);
            if (prevResult.node == null) return true;
            return prevResult.State != NodeState.Running;
        }
    }

}
