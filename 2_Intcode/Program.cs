using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SantasToolbox;

namespace _2_Intcode
{
    class Program
    {
        private enum IntInstruction : int
        {
            Add = 1,
            Multiply = 2,
            EoF = 99
        }

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
            // First, before running, replace the values of the program as required.
            programCode[1] = 12;
            programCode[2] = 2;

            for (int programPosition = 0; programPosition < programCode.Count; programPosition += 4)
            {
                IntInstruction instruction = (IntInstruction)programCode[programPosition];

                if (instruction == IntInstruction.EoF)
                {
                    break;
                }

                int address1 = programCode[programPosition + 1];
                int address2 = programCode[programPosition + 2];
                int writeTo = programCode[programPosition + 3];

                int value1 = programCode[address1];
                int value2 = programCode[address2];

                int result;

                if (instruction == IntInstruction.Add)
                {
                    result = value1 + value2;
                }
                else if (instruction == IntInstruction.Multiply)
                {
                    result = value1 * value2;
                }
                else
                {
                    throw new Exception("Unrecognized IntInstruction");
                }

                programCode[writeTo] = result;
            }

            int printPosition = 0;
            Console.WriteLine($"Value at Position {printPosition} is {programCode[printPosition]}");
        }

        private static void Part2(IList<int> programCode)
        {
            // First, before running, replace the values of the program as required.

            for (int noun = 0; noun < 99; noun++)
            {
                for (int verb = 0; verb < 99; verb++)
                {
                    var memory = programCode.ToList();
                    memory[1] = noun;
                    memory[2] = verb;

                    for (int programPosition = 0; programPosition < memory.Count; programPosition += 4)
                    {
                        IntInstruction instruction = (IntInstruction)memory[programPosition];

                        if (instruction == IntInstruction.EoF)
                        {
                            break;
                        }

                        int address1 = memory[programPosition + 1];
                        int address2 = memory[programPosition + 2];
                        int writeTo = memory[programPosition + 3];

                        int value1 = memory[address1];
                        int value2 = memory[address2];

                        int result;

                        if (instruction == IntInstruction.Add)
                        {
                            result = value1 + value2;
                        }
                        else if (instruction == IntInstruction.Multiply)
                        {
                            result = value1 * value2;
                        }
                        else
                        {
                            throw new Exception("Unrecognized IntInstruction");
                        }

                        memory[writeTo] = result;
                    }

                    int printPosition = 0;
                    int output = memory[printPosition];
                    Console.WriteLine($"Value at Position {printPosition} is {output}");

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
                
            public SingleLineStringInputParser() { }

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
