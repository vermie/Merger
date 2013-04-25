using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Merger
{
    public class MergeComparer<T> : IMergeComparer<T>
        where T : class
    {
        private class MergeWrapper
        {
            public int Id { get; private set; }
            public T Instance { get; private set; }

            public MergeWrapper(int id, T instance)
            {
                Id = id;
                Instance = instance;
            }
        }

        private class MergeScore
        {
            public MergeWrapper Source { get; private set; }
            public MergeWrapper Destination { get; private set; }
            public int Score { get; private set; }

            public MergeScore(MergeWrapper source, MergeWrapper destination, int score)
            {
                Source = source;
                Destination = destination;
                Score = score;
            }
        }

        private class MergeMatch
        {
            public T Source { get; private set; }
            public T Destination { get; private set; }

            public MergeMatch(T source, T destination)
            {
                Source = source;
                Destination = destination;
            }

            public MergeMatch(MergeScore score)
            {
                Source = score.Source.Instance;
                Destination = score.Destination.Instance;
            }
        }

        public IMatchAlgorithm<T> MatchAlgorithm { get; private set; }
        public ICompareAlgorithm<T> CompareAlgorithm { get; private set; }

        private MergeComparer(IMatchAlgorithm<T> matchAlgorithm, ICompareAlgorithm<T> compareAlgorithm)
        {
            MatchAlgorithm = matchAlgorithm;
            CompareAlgorithm = compareAlgorithm;
        }

        public static MergeComparer<T> Create<TKey>(Expression<Func<T, TKey>> keyAccessor, bool ignoreKey)
        {
            var matchAlgorithm = new MatchAlgorithm<T>();
            var mergeAlgorithm = new CompareAlgorithm<T, TKey>(keyAccessor, ignoreKey);
            return new MergeComparer<T>(matchAlgorithm, mergeAlgorithm);
        }

        private IEnumerable<MergeMatch> CompareInternal(IEnumerable<T> compareSource, IEnumerable<T> compareDestination)
        {
            var sources = compareSource.Select((instance, id) => new MergeWrapper(id, instance)).ToArray();
            var destinations = compareDestination.Select((instance, id) => new MergeWrapper(id, instance)).ToArray();

            var matches = new List<MergeMatch>();
            var matchedSources = new HashSet<int>();
            var matchedDestinations = new HashSet<int>();

            var scores = from s in sources
                         from d in destinations
                         let score = MatchAlgorithm.CalclateMatchIndex(s.Instance, d.Instance)
                         orderby score descending
                         select new MergeScore(s, d, score);

            foreach (var score in scores)
            {
                // not a match
                if (score.Score == 0)
                    continue;

                // already matched
                if (matchedDestinations.Contains(score.Destination.Id) || matchedSources.Contains(score.Source.Id))
                    continue;

                matches.Add(new MergeMatch(score));
                matchedSources.Add(score.Source.Id);
                matchedDestinations.Add(score.Destination.Id);
            }

            foreach (var source in sources.Where(s => !matchedSources.Contains(s.Id)))
                matches.Add(new MergeMatch(source.Instance, null));

            foreach (var destination in destinations.Where(d => !matchedDestinations.Contains(d.Id)))
                matches.Add(new MergeMatch(null, destination.Instance));

            return matches;
        }

        public IEnumerable<CompareResult<T>> Compare(IEnumerable<T> compareSource, IEnumerable<T> compareDestination)
        {
            var matches = CompareInternal(compareSource, compareDestination);

            return GetConflicts(matches);
        }

        private IEnumerable<CompareResult<T>> GetConflicts(IEnumerable<MergeMatch> matches)
        {
            var allConflicts = new List<CompareResult<T>>();
            foreach (var match in matches)
            {
                var source = match.Source;
                var destination = match.Destination;

                if (source == null || destination == null)
                {
                    allConflicts.Add(new CompareResult<T>(source ?? destination, Enumerable.Empty<Conflict>()));
                    continue;
                }

                var conflicts = CompareAlgorithm.Compare(source, destination);

                allConflicts.Add(new CompareResult<T>(destination, conflicts));
            }

            return allConflicts;
        }

        private IEnumerable<CompareResult<T>> MergeAndCompare(IEnumerable<T> mergeSource, IEnumerable<T> mergeDestination, Action<T, T> merge)
        {
            var matches = CompareInternal(mergeSource, mergeDestination);

            foreach (var match in matches)
            {
                var source = match.Source;
                var destination = match.Destination;

                if (source == null || destination == null)
                    continue;

                merge(source, destination);
            }

            return GetConflicts(matches);
        }

        public IEnumerable<CompareResult<T>> MergeMissingAndCompare(IEnumerable<T> mergeSource, IEnumerable<T> mergeDestination)
        {
            return MergeAndCompare(mergeSource, mergeDestination, CompareAlgorithm.MergeMissing);
        }

        public IEnumerable<CompareResult<T>> Merge(IEnumerable<T> mergeSource, IEnumerable<T> mergeDestination)
        {
            return MergeAndCompare(mergeSource, mergeDestination, CompareAlgorithm.MergeAll);
        }
    }
}
