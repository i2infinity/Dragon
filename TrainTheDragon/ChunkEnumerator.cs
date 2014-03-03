using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DragonClassifier
{
    /// <summary>
    /// Chunker - http://stackoverflow.com/a/419058/289970
    /// </summary>
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source,
                                                           int chunkSize)
        {
            // Validate parameters.
            if (source == null) throw new ArgumentNullException("source");
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException("chunkSize",
                                                      "The chunkSize parameter must be a positive value.");

            // Call the internal implementation.
            return source.ChunkInternal(chunkSize);
        }

        private static IEnumerable<IEnumerable<T>> ChunkInternal<T>(
            this IEnumerable<T> source, int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(source != null);
            Debug.Assert(chunkSize > 0);

            // Get the enumerator.  Dispose of when done.
            using (IEnumerator<T> enumerator = source.GetEnumerator())
                do
                {
                    // Move to the next element.  If there's nothing left
                    // then get out.
                    if (!enumerator.MoveNext()) yield break;

                    // Return the chunked sequence.
                    yield return ChunkSequence(enumerator, chunkSize);
                } while (true);
        }

        private static IEnumerable<T> ChunkSequence<T>(IEnumerator<T> enumerator,
                                                       int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(enumerator != null);
            Debug.Assert(chunkSize > 0);

            // The count.
            var count = 0;

            // There is at least one item.  Yield and then continue.
            do
            {
                // Yield the item.
                yield return enumerator.Current;
            } while (++count < chunkSize && enumerator.MoveNext());
        }
    }
}
