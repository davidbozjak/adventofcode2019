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
            var inputParser = new SingleLineStringInputParser<int>(int.TryParse);
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            inputProvider.Reset();

            Part2(inputProvider.ToList());

            //Debug example:
            //Part1(new List<int>{ 3, 15, 3, 16, 1002, 16, 10, 16, 1, 16, 15, 15, 4, 15, 99, 0, 0 }, new List<int> { 4, 3, 2, 1, 0 });
            //Part1(new List<int> { 3,23,3,24,1002,24,10,24,1002,23,-1,23, 101,5,23,23,1,24,23,23,4,23,99,0,0 }, new List<int> { 0, 1, 2, 3, 4 });
            //Part1(new List<int> { 3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33, 1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0 });

            //Part2(new List<int> { 3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5 });
        }

        private static void Part1(IList<int> programCode)
        {
            var sequence = new List<int> { 0, 1, 2, 3, 4 };

            int maxOutput = 0;

            foreach (var permutatedSequence in PermuteList(sequence))
            {
                var output = AmplifySequence(programCode, sequence);

                if (output > maxOutput)
                {
                    maxOutput = output;
                    Console.WriteLine($"New max output {maxOutput} with sequence {string.Join(", ", permutatedSequence)}");
                }
            }

            int AmplifySequence(IList<int> programCode, IList<int> sequence)
            {
                var computer = new IntCodeComputer();
                var memory = new IntCodeMemory(programCode);

                int input = 0;
                int output = 0;
                int inputCallCount = 0;

                for (int i = 0; i < 5; i++)
                {
                    input = output;
                    inputCallCount = 0;

                    var memoryAfterExecution = computer.Run(memory, InputFunc, OutputFunc);

                    int InputFunc()
                    {
                        if (inputCallCount++ == 0) return sequence[i];
                        else return input;
                    }

                    void OutputFunc(int compOutput)
                    {
                        output = compOutput;
                        //Console.WriteLine($"Computer {i} output: {output}");
                    }
                }

                return output;
            }
        }

        private static void Part2(IList<int> programCode)
        {
            var sequence = new List<int> { 5, 6, 7, 8, 9 };
            
            int maxOutput = 0;

            foreach (var permutatedSequence in PermuteList(sequence))
            {
                var output = AmplifySequence(programCode, sequence).Result;

                if (output > maxOutput)
                {
                    maxOutput = output;
                    Console.WriteLine($"New max output {maxOutput} with sequence {string.Join(", ", permutatedSequence)}");
                }
            }

            async Task<int> AmplifySequence(IList<int> programCode, IList<int> sequence)
            {
                var computer = new IntCodeComputer();
                var memory = new IntCodeMemory(programCode);

                int[] input = new int[5];
                int[] inputCallCount = new int[5];
                TaskCompletionSource<int>[] taskCompletionSources = 
                    Enumerable.Range(0, 5).Select(_ => new TaskCompletionSource<int>()).ToArray();

                taskCompletionSources[0].SetResult(0);

                var tasks = new List<Task>();

                for (int i = 0; i < 5; i++)
                {
                    int indexCopy = i;

                    tasks.Add(Task.Run(() =>
                    {
                        var memoryAfterExecution = computer.Run(memory, () => InputFunc(indexCopy).Result, output => OutputFunc(output, indexCopy));
                    }));
                }

                await Task.WhenAll(tasks);

                return input[0];

                async Task<int> InputFunc(int compIndex)
                {
                    if (inputCallCount[compIndex]++ == 0)
                    {
                        return sequence[compIndex];
                    }
                    else
                    {
                        var input = await taskCompletionSources[compIndex].Task;
                        taskCompletionSources[compIndex] = new TaskCompletionSource<int>();
                        return input;
                    }
                }

                void OutputFunc(int compOutput, int compIndex)
                {
                    int inputIndex = compIndex + 1;

                    if (inputIndex > 4)
                        inputIndex = 0;

                    input[inputIndex] = compOutput;

                    taskCompletionSources[inputIndex].SetResult(compOutput);

                    //Console.WriteLine($"Computer {compIndex} output: {compOutput}");
                }
            }
        }

        private static IEnumerable<IList<int>> PermuteList(IList<int> sequence)
        {
            return permute(sequence, 0, sequence.Count);

            IEnumerable<IList<int>> permute(IList<int> sequence, int k, int m)
            {
                if (k == m)
                {
                    yield return sequence;
                }
                else
                {
                    for (int i = k; i < m; i++)
                    {
                        swapPlaces(sequence, k, i);
                        
                        foreach (var newSquence in permute(sequence, k + 1, m))
                        {
                            yield return newSquence;
                        }

                        swapPlaces(sequence, k, i);
                    }
                }
            }

            void swapPlaces(IList<int> sequence, int indexA, int indexB)
            {
                var temp = sequence[indexA];
                sequence[indexA] = sequence[indexB];
                sequence[indexB] = temp;
            }
        }

        
    }
}
