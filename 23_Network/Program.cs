using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _23_Network
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            RunNetwork(inputProvider.ToList());
        }

        private static void RunNetwork(IList<long> programCode)
        {
            int networkSize = 50;
            var computers = new List<IIntCodeComputer>();
            var memories = new List<IIntCodeMemory>();

            var messageQueues = Enumerable.Range(0, networkSize).Select(w => new Queue<(long x, long y)>()).ToArray();
            var tasks = new List<Task>();
            var partialSentMessages = Enumerable.Range(0, networkSize).Select(w => (long?)w).ToArray();
            var partialReceivedMessages = new (long destinationAddress, long? x)?[networkSize];

            (long x, long y)? natPackage = null;

            var lockingObject = new object();

            for (int i = 0; i < networkSize; i++)
            {
                var computer = new IntCodeComputer();
                var memory = new IntCodeMemory(programCode);

                computers.Add(computer);
                memories.Add(memory);
                
                var networkAddress = i;

                tasks.Add(Task.Factory.StartNew(
                    () => computer.Run(memory, () => Input(networkAddress), output => Output(networkAddress, output)),
                    TaskCreationOptions.LongRunning));
            }

            Task.WhenAll(tasks).Wait();

            long Input(int networkAddress)
            {
                lock (lockingObject)
                {
                    if (partialSentMessages[networkAddress] != null)
                    {
                        var myAddress = partialSentMessages[networkAddress].Value;
                        partialSentMessages[networkAddress] = null;
                        return myAddress;
                    }
                    else if (messageQueues[networkAddress].Count > 0)
                    {
                        var message = messageQueues[networkAddress].Dequeue();
                        partialSentMessages[networkAddress] = message.y;
                        return message.x;
                    }
                    else
                    {
                        if (natPackage != null && messageQueues.All(w => w.Count == 0))
                        {
                            Console.WriteLine($"Message sent to NAT: ({natPackage.Value.x}, {natPackage.Value.y})");

                            messageQueues[0].Enqueue(natPackage.Value);
                            natPackage = null;
                        }

                        return -1;
                    }
                }
            }

            void Output(int networkAddress, long output)
            {
                lock (lockingObject)
                {
                    var partialMessage = partialReceivedMessages[networkAddress];

                    if (partialMessage == null)
                    {
                        partialReceivedMessages[networkAddress] = (output, null);
                    }
                    else
                    {
                        if (partialMessage.Value.x == null)
                        {
                            partialReceivedMessages[networkAddress] = (partialMessage.Value.destinationAddress, output);
                        }
                        else
                        {
                            var message = (partialMessage.Value.x.Value, output);

                            if (partialMessage.Value.destinationAddress < networkSize)
                            {
                                messageQueues[partialMessage.Value.destinationAddress].Enqueue(message);
                            }
                            else if (partialMessage.Value.destinationAddress == 255)
                            {
                                natPackage = message;
                            }
                            else throw new Exception("unexpected address");

                            partialReceivedMessages[networkAddress] = null;
                        }
                    }
                }
            }
        }
    }
}
