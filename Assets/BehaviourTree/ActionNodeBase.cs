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
    public abstract class ActionNode<T> : Node where T: Blackboard
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

        protected abstract bool CanPlay(T snapshot);
        protected abstract bool Play(T snapshot);

        public sealed override NodeResult Update(Blackboard snapshot)
        {
            var actualType = snapshot.GetType();
            if (actualType != typeof(T) && actualType.IsSubclassOf(typeof(T)) && typeof(T).IsAssignableFrom(actualType))
            {
                PrettyLog.Error("{0} can not be converted to {1}", actualType, typeof(T));
                return new NodeResult(this) { state = NodeState.Invalid };
            }

            var typedSnapshot = (T)snapshot;
            var canPlay = CanPlay(typedSnapshot);
            bool isStillPlaying = false;
            if (canPlay)
            {
                isStillPlaying = Play((T)snapshot);
            }
            var resultStatus = isStillPlaying ? NodeState.Running : NodeState.Finished;
            return new NodeResult(this) { state = resultStatus };
        }
    }
}