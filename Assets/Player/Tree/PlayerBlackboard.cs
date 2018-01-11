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
    public class PlayerBlackboard : Blackboard<PlayerBlackboard>
    {
        #region Fields
        #endregion

        #region Data
        public PlayerStatus? status;
        public bool? isOnGround;
        public bool? isGravityEnabled;
        public PlayerAction action;
        public PlayerAction playingAction;
        #endregion        

        #region Response Data (modified by behaviour tree and send back to view)
        public PlayerOrder order;
        #endregion

        public override bool IsEmpty
        {
            get
            {
                return
                    !status.HasValue &&
                    !isOnGround.HasValue &&
                    !isGravityEnabled.HasValue &&
                    action == null;
            }
        }

        public override void Copy(PlayerBlackboard target)
        {
            target.status = status;
            target.isOnGround = isOnGround;
            target.isGravityEnabled = isGravityEnabled;
            target.action = action;
            target.playingAction = playingAction;
            target.order = order;
        }

        public override Blackboard WithNewOrder()
        {
            order = new PlayerOrder();
            return this;
        }

        protected override void DoApplyChanges(PlayerBlackboard change)
        {
            if (change.status.HasValue)
            {
                status = change.status.Value;
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

        public override PlayerBlackboard UpdateFrameState(PlayerBlackboard frameUpdated)
        {
            playingAction = frameUpdated.playingAction;
            return this;
        }
    }

    [Serializable]
    public class PlayerOrder : IOrder
    {
        public PlayerStatus? status;
    }
}