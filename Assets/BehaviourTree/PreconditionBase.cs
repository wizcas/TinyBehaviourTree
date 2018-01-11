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
using UnityEngine;

namespace Cheers.BehaviourTree
{
    [Serializable]
    public abstract class Precondition
    {
        public abstract bool IsMatch(Blackboard snapshot);
        public override string ToString()
        {
            return GetType().Name;
        }
    }
    [Serializable]
    public abstract class Precondition<T> : Precondition where T : Blackboard
    {
        public sealed override bool IsMatch(Blackboard snapshot)
        {
            return IsMatch((T)snapshot);
        }
        protected abstract bool IsMatch(T snapshot);
    }

    [Serializable]
    public class PreconditionAnd : Precondition
    {
        public Precondition[] subPreconditions;
        public PreconditionAnd(params Precondition[] subPreconditions)
        {
            this.subPreconditions = subPreconditions.ToArray();
        }

        public override bool IsMatch(Blackboard snapshot)
        {
            var ret = true;
            foreach(var sub in subPreconditions)
            {
                ret = ret && sub.IsMatch(snapshot);
            }
            return ret;
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join(" <color=orange>AND</color> ", subPreconditions.Select(sp => sp.ToString()).ToArray()));
        }
    }

    [Serializable]
    public class PreconditionOr : Precondition
    {
        public Precondition[] subPreconditions;
        public PreconditionOr(params Precondition[] subPreconditions)
        {
            this.subPreconditions = subPreconditions.ToArray();
        }

        public override bool IsMatch(Blackboard snapshot)
        {
            var ret = false;
            foreach (var sub in subPreconditions)
            {
                ret = ret || sub.IsMatch(snapshot);
            }
            return ret;
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join(" <color=orange>OR</color> ", subPreconditions.Select(sp => sp.ToString()).ToArray()));
        }
    }
}