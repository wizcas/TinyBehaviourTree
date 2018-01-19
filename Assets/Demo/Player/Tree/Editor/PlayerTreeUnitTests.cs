/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Cheers.BehaviourTree;
using BT = Cheers.BehaviourTree.BehaviourTree;

namespace Player.BehaviourTree.Tests
{
    public class PlayerTreeUnitTests
    {
        Node rootNode;

        NodeState? FindNodeStatus(string name, NodeResult result)
        {
            if (result.node.name == name) return result.State;

            if (result.childResults == null || result.childResults.Length == 0)
                return null;

            foreach(var child in result.childResults)
            {
                var childStatus = FindNodeStatus(name, child);
                if (childStatus.HasValue)
                    return childStatus;
            }
            return null;
        }

        [SetUp]
        public void Setup() {
            rootNode = PlayerTree.MakeRootNode(rootNode);
        }
        [Test]
        public void TestHitActionAndRunningStatus()
        {
            var snapshot = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Hit, null, null, ()=>true),
                posture = PlayerPosture.Running,
                isOnGround = true,
            };
            var result = BT.TestUpdate(rootNode, snapshot, null);
            Assert.AreEqual(NodeState.Running, result.State);
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Hit", result));
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Running", result));
        }
        [Test]
        public void TestAttackActionAndIdleStatus()
        {
            var snapshot = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Cast, null, null, () =>true),
                posture = PlayerPosture.Idle,
                isOnGround = true,
            };
            var result = BT.TestUpdate(rootNode, snapshot, null);
            Assert.AreEqual(NodeState.Running, result.State);
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Attack", result));
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Idle", result));
        }
        [Test]
        public void TestDieActionAndFallingAliveAliveStatus()
        {
            var snapshot = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Die, null, null, () =>true),
                posture = PlayerPosture.Idle,
                isOnGround = false,
                isGravityEnabled = true,
            };
            var result = BT.TestUpdate(rootNode, snapshot, null);
            Assert.AreEqual(NodeState.Running, result.State);
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Die", result));
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Falling Alive", result));
        }

        [Test]
        public void TestFalllingDeadStatus()
        {
            var snapshot = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Die, null, null, null),
                posture = PlayerPosture.Dead,
                isOnGround = false,
                isGravityEnabled = true,
            };
            var result = BT.TestUpdate(rootNode, snapshot, null);
            Assert.AreEqual(NodeState.Running, result.State);
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Falling Dead", result));
        }

        [Test]
        public void TestDeadStatus()
        {
            var snapshot = new PlayerBlackboard()
            {
                action = new PlayerAction(PlayerActionType.Die, null, null, null),
                posture = PlayerPosture.Dead,
                isOnGround = true,
                isGravityEnabled = true,
            };
            var result = BT.TestUpdate(rootNode, snapshot, null);
            Assert.AreEqual(NodeState.Running, result.State);
            Assert.AreEqual(NodeState.Running, FindNodeStatus("Dead", result));
        }
    }
}
