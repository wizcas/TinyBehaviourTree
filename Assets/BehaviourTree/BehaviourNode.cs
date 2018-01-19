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
            base.Enter(snapshot);
            T typedSnapshot;
            if (!ValidateSnapshot(snapshot, out typedSnapshot))
            {
                SetState(NodeState.Invalid, snapshot);
            }
            else
            {
                Start(typedSnapshot);
            }
        }

        public sealed override NodeResult Update(Blackboard snapshot)
        {
            var result = base.Update(snapshot); // Calls Enter() in the base method. State becomes Running if everything's OK.
            if (state == NodeState.Running) // If the node is entered successfully, do updating
            {
                T typedSnapshot;
                if (ValidateSnapshot(snapshot, out typedSnapshot))
                {
                    var isRunning = Play(typedSnapshot);
                    SetState(isRunning ? NodeState.Running : NodeState.Finished, snapshot);
                }
                else
                {
                    SetState(NodeState.Invalid, snapshot);
                }
            }
            return result;
        }

        public sealed override void Leave(Blackboard snapshot)
        {
            base.Leave(snapshot);
            T typedSnapshot;
            if (ValidateSnapshot(snapshot, out typedSnapshot))
            {
                Stop(typedSnapshot);
            }
        }
    }
}