﻿using System;
using System.Collections.Generic;
using System.Linq;
using SantasToolbox;

namespace _5_Diagnostics
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
            //Part1(new List<long>{ 3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8 });
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

            var memoryAfterExecution = computer.Run(memory, () => 5, Console.WriteLine);
        }
    }
}
