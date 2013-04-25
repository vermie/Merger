using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Merger;
using System.Linq.Expressions;

namespace Merger.Test.Unit
{
    [TestClass]
    public class PropertyWrapperTests
    {
        private void CheckName(string expectedPropertyName, IPropertyWrapper<MergeTestObject> propertyWrapper)
        {
            Assert.AreEqual(expectedPropertyName, propertyWrapper.Name);
        }

        [TestMethod]
        public void Wrapper_PropertyNameExposedThroughNameProperty()
        {
            var propertyWrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);

            // this should match the name of whatever property is accessed in the lambda above
            CheckName("Property1", propertyWrapper);
        }

        [TestMethod]
        public void Wrapper_Copy_ValueCopied()
        {
            var propertyWrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var instance2 = new MergeTestObject();

            propertyWrapper.Copy(instance1, instance2);

            Assert.AreEqual(instance1.Property1, instance2.Property1);
        }

        [TestMethod]
        public void Wrapper_Copy_OnlyWrappedPropertiesCopied()
        {
            var propertyWrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var canary = 666;
            var instance2 = new MergeTestObject()
            {
                Property2 = canary,
                Property3 = canary.ToString()
            };

            propertyWrapper.Copy(instance1, instance2);

            Assert.AreEqual(canary, instance2.Property2);
            Assert.AreEqual(canary.ToString(), instance2.Property3);
        }

        [TestMethod]
        public void Wrapper_AreEqual_DefaultComparer_WorksProperly()
        {
            var property1Wrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);
            var property3Wrapper = new PropertyWrapper<MergeTestObject, string>(o => o.Property3);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1,
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = instance1.Property1,
                Property3 = instance1.Property3
            };

            Conflict conflict;
            Assert.IsTrue(property1Wrapper.AreEqual(instance1, instance2, out conflict));
            Assert.IsTrue(property3Wrapper.AreEqual(instance1, instance2, out conflict));
        }

        [TestMethod]
        public void Wrapper_AreEqual_CustomComparer_WorksProperly()
        {
            var property3Wrapper = new PropertyWrapper<MergeTestObject, string>(o => o.Property3, new SoftStringEqualityComparer(StringComparisonOptions.CaseInsensitive));

            var instance1 = new MergeTestObject()
            {
                Property3 = "foo"
            };

            var instance2 = new MergeTestObject()
            {
                Property3 = "FOO"
            };

            Conflict conflict;
            Assert.IsTrue(property3Wrapper.AreEqual(instance1, instance2, out conflict));
        }

        [TestMethod]
        public void Wrapper_AreEqual_ReturnsTrueAndNullConflict()
        {
            var property1Wrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = instance1.Property1
            };

            Conflict conflict;
            Assert.IsTrue(property1Wrapper.AreEqual(instance1, instance2, out conflict));
            Assert.IsNull(conflict);
        }

        [TestMethod]
        public void Wrapper_AreEqual_ReturnsFalseAndConflictHasProperValues()
        {
            var property1Wrapper = new PropertyWrapper<MergeTestObject, int>(o => o.Property1);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var instance2 = new MergeTestObject()
            {
                Property1 = 2
            };

            Conflict conflict;
            Assert.IsFalse(property1Wrapper.AreEqual(instance1, instance2, out conflict));

            Assert.IsNotNull(conflict);
            Assert.AreEqual("Property1", conflict.PropertyName);    // the property name in the lambda above
            Assert.AreEqual(instance1.Property1.ToString(), conflict.SourceValue);
            Assert.AreEqual(instance2.Property1.ToString(), conflict.DestinationValue);
        }

        [TestMethod]
        public void WrapperCreator_FromExpression_WorksProperly()
        {
            Expression<Func<MergeTestObject, int>> expression = o => o.Property1;

            var propertyWrapper = PropertyWrapperHelper.Create(expression);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var instance2 = new MergeTestObject();

            propertyWrapper.Copy(instance1, instance2);

            // this should match the name of whatever property is accessed in the lambda above
            Assert.AreEqual("Property1", propertyWrapper.Name);
            Assert.AreEqual(instance1.Property1, instance2.Property1);
        }

        [TestMethod]
        public void WrapperCreator_FromPropertyInfo_WorksProperly()
        {
            var propertyInfo = typeof(MergeTestObject).GetProperty("Property1");

            var propertyWrapper = PropertyWrapperHelper.Create<MergeTestObject>(propertyInfo);

            var instance1 = new MergeTestObject()
            {
                Property1 = 1
            };

            var instance2 = new MergeTestObject();

            propertyWrapper.Copy(instance1, instance2);

            // this should match the name of whatever property is accessed in the lambda above
            Assert.AreEqual("Property1", propertyWrapper.Name);
            Assert.AreEqual(instance1.Property1, instance2.Property1);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParameterValidation_ThrowsWhenParamterIsNotMemberAccess()
        {
            PropertyWrapperHelper.Create<MergeTestObject, int>(o => 1);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParameterValidation_ThrowsWhenParamterIsNotPropertyAccess()
        {
            PropertyWrapperHelper.Create<MergeTestObject, int>(o => o.NotProperty);
        }
    }
}
