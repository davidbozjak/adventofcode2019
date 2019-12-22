using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _22_Shuffle
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<IInstruction<int>>("Input.txt", GetInstruction);

            Part1(10007, inputProvider);
        }

        private static void Part1(int deckSize, IEnumerator<IInstruction<int>> instructionSet)
        {
            IList<int> deck = Enumerable.Range(0, deckSize).ToList();

            while (instructionSet.MoveNext())
            {
                deck = instructionSet.Current.ApplyTechnique(deck.ToList().AsReadOnly());
            }

            //Console.WriteLine("Deck after all instructions");
            //Console.WriteLine(string.Join(", ", deck));

            var indexToFind = 2019;
            var indexOfCard = deck.IndexOf(indexToFind);
            Console.WriteLine("Part 1 Done");
            Console.WriteLine($"Index of card {indexToFind}: {indexOfCard}");
        }

        private static bool GetInstruction(string? input, out IInstruction<int> value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                if (input.ToLower() == "deal into new stack")
                {
                    value = new DealNewStackInstruction<int>();
                    return true;
                }
                else if (input.ToLower().StartsWith("deal with increment"))
                {
                    int n = int.Parse(input.Substring("deal with increment".Length + 1));
                    value = new DealIncrementN<int>(n);
                    return true;
                }
                else if (input.ToLower().StartsWith("cut"))
                {
                    int n = int.Parse(input.Substring("cut".Length + 1));
                    value = new CutNInstruction<int>(n);
                    return true;
                }
            }
            catch
            {
                
            }

            return false;
        }

        class DealIncrementN<T> : IInstruction<T>
        {
            public int N { get; }

            public DealIncrementN(int n)
            {
                this.N = n;
            }

            public IList<T> ApplyTechnique(IReadOnlyList<T> input)
            {
                var output = new T[input.Count];

                for (int i = 0, inputCount = 0; inputCount < input.Count; i = i + N < output.Length ? i + N : i + N - output.Length, inputCount++)
                {
                    output[i] = input[inputCount];
                }

                return output.ToList();
            }
        }

        class CutNInstruction<T> : IInstruction<T>
        {
            public int N { get; }
            public CutNInstruction(int n)
            {
                this.N = n;
            }

            public IList<T> ApplyTechnique(IReadOnlyList<T> input)
            {
                int divide = N >= 0 ? N : N + input.Count;

                var output = new List<T>();

                output.AddRange(input.Skip(divide));
                output.AddRange(input.Take(divide));

                return output;
            }
        }

        class DealNewStackInstruction<T> : IInstruction<T>
        {
            public DealNewStackInstruction()
            {
            }

            public IList<T> ApplyTechnique(IReadOnlyList<T> input)
            {
                var output = input.ToList();
                output.Reverse();
                return output;
            }
        }

        interface IInstruction<T>
        {
            public IList<T> ApplyTechnique(IReadOnlyList<T> input);
        }
    }
}
