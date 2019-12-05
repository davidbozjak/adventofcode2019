using System;
using System.Collections.Generic;
using System.Linq;
using SantasToolbox;

namespace _5_Diagnostics
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<int>(int.TryParse);
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetValue);

            //Part1(inputProvider.ToList());

            //inputProvider.Reset();

            //Part2(inputProvider.ToList());

            //Debug example:
            Part1(new List<int>{ 3, 0, 4, 0, 99 });
        }

        private static void Part1(IList<int> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            var memoryAfterExecution = computer.Run(memory, () => 1, Console.WriteLine);

            int printPosition = 0;
            Console.WriteLine($"Value at Position {printPosition} is {memoryAfterExecution[printPosition]}");
        }

        private static void Part2(IList<int> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            
        }
    }
}
