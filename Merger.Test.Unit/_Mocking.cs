using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merger;

namespace Merger.Test.Unit
{
    public class MergeTestObject
    {
        public int ID { get; set; }

        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public string Property3 { get; set; }

        public int NotProperty;
    }

    public class MergeTestObjectWithComplexMember
    {
        public int ID { get; set; }

        public MergeTestObject ComplexMember1 { get; set; }
        public IEnumerable<object> ComplexMember2 { get; set; }
    }

    public class TestObjectWithReadonlyProperty
    {
        public int Id { get; set; }

        public int ReadonlyProperty { get { return 0; } }
    }

    public class MergerHelper
    {
        public static int Property1Score { get { return 10; } }
        public static int Property2Score { get { return 5; } }
        public static int Property3Score { get { return 5; } }

        public static MergeComparer<MergeTestObject> CreateMerger()
        {
            var merger = MergeComparer<MergeTestObject>.Create(o => o.ID, true);

            merger.MatchAlgorithm.AddMatchEvaluator(Property1Score, o => o.Property1);
            merger.MatchAlgorithm.AddMatchEvaluator(Property2Score, o => o.Property2);
            merger.MatchAlgorithm.AddMatchEvaluator(Property3Score, o => o.Property3, new SoftStringEqualityComparer(StringComparisonOptions.CaseInsensitive));

            return merger;
        }
    }
}
