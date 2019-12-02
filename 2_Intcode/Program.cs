using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SantasToolbox;

namespace _2_Intcode
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser();
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetInt);

            Part1(inputProvider.ToList());

            inputProvider.Reset();

            Part2(inputProvider.ToList());

            //Debug example:
            //Part1(new List<int>{ 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 });
        }

        private static void Part1(IList<int> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            // First, before running, replace the values of the program as required.
            memory[1] = 12;
            memory[2] = 2;

            var memoryAfterExecution = computer.Run(memory);

            int printPosition = 0;
            Console.WriteLine($"Value at Position {printPosition} is {memoryAfterExecution[printPosition]}");
        }

        private static void Part2(IList<int> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            for (int noun = 0; noun < 99; noun++)
            {
                for (int verb = 0; verb < 99; verb++)
                {
                    memory[1] = noun;
                    memory[2] = verb;

                    var memoryAfterExecution = computer.Run(memory);

                    int printPosition = 0;
                    int output = memoryAfterExecution[printPosition];

                    if (output == 19690720)
                    {
                        Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Reached the end!");
                        Console.WriteLine($"Initial values were Noun: {noun} Verb: {verb}");

                        return;
                    }
                }

            }
        }

        private class SingleLineStringInputParser
        {
            private string[]? inputStrings = null;
            private int currentPosition = 0;

            public bool GetInt(string? input, out int value)
            {
                value = -1;

                if (this.currentPosition >= this.inputStrings?.Length)
                {
                    this.inputStrings = null;
                    this.currentPosition = 0;
                }

                if (this.inputStrings == null)
                {
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        return false;
                    }

                    this.inputStrings = input.Split(',');
                }

                if (this.currentPosition >= this.inputStrings.Length)
                {
                    return false;
                }

                return int.TryParse(this.inputStrings[this.currentPosition++], out value);
            }
        }
    }
}
