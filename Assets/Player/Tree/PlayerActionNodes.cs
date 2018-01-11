﻿/*****************************************************
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
    [PrettyLog.Provider("PlayerTree", "action", "orange", "blue")]
    public class PlayerActionNode : ActionNode<PlayerBlackboard>
    {
        PlayerAction _playingAction;
        protected override bool CanPlay(PlayerBlackboard snapshot)
        {
            // If there is no playing action OR the playing action is this action itself, 
            // we consider this action can be played
            // i.e. The action is playable only when PlayerBlackboard.playingAction is externally set to null

            // So actions that interrupt playing ones should be responsible to set playingAction as null
            // in order to make it play forcely.

            return snapshot.playingAction == null || snapshot.action.type == _playingAction.type;
        }

        protected override bool Play(PlayerBlackboard snapshot)
        {
            if (snapshot.action == null)
                return false;

            _playingAction = snapshot.action;

            // if the action is playing, do not send the playing order again
            if (snapshot.playingAction != null && snapshot.playingAction.type == _playingAction.type) return true;

            snapshot.playingAction = _playingAction;
            PrettyLog.Log("take action: {0}", snapshot.action == null ? "(no action)" : _playingAction.type.ToString());
            if (snapshot.action.onAction != null)
            {
                snapshot.action.onAction();
            }
            // Action is continuous operation, which can only be stopped by setting playingAction to null
            return true;
        }
    }

    [Serializable]
    [PrettyLog.Provider("PlayerTree", "status", "orange", "green")]
    public class PlayerStatusNode : ActionNode<PlayerBlackboard>
    {
        public PlayerStatus? status = null;

        protected override bool CanPlay(PlayerBlackboard snapshot)
        {
            return true;
        }

        protected override bool Play(PlayerBlackboard snapshot)
        {
            if (snapshot.order != null)
            {
                snapshot.order.status = status.HasValue ? status.Value : snapshot.status;
            }
            return false; // Status is an instant operation, so it always marked finished in every update
        }
    }
}