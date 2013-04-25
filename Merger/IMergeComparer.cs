using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public class Conflict
    {
        public string PropertyName { get; private set; }
        public string DestinationValue { get; private set; }
        public string SourceValue { get; private set; }

        public Conflict(string name, string sourceValue, string destinationValue)
        {
            PropertyName = name;
            SourceValue = sourceValue;
            DestinationValue = destinationValue;
        }
    }

    public class CompareResult<T>
    {
        public T Instance { get; private set; }
        public IEnumerable<Conflict> Conflicts { get; private set; }

        public CompareResult(T instance, IEnumerable<Conflict> conflicts)
        {
            Instance = instance;
            Conflicts = conflicts;
        }
    }

    public interface IMergeComparer<T>
        where T : class
    {
        /// <summary>
        /// Performs a union operation on the supplied sets of objects, and returns conflict information.
        /// <para>The union operation is implementation-specified behavior.</para>
        /// </summary>
        /// <param name="compareSource">The set that is being merged into another set.</param>
        /// <param name="compareDestination">The set that would ultimately hold the combined set of results.</param>
        /// <returns></returns>
        IEnumerable<CompareResult<T>> Compare(IEnumerable<T> compareSource, IEnumerable<T> compareDestination);

        /// <summary>
        /// Merges destination's missing values in from source.
        /// <para>Then performs a union operation on the supplied sets of objects, and returns conflict information.</para>
        /// <para>The union operation is implementation-specified behavior.</para>
        /// </summary>
        /// <param name="mergeSource">The set that is being merged into another set.</param>
        /// <param name="mergeDestination">The set that would ultimately hold the combined set of results.</param>
        /// <returns></returns>
        IEnumerable<CompareResult<T>> AutoMergeAndCompare(IEnumerable<T> mergeSource, IEnumerable<T> mergeDestination);
    }
}
