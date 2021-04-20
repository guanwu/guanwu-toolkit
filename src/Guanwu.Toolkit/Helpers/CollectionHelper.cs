using System.Collections.Generic;
using System.Linq;

namespace Guanwu.Toolkit.Helpers
{
    public sealed class CollectionHelper
    {
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> empty = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(empty, (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item })
            );
        }
    }
}