using System;
using SantasToolbox;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _7_Amplify
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
            //Part1(new List<int>{ 3, 15, 3, 16, 1002, 16, 10, 16, 1, 16, 15, 15, 4, 15, 99, 0, 0 }, new List<int> { 4, 3, 2, 1, 0 });
            //Part1(new List<int> { 3,23,3,24,1002,24,10,24,1002,23,-1,23, 101,5,23,23,1,24,23,23,4,23,99,0,0 }, new List<int> { 0, 1, 2, 3, 4 });
            //Part1(new List<int> { 3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33, 1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0 });

            //Part2(new List<int> { 3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5 });
        }

        private static void Part1(IList<long> programCode)
        {
            var sequence = new List<int> { 0, 1, 2, 3, 4 };

            (long maxOutput, IList<int> permutatedSequence) = FindMaxPermutation(programCode, sequence);

            Console.WriteLine($"Part1: New max output {maxOutput} with sequence {string.Join(", ", permutatedSequence)}");
        }

        private static void Part2(IList<long> programCode)
        {
            var sequence = new List<int> { 5, 6, 7, 8, 9 };

            (long maxOutput, IList<int> permutatedSequence) = FindMaxPermutation(programCode, sequence);

            Console.WriteLine($"Part2: New max output {maxOutput} with sequence {string.Join(", ", permutatedSequence)}");
        }

        private static (long maxOutput, IList<int> sequenceWithOutput) FindMaxPermutation(IList<long> programCode, IList<int> sequence)
        {
            long maxOutput = 0;
            IList<int> maxSequence = sequence;

            foreach (var permutatedSequence in sequence.PermuteList())
            {
                var output = AmplifySequence(programCode, sequence).Result;

                if (output > maxOutput)
                {
                    maxOutput = output;
                    maxSequence = permutatedSequence.ToList();
                }
            }

            return (maxOutput, maxSequence);
        }

        private static async Task<long> AmplifySequence(IList<long> programCode, IList<int> sequence)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            long[] input = new long[sequence.Count];
            int[] inputCallCount = new int[sequence.Count];
            TaskCompletionSource<long>[] taskCompletionSources =
                Enumerable.Range(0, sequence.Count).Select(_ => new TaskCompletionSource<long>()).ToArray();

            taskCompletionSources[0].SetResult(0);

            var tasks = new List<Task>();

            for (int i = 0; i < sequence.Count; i++)
            {
                int indexCopy = i;

                tasks.Add(Task.Run(() =>
                {
                    var memoryAfterExecution = computer.Run(memory, () => InputFunc(indexCopy).Result, output => OutputFunc(output, indexCopy));
                }));
            }

            await Task.WhenAll(tasks);

            return input[0];

            async Task<long> InputFunc(int compIndex)
            {
                if (inputCallCount[compIndex]++ == 0)
                {
                    return sequence[compIndex];
                }
                else
                {
                    var input = await taskCompletionSources[compIndex].Task;
                    taskCompletionSources[compIndex] = new TaskCompletionSource<long>();
                    return input;
                }
            }

            void OutputFunc(long compOutput, int compIndex)
            {
                int inputIndex = compIndex + 1;

                if (inputIndex > 4)
                    inputIndex = 0;

                input[inputIndex] = compOutput;

                taskCompletionSources[inputIndex].SetResult(compOutput);
            }
        }
    }
}
