using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _9_Boost
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            inputProvider.Reset();

            Part2(inputProvider.ToList());

            //Debug example:
            //Part1(new List<long>{ 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 });
            //Part1(new List<long> { 1102, 34915192, 34915192, 7, 4, 7, 99, 0 });
            //Part1(new List<long> { 104, 1125899906842624, 99 });
        }

        private static void Part1(IList<long> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            var memoryAfterExecution = computer.Run(memory, () => 1, Console.WriteLine);
        }

        private static void Part2(IList<long> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            var memoryAfterExecution = computer.Run(memory, () => 2, Console.WriteLine);
        }
    }
}
