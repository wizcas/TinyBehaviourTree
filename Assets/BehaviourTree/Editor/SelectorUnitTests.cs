/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheers.BehaviourTree.Tests
{
    public class SelectorUnitTests
    {
        [Test]
        public void TestBinarySearch()
        {
            var arr = new[] { .2f, .8f, .1f };
            Assert.AreEqual(0, WeightedSelectorNode.BinarySearchInRanges(arr, -100f, 0, 2));
            Assert.AreEqual(0, WeightedSelectorNode.BinarySearchInRanges(arr, .1f, 0, 2));
            Assert.AreEqual(1, WeightedSelectorNode.BinarySearchInRanges(arr, .4f, 0, 2));
            Assert.AreEqual(2, WeightedSelectorNode.BinarySearchInRanges(arr, .9f, 0, 2));
            Assert.AreEqual(0, WeightedSelectorNode.BinarySearchInRanges(arr, 0f, 0, 2));
            Assert.AreEqual(0, WeightedSelectorNode.BinarySearchInRanges(arr, .2f, 0, 2));
            Assert.AreEqual(1, WeightedSelectorNode.BinarySearchInRanges(arr, .8f, 0, 2));
            Assert.AreEqual(2, WeightedSelectorNode.BinarySearchInRanges(arr, 1, 0, 2));
            Assert.AreEqual(2, WeightedSelectorNode.BinarySearchInRanges(arr, 1.1f, 0, 2));

            arr = new float[] { 1 };
            Assert.AreEqual(0, WeightedSelectorNode.BinarySearchInRanges(arr, -100f, 0, 2));

            arr = new float[0];
            Assert.AreEqual(-1, WeightedSelectorNode.BinarySearchInRanges(arr, -100f, 0, 2));

            arr = null;
            Assert.AreEqual(-1, WeightedSelectorNode.BinarySearchInRanges(arr, -100f, 0, 2));
        }
    }
}