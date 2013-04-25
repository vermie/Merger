using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Merger
{
    internal class MergeWrapper<T>
        where T : class
    {
        public int Id { get; private set; }
        public T Instance { get; private set; }

        public MergeWrapper(int id, T instance)
        {
            Id = id;
            Instance = instance;
        }
    }

    internal class MergeScore<T>
        where T : class
    {
        public MergeWrapper<T> Source { get; private set; }
        public MergeWrapper<T> Destination { get; private set; }
        public int Score { get; private set; }

        public MergeScore(MergeWrapper<T> source, MergeWrapper<T> destination, int score)
        {
            Source = source;
            Destination = destination;
            Score = score;
        }
    }

    internal class MergeMatch<T>
        where T : class
    {
        public T Source { get; private set; }
        public T Destination { get; private set; }

        public MergeMatch(T source, T destination)
        {
            Source = source;
            Destination = destination;
        }

        public MergeMatch(MergeScore<T> score)
        {
            Source = score.Source.Instance;
            Destination = score.Destination.Instance;
        }
    }

    public class MergeComparer<T> : IMergeComparer<T>
        where T : class
    {
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

        private IEnumerable<MergeMatch<T>> CompareInternal(IEnumerable<T> compareSource, IEnumerable<T> compareDestination)
        {
            var sources = compareSource.Select((instance, id) => new MergeWrapper<T>(id, instance)).ToArray();
            var destinations = compareDestination.Select((instance, id) => new MergeWrapper<T>(id, instance)).ToArray();

            var matches = new List<MergeMatch<T>>();
            var matchedSources = new HashSet<int>();
            var matchedDestinations = new HashSet<int>();

            var scores = from s in sources
                         from d in destinations
                         let score = MatchAlgorithm.CalclateMatchIndex(s.Instance, d.Instance)
                         orderby score descending
                         select new MergeScore<T>(s, d, score);

            foreach (var score in scores)
            {
                // not a match
                if (score.Score == 0)
                    continue;

                // already matched
                if (matchedDestinations.Contains(score.Destination.Id) || matchedSources.Contains(score.Source.Id))
                    continue;

                matches.Add(new MergeMatch<T>(score));
                matchedSources.Add(score.Source.Id);
                matchedDestinations.Add(score.Destination.Id);
            }

            foreach (var source in sources.Where(s => !matchedSources.Contains(s.Id)))
                matches.Add(new MergeMatch<T>(source.Instance, null));

            foreach (var destination in destinations.Where(d => !matchedDestinations.Contains(d.Id)))
                matches.Add(new MergeMatch<T>(null, destination.Instance));

            return matches;
        }

        public IEnumerable<CompareResult<T>> Compare(IEnumerable<T> compareSource, IEnumerable<T> compareDestination)
        {
            var matches = CompareInternal(compareSource, compareDestination);

            return GetConflicts(matches);
        }

        private IEnumerable<CompareResult<T>> GetConflicts(IEnumerable<MergeMatch<T>> matches)
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

        public IEnumerable<CompareResult<T>> AutoMergeAndCompare(IEnumerable<T> mergeSource, IEnumerable<T> mergeDestination)
        {
            var matches = CompareInternal(mergeSource, mergeDestination);

            foreach (var match in matches)
            {
                var source = match.Source;
                var destination = match.Destination;

                if (source == null || destination == null)
                    continue;

                CompareAlgorithm.AutoMerge(source, destination);
            }

            return GetConflicts(matches);
        }
    }
}
