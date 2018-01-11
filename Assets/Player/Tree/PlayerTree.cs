/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cheers.BehaviourTree;
using System;
using Cheers;

namespace Player.BehaviourTree
{
    public class PlayerTree : BehaviourTree<PlayerBlackboard>
    {
        public bool IsGravityEnabled
        {
            get
            {
                return blackboard.isGravityEnabled.HasValue ? blackboard.isGravityEnabled.Value : false;
            }
        }
        public bool IsOnGround
        {
            get { return blackboard.isOnGround.HasValue ? blackboard.isOnGround.Value : false; }
        }

        protected override PlayerBlackboard MakeInitBlackboard()
        {
            return new PlayerBlackboard()
            {
                status = PlayerStatus.Idle,
                isOnGround = true,
                isGravityEnabled = true
            };
        }

        [See]
        public void MakeTree()
        {
            rootNode = MakeRootNode(rootNode);
        }

        [See]
        public void Clear()
        {
            rootNode.Clear();
        }

        public static Node MakeRootNode(Node root)
        {
            if (root == null)
            {
                root = new ParallelNode() { name = "Root", statusOperator = ParallelNode.Operator.OR };
            }
            root.Clear();

            root.AddNodes(
                new LastFirstSelectorNode() { name = "Action" }
                .AddNodes(
                    new LastFirstSelectorNode()
                    {
                        name = "Alive Actions",
                        precondition = new PreconditionIsDead(false)
                    }
                    .AddNodes(new PlayerActionNode() { name = "Attack", precondition = new PreconditionActionType(PlayerActionType.Cast) })
                    .AddNodes(new PlayerActionNode() { name = "Hit", precondition = new PreconditionActionType(PlayerActionType.Hit) })
                    .AddNodes(new PlayerActionNode() { name = "Die", precondition = new PreconditionActionType(PlayerActionType.Die) })
                ),
                new LastFirstSelectorNode() { name = "Status" }
                .AddNodes(
                    new LastFirstSelectorNode()
                    {
                        name = "In Air w/ Alive",
                        precondition = new PreconditionAnd(new PreconditionIsOnGround(false), new PreconditionIsGravityEnabled(true))
                    }
                    .AddNodes(new PlayerStatusNode() { name = "Falling Alive", precondition = new PreconditionIsDead(false), status = PlayerStatus.FallingAlive })
                    .AddNodes(new PlayerStatusNode() { name = "Falling Dead", precondition = new PreconditionIsDead(true), status = PlayerStatus.FallingDead }),
                    new LastFirstSelectorNode()
                    {
                        name = "On Ground",
                        precondition = new PreconditionIsOnGround(true)
                    }
                    .AddNodes(new PlayerStatusNode() { name = "Empty", precondition = new PreconditionStatus(PlayerStatus.Empty) })
                    .AddNodes(new PlayerStatusNode() { name = "Idle", precondition = new PreconditionStatus(PlayerStatus.Idle) })
                    .AddNodes(new PlayerStatusNode() { name = "Running", precondition = new PreconditionStatus(PlayerStatus.Running) })
                    .AddNodes(new PlayerStatusNode() { name = "Dead", precondition = new PreconditionStatus(PlayerStatus.Dead) })
                )
            );
            return root;
        }

        protected override void Ready()
        {
            base.Ready();
            Messenger.Broadcast(Msg.Player.Ready);
        }

        protected override void AfterUpdate(PlayerBlackboard snapshot)
        {
            Messenger.Broadcast(Msg.Player.Order, snapshot.order);
        }

        #region API
        public void StopPlayingAction(PlayerAction enteringAction)
        {
            var pb = blackboard;
            if (pb.playingAction == enteringAction) return;
            // clear the action state if it's the playing action
            // or the action will be played in next frame again
            if (pb.playingAction == pb.action)
            {
                pb.action = null;
            }
            pb.playingAction = null;
        }
        #endregion

        #region Test
        [See]
        void TestStatusHitAndRun()
        {
            var request = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Hit, null),
                status = PlayerStatus.Running,
                isOnGround = true,
                playingAction = null,
            };
            ExecuteTest(request);
        }
        [See]
        void TestIdleAndAttack()
        {
            var request = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Cast, null),
                status = PlayerStatus.Idle,
                isOnGround = true,
                playingAction = null,
            };
            ExecuteTest(request);
        }
        [See]
        void TestFallingAliveAndDie()
        {
            var request = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Die, null),
                status = PlayerStatus.Idle,
                isOnGround = false,
                isGravityEnabled = true,
                playingAction = null,
            };
            ExecuteTest(request);
        }
        [See]
        void TestFallingDead()
        {
            var request = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Die, null),
                status = PlayerStatus.Dead,
                isOnGround = false,
                isGravityEnabled = true,
            };
            ExecuteTest(request);
        }

        void ExecuteTest(PlayerBlackboard request)
        {
            if (!Application.isPlaying)
            {
                PrettyLog.Error("Run only when playing");
                return;
            }
            StartCoroutine(ExecuteTestCo(request));
        }

        IEnumerator ExecuteTestCo(PlayerBlackboard request)
        {

            WriteOnBlackboard(request);
            // wait for result
            yield return new WaitForSeconds(.1f);
            PrintFrameResult();
        }
        #endregion
    }
}