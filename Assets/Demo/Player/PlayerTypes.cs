/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.BehaviourTree
{
    public enum PlayerPosture
    {
        Empty,
        Idle,
        Running,
        Dead,
        FallingAlive,
        FallingDead
    }

    public enum PlayerActionType
    {
        None,
        Hit,
        Cast
    }

    public class PlayerAction
    {
        public PlayerActionType type;
        public System.Action startAction;
        public System.Func<bool> isActionPlaying;
        public System.Action stopAction;

        public PlayerAction(PlayerActionType type, System.Action startAction, System.Action stopAction, System.Func<bool> isActionPlaying)
        {
            this.type = type;
            this.startAction = startAction;
            this.stopAction = stopAction;
            this.isActionPlaying = isActionPlaying;
        }
    }
}