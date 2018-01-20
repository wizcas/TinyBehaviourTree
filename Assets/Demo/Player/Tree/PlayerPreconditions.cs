/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cheers.BehaviourTree;

namespace Player.BehaviourTree
{
    public class PreconditionIsDead : Precondition<PlayerBlackboard>
    {
        public bool isDead;
        public PreconditionIsDead(bool expect) { isDead = expect; }

        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            return (snapshot.posture == PlayerPosture.Dead) == isDead;
        }
    }

    public class PreconditionIsOnGround : Precondition<PlayerBlackboard>
    {
        public bool isOnGround;
        public PreconditionIsOnGround(bool expect) { isOnGround = expect; }
        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            return snapshot.isOnGround == isOnGround;
        }
    }
    public class PreconditionIsGravityEnabled : Precondition<PlayerBlackboard>
    {
        public bool isGravityEnabled;
        public PreconditionIsGravityEnabled(bool expect) { isGravityEnabled = expect; }
        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            return snapshot.isGravityEnabled == isGravityEnabled;
        }
    }

    public class PreconditionIsNodeAction : Precondition<PlayerBlackboard>
    {
        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            if (snapshot.action != null && node is PlayerActionNode)
            {
                return ((PlayerActionNode)node).actionType == snapshot.action.type;
            }
            return false;
        }
    }

    public class PreconditionIsNodePosture : Precondition<PlayerBlackboard>{
        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            if(node is PlayerPostureNode){
                return ((PlayerPostureNode)node).posture == snapshot.posture;
            }
            return false;
        }
    }
}
