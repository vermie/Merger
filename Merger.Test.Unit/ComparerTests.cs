using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Merger.Test.Unit
{
    [TestClass]
    public class ComparerTests
    {
        [TestMethod]
        public void Compare_IdenticalObjects_ReturnsOneResultWithNoConflicts()
        {
            List<MergeTestObject> source, src, destination, dst;

            src = source = new List<MergeTestObject>();
            dst = destination = new List<MergeTestObject>();

            src.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.Compare(src, dst);

            Assert.AreEqual(1, mergeResults.Count());
            Assert.AreEqual(0, mergeResults.First().Conflicts.Count());
        }

        [TestMethod]
        public void Compare_MatchedObjects_ReturnsOneResultWithConflicts()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            src.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property2 = 1, Property3 = "bar" });

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.Compare(src, dst);

            Assert.AreEqual(1, mergeResults.Count());
            Assert.AreEqual(2, mergeResults.First().Conflicts.Count());    // Property2 and Property3 don't match
        }

        [TestMethod]
        public void Compare_UnmatchedObjects_ReturnsTwoResultWithNoConflicts()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            // no match
            src.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 2, Property2 = 1, Property3 = "bar" });

            var merger = MergerHelper.CreateMerger();
            var mergeResults = merger.Compare(src, dst);

            Assert.AreEqual(2, mergeResults.Count());
            Assert.IsTrue(mergeResults.All(r => r.Conflicts.Count() == 0));
        }

        [TestMethod]
        public void Compare_MultipleSources_SignleDestination_OneMatch()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            // match 1 - 2 conflicts
            src.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property2 = 1, Property3 = "bar" });

            // 'match' 2 - 0 conflicts
            src.Add(new MergeTestObject() { Property1 = 2, Property2 = 2, Property3 = "foo" });

            var merger = MergerHelper.CreateMerger();
            var mergeResults = merger.Compare(src, dst);

            Assert.AreEqual(2, mergeResults.Count());
            Assert.AreEqual(2, mergeResults.First(r => r.Instance.Property1 == 1).Conflicts.Count());
            Assert.AreEqual(0, mergeResults.First(r => r.Instance.Property1 == 2).Conflicts.Count());
        }

        [TestMethod]
        public void Compare_TwoSources_TwoDestination_TwoMatches()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            // match 1 - 2 conflicts
            src.Add(new MergeTestObject() { Property1 = 1, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property2 = 1, Property3 = "bar" });

            // match 2 - 1 conflicts
            src.Add(new MergeTestObject() { Property1 = 2, Property2 = 2, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 2, Property2 = 1, Property3 = "foo" });

            var merger = MergerHelper.CreateMerger();
            var mergeResults = merger.Compare(src, dst);

            Assert.AreEqual(2, mergeResults.Count());
            Assert.AreEqual(2, mergeResults.First(r => r.Instance.Property1 == 1).Conflicts.Count());
            Assert.AreEqual(1, mergeResults.First(r => r.Instance.Property1 == 2).Conflicts.Count());
        }

        [TestMethod]
        public void MergeMissingAndCompare_MergesMissingValue()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            // match 1 - 0 conflicts (AutoMerge resolves the Property3 conflict)
            src.Add(new MergeTestObject() { Property1 = 1, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property3 = null });

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.MergeMissingAndCompare(src, dst);

            var result = mergeResults.First();
            Assert.AreEqual(0, result.Conflicts.Count());
            Assert.AreEqual("foo", result.Instance.Property3);
        }

        [TestMethod]
        public void MergeMissingAndCompare_DoesNotOverwriteExistingValue()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            // match 1 - 1 conflicts
            src.Add(new MergeTestObject() { Property1 = 1, Property3 = "foo" });
            dst.Add(new MergeTestObject() { Property1 = 1, Property3 = "bar" });

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.MergeMissingAndCompare(src, dst);

            var result = mergeResults.First();
            Assert.AreEqual(1, result.Conflicts.Count());
            Assert.AreEqual("bar", result.Instance.Property3);
        }

        [TestMethod]
        public void Merge_OverwritesAllValues()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            var source = new MergeTestObject() { Property1 = 1, Property3 = "foo" };
            var destination = new MergeTestObject() { Property1 = 2, Property3 = "bar" };

            // match 1 - 1 conflicts
            src.Add(source);
            dst.Add(destination);

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.Merge(src, dst);

            var result = mergeResults.First();
            Assert.AreEqual(source.Property1, destination.Property1);
            Assert.AreEqual(source.Property2, destination.Property2);
            Assert.AreEqual(source.Property3, destination.Property3);
        }

        [TestMethod]
        public void Merge_NoConflicts()
        {
            List<MergeTestObject> src, dst;

            src = new List<MergeTestObject>();
            dst = new List<MergeTestObject>();

            var source = new MergeTestObject() { Property1 = 1, Property3 = "foo" };
            var destination = new MergeTestObject() { Property1 = 2, Property3 = "bar" };

            // match 1 - 1 conflicts
            src.Add(source);
            dst.Add(destination);

            var merger = MergerHelper.CreateMerger();

            var mergeResults = merger.Merge(src, dst);

            Assert.IsTrue(mergeResults.All(r => !r.Conflicts.Any()));
        }
    }
}
