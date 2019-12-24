using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _16_Code
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<int>(int.TryParse, w => w.Select(w => new string(w, 1)).ToArray());
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            Console.WriteLine();
            inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<int> input)
        {
            input = RunFFT(input.ToArray());

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

            input = RunFFT(input.ToArray(), (int)offset);

            Console.WriteLine($"After 100 phases: ");
            for (int i = 0; i < 8; i++)
            {
                int pos = (int)(i + offset);
                Console.Write(input[pos]);
            }
        }

        private static IList<int> RunFFT(int[] input, int offset = 0)
        {
            for (int phase = 0; phase < 100; phase++)
            {
                var output = new int[input.Length];

                int sum = 0;

                for (int i = input.Length - 1; i >= offset; i--)
                {
                    if (i > (input.Length * 3 / 4))
                    {
                        // for last quater we just add up all the digits
                        sum += input[i];
                    }
                    else
                    {
                        // the rest mustn't rely on the last digit, instead it needs to start from scratch and to the "real algorithm"
                        sum = 0;

                        for (int rep = i; rep < input.Length; rep += 4 * (i + 1))
                        {
                            for (int j = rep, count = 0; count < i + 1 && j < input.Length; j++, count++)
                            {
                                sum += input[j];
                            }

                            for (int j = rep + 2 * (i + 1), count = 0; count < i + 1 && j < input.Length; j++, count++)
                            {
                                sum -= input[j];
                            }
                        }
                    }

                    sum = (sum > 0 ? sum : -sum) % 10;

                    output[i] = sum;
                }

                input = output;
            }

            return input;
        }
    }
}
