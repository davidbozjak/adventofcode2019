using System;

namespace _2_Intcode
{
    interface IIntCodeComputer
    {
        IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM);
    }

    class IntCodeComputer : IIntCodeComputer
    {
        private enum IntInstruction : int
        {
            Add = 1,
            Multiply = 2,
            EoF = 99
        }

        public IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM)
        {
            var workingMemory = ROM.CloneWriteable();

            for (int programPosition = 0; programPosition < workingMemory.Length; programPosition += 4)
            {
                IntInstruction instruction = (IntInstruction)workingMemory[programPosition];

                if (instruction == IntInstruction.EoF)
                {
                    break;
                }

                int address1 = workingMemory[programPosition + 1];
                int address2 = workingMemory[programPosition + 2];
                int writeTo = workingMemory[programPosition + 3];

                int value1 = workingMemory[address1];
                int value2 = workingMemory[address2];

                int result = instruction switch
                {
                    IntInstruction.Add => value1 + value2,
                    IntInstruction.Multiply => value1 * value2,
                    _ => throw new Exception("Unrecognized IntInstruction"),
                };

                workingMemory[writeTo] = result;
            }

            return workingMemory;
        }
    }
}
