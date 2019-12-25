using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _25_SantasMaze
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            computer.Run(memory, Input, Output);

            long Input()
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                {
                    return 10;
                }

                return key.KeyChar;
            }

            void Output(long output)
            {
                Console.Write((char)output);
            }
        }
    }
}
