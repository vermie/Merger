using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Merger;

namespace Merger.Test.Unit
{
    [TestClass]
    public class CompareAlgorithmTests
    {
        [TestMethod]
        public void Compare_GeneratesOneConflictPerProperty()
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

            var conflicts = merger.CompareAlgorithm.Compare(instance1, instance2);

            Assert.AreEqual(3, conflicts.Count());
            Assert.IsNotNull(conflicts.FirstOrDefault(c => c.PropertyName == "Property1"));
            Assert.IsNotNull(conflicts.FirstOrDefault(c => c.PropertyName == "Property2"));
            Assert.IsNotNull(conflicts.FirstOrDefault(c => c.PropertyName == "Property3"));
        }

        [TestMethod]
        public void Compare_ConflictsHaveCorrectValues()
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

            var conflicts = merger.CompareAlgorithm.Compare(instance1, instance2);

            var conflict = conflicts.First(c => c.PropertyName == "Property1");
            Assert.AreEqual(instance1.Property1.ToString(), conflict.SourceValue);
            Assert.AreEqual(instance2.Property1.ToString(), conflict.DestinationValue);

            conflict = conflicts.First(c => c.PropertyName == "Property2");
            Assert.AreEqual(instance1.Property2.ToString(), conflict.SourceValue);
            Assert.AreEqual(instance2.Property2.ToString(), conflict.DestinationValue);

            conflict = conflicts.First(c => c.PropertyName == "Property3");
            Assert.AreEqual(instance1.Property3.ToString(), conflict.SourceValue);
            Assert.AreEqual(instance2.Property3.ToString(), conflict.DestinationValue);
        }

        [TestMethod]
        public void IgnoreProperty_IgnoredPropertiesDontHaveConflicts()
        {
            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = 2,
            };

            var merger = MergerHelper.CreateMerger();

            merger.CompareAlgorithm.IgnoreProperty(o => o.Property1);

            var conflicts = merger.CompareAlgorithm.Compare(instance1, instance2);
            Assert.IsFalse(conflicts.Any());
        }

        [TestMethod]
        public void ForProperty_DiscoveredPropertiesAreOverridenWithForProperty()
        {
            var instance1 = new MergeTestObject()
            {
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property3 = "FOO"
            };

            var merger = MergerHelper.CreateMerger();

            // ignore case will result in no conflicts
            merger.CompareAlgorithm.ForProperty(o => o.Property3, new StringSoftEqualityComparer(StringComparisonOptions.CaseInsensitive));

            var conflicts = merger.CompareAlgorithm.Compare(instance1, instance2);

            Assert.IsFalse(conflicts.Any());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParameterValidation_IgnoreProperty_ThrowsWhenParamterIsNotPropertyAccess()
        {
            var merger = MergerHelper.CreateMerger();
            merger.CompareAlgorithm.IgnoreProperty(o => o.NotProperty);
        }

        [TestMethod]
        public void Compare_NonSimplePropertiesAreIgnored()
        {
            var comparer = MergeComparer<MergeTestObjectWithComplexMember>.Create(o => o.ID, true);

            var instance1 = new MergeTestObjectWithComplexMember()
            {
                ComplexMember1 = new MergeTestObject(),
                ComplexMember2 = new List<object>(new[] { "foo" })
            };

            var instance2 = new MergeTestObjectWithComplexMember()
            {
                ComplexMember1 = new MergeTestObject(),
                ComplexMember2 = new List<object>(new[] { "bar" })
            };

            var conflicts = comparer.CompareAlgorithm.Compare(instance1, instance2);

            Assert.AreEqual(0, conflicts.Count());
        }

        [TestMethod]
        public void AutoCompare_MergesMissingValue()
        {
            var comparer = MergeComparer<MergeTestObject>.Create(o => o.ID, true);

            var instance1 = new MergeTestObject()
            {
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property3 = null
            };

            comparer.CompareAlgorithm.AutoMerge(instance1, instance2);

            Assert.AreEqual(instance1.Property3, instance2.Property3);
            Assert.AreEqual("foo", instance1.Property3);
            Assert.AreEqual("foo", instance2.Property3);
        }

        [TestMethod]
        public void AutoCompare_DoesNotOverwriteExistingValue()
        {
            var comparer = MergeComparer<MergeTestObject>.Create(o => o.ID, true);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property3 = "bar"
            };

            comparer.CompareAlgorithm.AutoMerge(instance1, instance2);

            Assert.AreNotEqual(instance1.Property3, instance2.Property3);
            Assert.AreEqual("foo", instance1.Property3);
            Assert.AreEqual("bar", instance2.Property3);
        }
    }
}
