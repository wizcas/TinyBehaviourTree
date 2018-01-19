/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System;
using System.Collections;
using UnityEngine;

namespace Cheers.BehaviourTree
{
    [Serializable]
    public abstract class BehaviourNode<T> : Node where T: Blackboard
    {
        
        public override bool IsLeaf
        {
            get
            {
                return true;
            }
        }
        public override Color EditorColor
        {
            get
            {
                return ColorHelper.ByWeb("#55a8ba");
            }
        }

        protected abstract bool IsKeepPlaying(T snapshot);
        protected abstract void Start(T snapshot);
        protected abstract bool Play(T snapshot);
        protected abstract void Stop(T snapshot);

        bool ValidateSnapshot(Blackboard snapshot, out T typedSnapshot){
            var actualType = snapshot.GetType();
            if (actualType != typeof(T) && actualType.IsSubclassOf(typeof(T)) && typeof(T).IsAssignableFrom(actualType))
            {
                PrettyLog.Error("{0} can not be converted to {1}", actualType, typeof(T));
                typedSnapshot = null;
                return false;
            }

            typedSnapshot = (T)snapshot;
            return true;
        }

        public sealed override void Enter(Blackboard snapshot)
        {
            T typedSnapshot;
            if (!ValidateSnapshot(snapshot, out typedSnapshot))
            {
                return;
            }
            Start(typedSnapshot);
        }

        public sealed override NodeResult Update(Blackboard snapshot)
        {
            T typedSnapshot;
            if (!ValidateSnapshot(snapshot, out typedSnapshot))
            {
                return new NodeResult(this) { state = NodeState.Invalid };
            }

            var canPlay = IsKeepPlaying(typedSnapshot);
            bool isStillPlaying = false;
            if (canPlay)
            {
                isStillPlaying = Play(typedSnapshot);
            }
            var resultStatus = isStillPlaying ? NodeState.Running : NodeState.Finished;
            return new NodeResult(this) { state = resultStatus };
        }

        public sealed override void Leave(Blackboard snapshot)
        {
            T typedSnapshot;
            if (!ValidateSnapshot(snapshot, out typedSnapshot))
            {
                return;
            }
            Stop(typedSnapshot);
        }
    }
}