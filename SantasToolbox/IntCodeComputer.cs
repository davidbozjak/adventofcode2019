using System;

namespace SantasToolbox
{
    public interface IIntCodeComputer
    {
        IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM, Func<int>? input = null, Action<int>? output = null);
    }

    public class IntCodeComputer : IIntCodeComputer
    {
        private enum IntInstruction : int
        {
            Add = 1,
            Multiply = 2,
            Input = 3,
            Output = 4,
            EoF = 99
        }

        private enum InstructionMode : int
        {
            PositionMode = 0,
            ImmediateMode = 1,
        }

        public IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM, Func<int>? input = null, Action<int>? output = null)
        {
            var workingMemory = ROM.CloneWriteable();

            int sizeOfInstruction;

            for (int programPosition = 0; programPosition < workingMemory.Length; programPosition += sizeOfInstruction)
            {
                (IntInstruction instruction, InstructionMode modeParam1, InstructionMode modeParam2, InstructionMode modeParam3) = ParseInstruction(workingMemory[programPosition]);

                if (instruction == IntInstruction.EoF)
                {
                    break;
                }

                sizeOfInstruction = instruction switch
                {
                    IntInstruction.Add => 4,
                    IntInstruction.Multiply => 4,
                    IntInstruction.Input => 2,
                    IntInstruction.Output => 2,
                    _ => throw new Exception("Unrecognized IntInstruction"),
                };

                if (instruction == IntInstruction.Add || instruction == IntInstruction.Multiply)
                {
                    HandleAdditionOrMultiplication(workingMemory, programPosition, instruction, modeParam1, modeParam2, modeParam3);
                }
                else if (instruction == IntInstruction.Input)
                {
                    int address1 = workingMemory[programPosition + 1];

                    if (input == null)
                        throw new Exception("Program is expecting Input to be wired up");

                    workingMemory[address1] = input();
                }
                else if (instruction == IntInstruction.Output)
                {
                    int address1 = workingMemory[programPosition + 1];

                    if (output == null)
                        throw new Exception("Program is expecting Output to be wired up");

                    output(workingMemory[address1]);
                }
            }

            return workingMemory;
        }

        private static void HandleAdditionOrMultiplication(IIntCodeMemory workingMemory, int programPosition, IntInstruction instruction, InstructionMode modeParam1, InstructionMode modeParam2, InstructionMode modeParam3)
        {
            int address1 = workingMemory[programPosition + 1];
            int address2 = workingMemory[programPosition + 2];
            int writeTo = workingMemory[programPosition + 3];

            int value1 = modeParam1 == InstructionMode.PositionMode ? workingMemory[address1] : address1;
            int value2 = modeParam2 == InstructionMode.PositionMode ? workingMemory[address2] : address2;

            int result = instruction switch
            {
                IntInstruction.Add => value1 + value2,
                IntInstruction.Multiply => value1 * value2,
                _ => throw new Exception("Unrecognized IntInstruction"),
            };

            workingMemory[writeTo] = result;
        }

        private static (IntInstruction, InstructionMode, InstructionMode, InstructionMode) ParseInstruction(int input)
        {
            IntInstruction instruction;
            InstructionMode modeParam1, modeParam2, modeParam3;

            instruction = (IntInstruction)(input % 100);

            var strInput = input.ToString();

            modeParam1 = ParseInstructionAtIndex(3);

            modeParam2 = ParseInstructionAtIndex(4);

            modeParam3 = ParseInstructionAtIndex(5);

            return (instruction, modeParam1, modeParam2, modeParam3);

            InstructionMode ParseInstructionAtIndex(int index) =>
                strInput.Length <= index ? InstructionMode.ImmediateMode :
                    strInput[index] == '1' ? InstructionMode.PositionMode : InstructionMode.ImmediateMode;
        }
    }
}
