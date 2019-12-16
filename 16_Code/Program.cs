using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace _16_TBN
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<int>(int.TryParse, w => w.Select(w => new string(w, 1)).ToArray());
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetValue);

            //Part1(inputProvider.ToList());

            //Console.WriteLine();
            //inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<int> input)
        {
            input = RunFFT(input);

            Console.WriteLine($"After 100 phases: ");
            for (int i = 0; i < 8; i++)
            {
                Console.Write(input[i]);
            }
        }

        private static void Part2(IList<int> input)
        {
            double offset = 0;

            for (int i = 0; i <= 6; i++)
            {
                offset += Math.Pow(10, i) * input[6 - i];
            }

            var repeatedInput = new List<int>();

            for (int i = 0; i < 10000; i++)
            {
                repeatedInput.AddRange(input);
            }

            input = repeatedInput;

            input = RunFFT(input);

            Console.WriteLine($"After 100 phases: ");
            for (int i = 0; i < 8; i++)
            {
                int pos = (int)(i + offset);
                Console.Write(input[pos]);
            }
        }

        private static IList<int> RunFFT(IList<int> input)
        {
            int[] basePattern = { 0, 1, 0, -1};
            for (int phase = 0; phase < 100; phase++)
            {
                var output = new List<int>(input.Count);

                for (int i = 0; i < input.Count; i++)
                {
                    List<int> positive, negative;
                    GetIndices(i, out positive, out negative);

                    int sum = 0;

                    foreach (var index in positive)
                    {
                        sum += input[index];
                    }

                    foreach (var index in negative)
                    {
                        sum -= input[index];
                    }

                    //hack, j = i, as you know at the beginning you will only have 0s

                    //for (int j = i, factor = i + 1; j < input.Count; j++, factor = factor + 1 >= pattern.Length ? 0 : factor + 1)
                    //{
                    //    sum += input[j] * pattern[factor];
                    //}

                    output.Add((sum > 0 ? sum : -sum) % 10);
                }

                input = output;
            }

            return input;

            void GetIndices(int i, out List<int> positive, out List<int> negative)
            {
                positive = new List<int>();
                negative = new List<int>();

                for (int rep = i; rep < input.Count; rep += 4 * (i + 1))
                {
                    for (int j = rep, count = 0; count < i + 1 && j < input.Count; j++, count++)
                    {
                        positive.Add(j);
                    }

                    for (int j = rep + 2 * (i + 1), count = 0; count < i + 1 && j < input.Count; j++, count++)
                    {
                        negative.Add(j);
                    }
                }
            }
        }
    }
}
