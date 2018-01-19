/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers.BehaviourTree;
using System;

namespace Player.BehaviourTree
{
    [Serializable]
    [PrettyLog.Provider("PlayerTree", "action", "orange", "blue")]
    public class PlayerActionNode : BehaviourNode<PlayerBlackboard>
    {
        public PlayerActionType actionType;

        public static PlayerActionNode New(string name, PlayerActionType type, Precondition precondition = null)
        {
            var node = Node.MakeNode<PlayerActionNode>(name, precondition);
            node.actionType = type;
            return node;
        }

        protected override void Start(PlayerBlackboard snapshot)
        {
            PrettyLog.Log("take action: {0}", snapshot.action == null ? "(no action)" : snapshot.action.type.ToString());
            if (snapshot.HasOrder)
                snapshot.order.actionCallback = snapshot.action.startAction;
        }

        protected override void Stop(PlayerBlackboard snapshot)
        {
            if (snapshot.HasOrder)
            {
                snapshot.order.actionCallback = snapshot.action.stopAction;
                snapshot.order.clearAction = true;
            }
        }

        protected override bool Play(PlayerBlackboard snapshot)
        {
            if (snapshot.action == null)
                return false;

            if (snapshot.action.isActionPlaying != null)
            {
                return snapshot.action.isActionPlaying();
            }
            return false;            
        }
    }

    [Serializable]
    [PrettyLog.Provider("PlayerTree", "status", "orange", "green")]
    public class PlayerPostureNode : BehaviourNode<PlayerBlackboard>
    {
        public PlayerPosture posture;


        public static PlayerPostureNode New(string name, PlayerPosture posture, Precondition precondition = null)
        {
            var node = Node.MakeNode<PlayerPostureNode>(name, precondition);
            node.posture = posture;
            return node;
        }

        protected override bool Play(PlayerBlackboard snapshot)
        {
            return !snapshot.stopCurrentPosture;
        }

        protected override void Start(PlayerBlackboard snapshot)
        {
            if (snapshot.order != null)
            {
                snapshot.order.posture = posture;
            }
        }

        protected override void Stop(PlayerBlackboard snapshot)
        {
        }

    }
}