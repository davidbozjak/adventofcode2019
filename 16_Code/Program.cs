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

            Part1(inputProvider.ToList());

            //Console.WriteLine();
            //inputProvider.Reset();

            //Part2(inputProvider.ToList());
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
            int[] basePattern = { 0, 1, 0, -1 };
            for (int phase = 0; phase < 100; phase++)
            {
                var output = new List<int>(input.Count);

                for (int i = 0; i < input.Count; i++)
                {
                    var pattern = new List<int>();
                    for (int j = 0; j < basePattern.Length; j++)
                    {
                        for (int k = 0; k < (i + 1); k++)
                        {
                            pattern.Add(basePattern[j]);
                        }
                    }

                    int sum = 0;

                    for (int j = 0, factor = 1; j < input.Count; j++, factor = factor + 1 >= pattern.Count ? 0 : factor + 1)
                    {
                        sum += input[j] * pattern[factor];
                    }

                    var result = sum.ToString();
                    output.Add(result[result.Length - 1] - '0');
                }

                input = output;
            }

            return input;
        }
    }
}
