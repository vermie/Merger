using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger.Test.Executable
{
    class TestObject
    {
        public Guid Id { get; set; }

        public string Property1 { get; set; }
        public int Property2 { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // create two objects with the same key
            var key = Guid.NewGuid();

            var fromDatabase = new TestObject()
            {
                Id = key,
                Property1 = "foo",
                Property2 = 1
            };

            var fromView = new TestObject()
            {
                Id = key,
                Property1 = "bar",
                Property2 = 1
            };

            // create object that matches TestObject instances on Id things (usually want to make this into a singleton)
            var mergeComparer = MergeComparer<TestObject>.Create(o => o.Id, false);
            mergeComparer.MatchAlgorithm.AddMatchEvaluator(100, o => o.Id);

            // compare the two objects, using the user-supplied input as a source
            var results = mergeComparer.Compare(new[] { fromView }, new[] { fromDatabase });

            // there should only be one result, since we match on Id
            var result = results.Single();

            foreach (var conflict in result.Conflicts)
            {
                // destination is the old value in the database
                Console.WriteLine("User changing {0} from {1} to {2}", conflict.PropertyName, conflict.DestinationValue, conflict.SourceValue);
            }

            WaitForUser();
        }

        static void WaitForUser()
        {
            if (Environment.UserInteractive)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    Console.Write("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
