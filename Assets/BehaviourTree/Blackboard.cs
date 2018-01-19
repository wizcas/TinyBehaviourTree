using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheers.BehaviourTree
{
    public abstract class Blackboard
    {
        public IOrder order;
        public abstract bool IsEmpty { get; }
        public abstract Blackboard WithNewOrder();
        public abstract Blackboard Copy();
        public Blackboard ApplyChanges(Blackboard changes)
        {
            if (changes != null)
            {
                DoApplyChanges(changes);
            }
            var snapshot = Copy();
            AfterApplyChanges();
            return snapshot;
        }
        protected abstract void DoApplyChanges(Blackboard changes);
        protected virtual void AfterApplyChanges() { }
        public abstract void ExecuteOrder(IOrder newOrder);
    }

    public abstract class Blackboard<T, U> : Blackboard 
        where T : Blackboard, new()
        where U : IOrder, new()
    {
        public new U order
        {
            get { return (U)base.order; }
            set { base.order = value; }
        }

        public sealed override Blackboard Copy()
        {
            var copy = new T();
            Copy(copy);
            return copy;
        }

        public override Blackboard WithNewOrder()
        {
            order = new U();
            return this;
        }

        protected sealed override void DoApplyChanges(Blackboard changes)
        {
            DoApplyChanges((T)changes);
        }

        public sealed override void ExecuteOrder(IOrder newOrder)
        {
            ExecuteOrder((U)newOrder);
        }

        public abstract void Copy(T target);
        protected abstract void DoApplyChanges(T changes);
        public abstract void ExecuteOrder(U newOrder);
    }
}