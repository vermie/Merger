using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Merger;

namespace Merger.Test.Unit
{
    [TestClass]
    public class MatchAlgorithmTests
    {
        [TestMethod]
        public void CalculateMatchScore_ReturnsZeroForNoMatch()
        {
            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = 2,
                Property2 = 2,
                Property3 = "bar"
            };

            var merger = MergerHelper.CreateMerger();

            Assert.AreEqual(0, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));
        }

        [TestMethod]
        public void CalculateMatchScore_ReturnsCorrectMatchScores()
        {
            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject();

            var merger = MergerHelper.CreateMerger();

            instance2.Property1 = instance1.Property1;
            Assert.AreEqual(MergerHelper.Property1Score, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));

            instance2.Property1 = 0;
            instance2.Property2 = instance1.Property2;
            Assert.AreEqual(MergerHelper.Property2Score, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));

            instance2.Property2 = 0;
            instance2.Property3 = instance1.Property3;
            Assert.AreEqual(MergerHelper.Property3Score, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));
        }

        [TestMethod]
        public void CalculateMatchScore_ReturnsCorrectMatchScoreTotal()
        {
            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var merger = MergerHelper.CreateMerger();

            var totalScore = MergerHelper.Property1Score
                           + MergerHelper.Property2Score
                           + MergerHelper.Property3Score;

            Assert.AreEqual(totalScore, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));
        }

        [TestMethod]
        public void CalculateMatchScore_SameInstanceReturnsMaxPoints()
        {
            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var merger = MergerHelper.CreateMerger();

            Assert.AreEqual(int.MaxValue, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance1));
        }

        [TestMethod]
        public void CalculateMatchScore_UsesSpecifiedEqualityComparer()
        {
            // we'll compare mismatched cases using StringComparer.OrdinalIgnoreCase (specified in MergerHelper.CreateMerger)

            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property2 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = 2,
                Property2 = 2,
                Property3 = "FOO"
            };

            var merger = MergerHelper.CreateMerger();

            Assert.AreEqual(MergerHelper.Property3Score, merger.MatchAlgorithm.CalclateMatchIndex(instance1, instance2));
        }
    }
}
