using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace _22_Shuffle
{
    class Program
    {
        static void Main(string[] args)
        {
            using var intInputProvider = new InputProvider<IInstruction<int>>("Input.txt", GetInstruction);

            Part1(10007, intInputProvider);

            using var longInputProvider = new InputProvider<IInstruction<long>>("Input.txt", GetInstruction);
            Part2(2020, 119315717514047, 101741582076661, longInputProvider.ToList());
            
            //testing with part1 input:
            //Part2(2306, 10007, 1, longInputProvider.ToList());    // Must return 2019
        }

        private static void Part1(int deckSize, IEnumerator<IInstruction<int>> instructionSet)
        {
            IList<int> deck = Enumerable.Range(0, deckSize).ToList();

            while (instructionSet.MoveNext())
            {
                deck = instructionSet.Current.ApplyTechnique(deck.ToList().AsReadOnly());
            }

            var indexToFind = 2019;
            var indexOfCard = deck.IndexOf(indexToFind);
            Console.WriteLine("Part 1 Done");
            Console.WriteLine($"Index of card {indexToFind}: {indexOfCard}");
        }

        private static void Part2(long indexToLookAt, long deckSize, long repeatCount, IList<IInstruction<long>> instructionSet)
        {
            BigInteger Y = IndexAfterInstructionSet(indexToLookAt);
            BigInteger Z = IndexAfterInstructionSet(Y);
            BigInteger A = (Y - Z) * ModInverse(indexToLookAt - Y + deckSize, deckSize) % deckSize;
            while (A < 0) A += deckSize;
            BigInteger B = (Y - A * indexToLookAt) % deckSize;
            while (B < 0) B += deckSize;

            var cardAtIndexToLookAt = (BigInteger.ModPow(A, repeatCount, deckSize) * indexToLookAt + (BigInteger.ModPow(A, repeatCount, deckSize) - 1) * ModInverse(A - 1, deckSize) * B) % deckSize;

            Console.WriteLine("Part 2 Done");
            Console.WriteLine($"Card at index {indexToLookAt} is: {cardAtIndexToLookAt}");

            BigInteger IndexAfterInstructionSet(BigInteger index)
            {
                var reversedOrderInstructions = instructionSet.Reverse();

                foreach (var instruction in reversedOrderInstructions)
                {
                    index = instruction.IndexAfterTechniqueIsApplied(index, deckSize);
                }

                return index;
            }
        }

        private static bool GetInstruction<T>(string? input, out IInstruction<T> value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                if (input.ToLower() == "deal into new stack")
                {
                    value = new DealNewStackInstruction<T>();
                    return true;
                }
                else if (input.ToLower().StartsWith("deal with increment"))
                {
                    int n = int.Parse(input.Substring("deal with increment".Length + 1));
                    value = new DealIncrementN<T>(n);
                    return true;
                }
                else if (input.ToLower().StartsWith("cut"))
                {
                    int n = int.Parse(input.Substring("cut".Length + 1));
                    value = new CutNInstruction<T>(n);
                    return true;
                }
            }
            catch
            {
                
            }

            return false;
        }

        private static BigInteger ModInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
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

            public BigInteger IndexAfterTechniqueIsApplied(BigInteger index, long deckSize)
            {
                var result = ModInverse(N, deckSize) * index % deckSize;
                return result;
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

            public BigInteger IndexAfterTechniqueIsApplied(BigInteger index, long deckSize)
            {
                var result = (index + N + deckSize) % deckSize;
                return result;
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

            public BigInteger IndexAfterTechniqueIsApplied(BigInteger index, long deckSize)
            {
                var result = deckSize - index - 1;
                return result;
            }
        }

        interface IInstruction<T>
        {
            public IList<T> ApplyTechnique(IReadOnlyList<T> input);

            public BigInteger IndexAfterTechniqueIsApplied(BigInteger index, long deckSize);
        }
    }
}
