/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers.BehaviourTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.BehaviourTree
{
    [Serializable]
    public class PlayerBlackboard : Blackboard<PlayerBlackboard, PlayerOrder>
    {
        #region Fields
        public bool stopCurrentPosture;
        #endregion

        #region Data
        public PlayerPosture? posture;
        public bool? isOnGround;
        public bool? isGravityEnabled;
        public PlayerAction action;
        public PlayerAction playingAction;
        #endregion        

        #region Response Data (modified by behaviour tree and send back to view)
        public bool HasOrder
        {
            get { return order != null; }
        }
        #endregion

        public override bool IsEmpty
        {
            get
            {
                return
                    !posture.HasValue &&
                    !isOnGround.HasValue &&
                    !isGravityEnabled.HasValue &&
                    action == null;
            }
        }

        public override void Copy(PlayerBlackboard target)
        {
            target.posture = posture;
            target.isOnGround = isOnGround;
            target.isGravityEnabled = isGravityEnabled;
            target.action = action;
            target.playingAction = playingAction;
            target.order = order;
            target.stopCurrentPosture = stopCurrentPosture;
        }        

        protected override void DoApplyChanges(PlayerBlackboard change)
        {
            if (change.posture.HasValue)
            {
                if(change.posture.Value != posture)
                {
                    stopCurrentPosture = true;
                }
                posture = change.posture.Value;
            }
            if (change.isOnGround.HasValue)
            {
                isOnGround = change.isOnGround.Value;
            }
            if (change.isGravityEnabled.HasValue)
            {
                isGravityEnabled = change.isGravityEnabled.Value;
            }
            if (change.action != null)
            {
                action = change.action;
            }
        }

        protected override void AfterApplyChanges()
        {
            base.AfterApplyChanges();
            stopCurrentPosture = false;
        }

        public override void ExecuteOrder(PlayerOrder newOrder)
        {
            if (newOrder.clearAction) action = null;
        }
    }

    [Serializable]
    public class PlayerOrder : IOrder
    {
        public PlayerPosture? posture;
        public Action actionCallback;
        public bool clearAction;
    }
}