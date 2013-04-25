using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Merger;

namespace Merger.Test.Unit
{
    [TestClass]
    public class SoftEqualityComparerTests
    {
        [TestMethod]
        public void DefaultComparer_EqualReturnsTrue()
        {
            var comparer = DefaultSoftEqualityComparer<string>.Instance;

            Assert.IsTrue(comparer.Equals("foo", "foo"));
        }

        [TestMethod]
        public void DefaultComparer_NotEqualReturnsFalse()
        {
            var comparer = DefaultSoftEqualityComparer<string>.Instance;

            Assert.IsFalse(comparer.Equals("foo", "bar"));
        }

        [TestMethod]
        public void StringComparer_NoOptions_EqualReturnsTrue()
        {
            var comparer = new StringSoftEqualityComparer();

            Assert.IsTrue(comparer.Equals("foo", "foo"));
        }

        [TestMethod]
        public void StringComparer_NoOptions_NotEqualReturnsFalse()
        {
            var comparer = new StringSoftEqualityComparer();

            Assert.IsFalse(comparer.Equals("foo", "bar"));
        }

        [TestMethod]
        public void StringComparer_IgnoreWhitespace_EqualReturnsTrue()
        {
            var comparer = new StringSoftEqualityComparer(StringComparisonOptions.IgnoreWhitespace);

            Assert.IsTrue(comparer.Equals("foo", "foo"));
            Assert.IsTrue(comparer.Equals("foo", " foo "));
        }

        [TestMethod]
        public void StringComparer_CaseInsensitive_EqualReturnsTrue()
        {
            var comparer = new StringSoftEqualityComparer(StringComparisonOptions.CaseInsensitive);

            Assert.IsTrue(comparer.Equals("foo", "foo"));
            Assert.IsTrue(comparer.Equals("foo", "FOO"));
        }

        [TestMethod]
        public void StringComparer_CaseInsensitiveIgnoreWhitespace_EqualReturnsTrue()
        {
            var comparer = new StringSoftEqualityComparer(StringComparisonOptions.CaseInsensitive | StringComparisonOptions.IgnoreWhitespace);

            Assert.IsTrue(comparer.Equals("foo", "foo"));
            Assert.IsTrue(comparer.Equals("foo", " FOO "));
        }

        [TestMethod]
        public void StringComparer_NullEqualsAnything_EqualReturnsTrue()
        {
            var comparer = new StringSoftEqualityComparer(StringComparisonOptions.CaseInsensitive | StringComparisonOptions.IgnoreWhitespace, true);

            Assert.IsTrue(comparer.Equals("foo", "foo"));
            Assert.IsTrue(comparer.Equals("foo", null));
        }

        [TestMethod]
        public void NullComparer_NullEqualsAnything_NotNull_EqualReturnsTrue()
        {
            var comparer = new NullAwareSoftEqualityComparer<string>();

            Assert.IsTrue(comparer.Equals("foo", "foo"));
        }

        [TestMethod]
        public void NullComparer_NullEqualsAnything_Null_EqualReturnsTrue()
        {
            var comparer = new NullAwareSoftEqualityComparer<string>();

            Assert.IsTrue(comparer.Equals("foo", null));
        }

        [TestMethod]
        public void NullComparer_NullEqualsAnything_NotNullNullable_EqualReturnsTrue()
        {
            var comparer = new NullAwareSoftEqualityComparer<int?>();

            Assert.IsTrue(comparer.Equals(1, 1));
        }

        [TestMethod]
        public void NullComparer_NullEqualsAnything_NullNullable_EqualReturnsTrue()
        {
            var comparer = new NullAwareSoftEqualityComparer<int?>();

            Assert.IsTrue(comparer.Equals(1, null));
        }
    }
}
