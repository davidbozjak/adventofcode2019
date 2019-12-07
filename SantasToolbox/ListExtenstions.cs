using System;
using System.Collections.Generic;
using System.Text;

namespace SantasToolbox
{
    public static class ListExtenstions
    {
        public static bool AddIfNotNull<T>(this List<T> list, T? element)
            where T:class
        {
            if (element == null)
            {
                return false;
            }

            list.Add(element);
            return true;
        }

        public static IEnumerable<IList<T>> PermuteList<T>(this IList<T> sequence)
        {
            return permute(sequence, 0, sequence.Count);

            IEnumerable<IList<T>> permute(IList<T> sequence, int k, int m)
            {
                if (k == m)
                {
                    yield return sequence;
                }
                else
                {
                    for (int i = k; i < m; i++)
                    {
                        SwapPlaces(sequence, k, i);

                        foreach (var newSquence in permute(sequence, k + 1, m))
                        {
                            yield return newSquence;
                        }

                        SwapPlaces(sequence, k, i);
                    }
                }
            }

            void SwapPlaces(IList<T> sequence, int indexA, int indexB)
            {
                T temp = sequence[indexA];
                sequence[indexA] = sequence[indexB];
                sequence[indexB] = temp;
            }
        }
    }
}
