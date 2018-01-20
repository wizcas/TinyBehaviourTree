/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Cheers.BehaviourTree
{    
    public abstract class BehaviourTree : MonoBehaviour
    {
        public BehaviourTreeData treeData;
        public int frameRate = 30;
        public bool logFrames;

        protected Blackboard _blackboard;
        Blackboard _snapshot;
        Queue<Blackboard> _queueingBlackboard = new Queue<Blackboard>();
        float _nextUpdateTime;
        bool _isReady;

        #region Editor Use
        [System.NonSerialized]
        public NodeResult _frameResult;
        [System.NonSerialized]
        public System.Action onTreeUpdated;
        #endregion

        public Node rootNode
        {
            set { treeData.rootNode = value; }
            get { return treeData.rootNode; }
        }

        float updateInterval
        {
            get
            {
                return 1f / frameRate;
            }
        }

        public void WriteOnBlackboard(Blackboard changes)
        {
            _queueingBlackboard.Enqueue(changes);
        }

        private void Start()
        {
            InitializeBlackboard();
            DoUpdate(null); // runs tree for the first time
            Ready();
        }

        protected abstract void InitializeBlackboard();
        protected virtual void Ready() { }

        void DoUpdate(Blackboard request)
        {
            BTLogger.Enabled = logFrames;
            BTLogger.BeginFrame(rootNode);
            _snapshot = _blackboard.ApplyChanges(request).WithNewOrder();
            _frameResult = rootNode.Update(_snapshot);
            _blackboard.ExecuteOrder(_snapshot.order);
            ExecuteOrder(_snapshot.order);
            BTLogger.EndFrame(rootNode);
        }

        void Update()
        {
            if (rootNode == null) return;
            if (_blackboard == null || _blackboard.IsEmpty) return;

            if (Time.time < _nextUpdateTime)
            {
                return;
            }
            _nextUpdateTime = Time.time + updateInterval;

            Blackboard request = null;
            if (_queueingBlackboard.Count > 0)
            {
                while (_queueingBlackboard.Count > 0)
                {
                    request = _queueingBlackboard.Dequeue();
                    Profiler.BeginSample("Update queued request");
                    DoUpdate(request);
                    Profiler.EndSample();
                }
            }
            else
            {
                Profiler.BeginSample("Update current snapshot");
                DoUpdate(request);
                Profiler.EndSample();
            }

            if (onTreeUpdated != null)
                onTreeUpdated();
        }

        protected abstract void ExecuteOrder(IOrder order);

        #region Test
        public static NodeResult TestUpdate(Node rootNode, Blackboard snapshot, Blackboard next)
        {
            return rootNode.Update(snapshot);
        }

        [See]
        public void PrintFrameResult()
        {
            PrettyLog.Log("Frame result: \n{0}", _frameResult);
        }
        #endregion
    }

    public abstract class BehaviourTree<T, U> : BehaviourTree 
        where T : Blackboard
        where U : IOrder
    {
        protected T blackboard
        {
            get { return (T)_blackboard; }
        }

        protected sealed override void InitializeBlackboard()
        {
            _blackboard = MakeInitBlackboard();
        }

        protected abstract T MakeInitBlackboard();

        protected sealed override void ExecuteOrder(IOrder newOrder)
        {
            ExecuteOrder((U)newOrder);
        }

        protected abstract void ExecuteOrder(U newOrder);
    }

    public interface IOrder
    {

    }

}