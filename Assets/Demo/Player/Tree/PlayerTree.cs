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
                posture = PlayerPosture.Idle,
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
                root = ParallelNode.New("Root", ParallelNode.Operator.OR, null);
                //root = new ParallelNode() { name = "Root", statusOperator = ParallelNode.Operator.OR };
            }
            root.Clear();

            root.AddNodes(
                LastFirstSelectorNode.New("Action")
                .AddNodes(
                    LastFirstSelectorNode.New("Alive Actions", new PreconditionIsDead(false))
                    .AddNodes(
                        PlayerActionNode.New("Attack", PlayerActionType.Cast, new PreconditionIsNodeAction()),
                        PlayerActionNode.New("Hit", PlayerActionType.Hit, new PreconditionIsNodeAction()),
                        PlayerActionNode.New("Die", PlayerActionType.Die, new PreconditionIsNodeAction())
                    )
                ),
                LastFirstSelectorNode.New("Posture")
                .AddNodes(
                    LastFirstSelectorNode.New("In Air + Gravity",
                                                   new PreconditionAnd(
                                                       new PreconditionIsOnGround(false),
                                                       new PreconditionIsGravityEnabled(true)
                                                      ))
                    .AddNodes(
                        PlayerPostureNode.New("Falling Alive", PlayerPosture.FallingAlive, new PreconditionIsDead(false)),
                        PlayerPostureNode.New("Falling Dead", PlayerPosture.FallingDead, new PreconditionIsDead(true))
                    ),
                    LastFirstSelectorNode.New("On Ground", new PreconditionIsOnGround(true))
                    .AddNodes(
                        PlayerPostureNode.New("Empty", PlayerPosture.Empty, new PreconditionIsNodePosture()),
                        PlayerPostureNode.New("Idle", PlayerPosture.Idle, new PreconditionIsNodePosture()),
                        PlayerPostureNode.New("Running", PlayerPosture.Running, new PreconditionIsNodePosture()),
                        PlayerPostureNode.New("Dead", PlayerPosture.Dead, new PreconditionIsNodePosture())
                    )
                )
            );

            //root.AddNodes(
            //    new LastFirstSelectorNode() { name = "Action" }
            //    .AddNodes(
            //        new LastFirstSelectorNode()
            //        {
            //            name = "Alive Actions",
            //            precondition = new PreconditionIsDead(false)
            //        }
            //        .AddNodes(new PlayerActionNode() { name = "Attack", precondition = new PreconditionActionType(PlayerActionType.Cast) })
            //        .AddNodes(new PlayerActionNode() { name = "Hit", precondition = new PreconditionActionType(PlayerActionType.Hit) })
            //        .AddNodes(new PlayerActionNode() { name = "Die", precondition = new PreconditionActionType(PlayerActionType.Die) })
            //    ),
            //    new LastFirstSelectorNode() { name = "Status" }
            //    .AddNodes(
            //        new LastFirstSelectorNode()
            //        {
            //            name = "In Air w/ Alive",
            //            precondition = new PreconditionAnd(new PreconditionIsOnGround(false), new PreconditionIsGravityEnabled(true))
            //        }
            //        .AddNodes(new PlayerPostureNode() { name = "Falling Alive", precondition = new PreconditionIsDead(false), posture = PlayerPosture.FallingAlive })
            //        .AddNodes(new PlayerPostureNode() { name = "Falling Dead", precondition = new PreconditionIsDead(true), posture = PlayerPosture.FallingDead }),
            //        new LastFirstSelectorNode()
            //        {
            //            name = "On Ground",
            //            precondition = new PreconditionIsOnGround(true)
            //        }
            //        .AddNodes(new PlayerPostureNode() { name = "Empty", precondition = new PreconditionStatus(PlayerPosture.Empty) })
            //        .AddNodes(new PlayerPostureNode() { name = "Idle", precondition = new PreconditionStatus(PlayerPosture.Idle) })
            //        .AddNodes(new PlayerPostureNode() { name = "Running", precondition = new PreconditionStatus(PlayerPosture.Running) })
            //        .AddNodes(new PlayerPostureNode() { name = "Dead", precondition = new PreconditionStatus(PlayerPosture.Dead) })
            //    )
            //);
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
                posture = PlayerPosture.Running,
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
                posture = PlayerPosture.Idle,
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
                posture = PlayerPosture.Idle,
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
                posture = PlayerPosture.Dead,
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