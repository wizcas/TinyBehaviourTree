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
            return (snapshot.status == PlayerStatus.Dead) == isDead;
        }
    }

    public class PreconditionActionType : Precondition<PlayerBlackboard>
    {
        public PlayerActionType actionType;
        public PreconditionActionType(PlayerActionType expect) { actionType = expect; }
        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            return snapshot.action != null && snapshot.action.type == actionType;
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

    public class PreconditionStatus : Precondition<PlayerBlackboard>
    {
        public PlayerStatus status;
        public PreconditionStatus(PlayerStatus expect) { status = expect; }

        protected override bool IsMatch(PlayerBlackboard snapshot)
        {
            return snapshot.status == status;
        }
    }
}
