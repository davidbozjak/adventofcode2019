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
            JumpIfTrue = 5,
            JumpIfFalse = 6,
            LessThan = 7,
            Equals = 8,
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
                    IntInstruction.JumpIfTrue => 3,
                    IntInstruction.JumpIfFalse => 3,
                    IntInstruction.LessThan => 4,
                    IntInstruction.Equals => 4,
                    _ => throw new Exception("Unrecognized IntInstruction"),
                };

                if (instruction == IntInstruction.Add || instruction == IntInstruction.Multiply)
                {
                    HandleAdditionOrMultiplication(workingMemory, programPosition, instruction, modeParam1, modeParam2);
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
                    int input1 = workingMemory[programPosition + 1];
                    int param1 = modeParam1 == InstructionMode.PositionMode ? workingMemory[input1] : input1;

                    if (output == null)
                        throw new Exception("Program is expecting Output to be wired up");

                    output(param1);
                }
                else if (instruction == IntInstruction.JumpIfTrue || instruction == IntInstruction.JumpIfFalse)
                {
                    int input1 = workingMemory[programPosition + 1];
                    int param1 = modeParam1 == InstructionMode.PositionMode ? workingMemory[input1] : input1;

                    int input2 = workingMemory[programPosition + 2];
                    int param2 = modeParam2 == InstructionMode.PositionMode ? workingMemory[input2] : input2;

                    if ((instruction == IntInstruction.JumpIfTrue && (param1 != 0)) ||
                        (instruction == IntInstruction.JumpIfFalse && (param1 == 0)))
                    {
                        programPosition = param2;
                        sizeOfInstruction = 0;
                    }
                }
                else if (instruction == IntInstruction.Equals || instruction == IntInstruction.LessThan)
                {
                    int input1 = workingMemory[programPosition + 1];
                    int input2 = workingMemory[programPosition + 2];
                    int writeTo = workingMemory[programPosition + 3];

                    int value1 = modeParam1 == InstructionMode.PositionMode ? workingMemory[input1] : input1;
                    int value2 = modeParam2 == InstructionMode.PositionMode ? workingMemory[input2] : input2;

                    var result = instruction switch
                    {
                        IntInstruction.LessThan => value1 < value2 ? 1 : 0,
                        IntInstruction.Equals => value1 == value2 ? 1 : 0,
                        _ => throw new Exception("Unexpected instruction")
                    };

                    workingMemory[writeTo] = result;
                }
                else throw new Exception("Unexpected instruction");
            }

            return workingMemory;
        }

        private static void HandleAdditionOrMultiplication(IIntCodeMemory workingMemory, int programPosition, IntInstruction instruction, InstructionMode modeParam1, InstructionMode modeParam2)
        {
            int input1 = workingMemory[programPosition + 1];
            int input2 = workingMemory[programPosition + 2];
            int writeTo = workingMemory[programPosition + 3];

            int value1 = modeParam1 == InstructionMode.PositionMode ? workingMemory[input1] : input1;
            int value2 = modeParam2 == InstructionMode.PositionMode ? workingMemory[input2] : input2;

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

            modeParam1 = ParseInstructionAtIndex(strInput.Length - 3);

            modeParam2 = ParseInstructionAtIndex(strInput.Length - 4);

            modeParam3 = ParseInstructionAtIndex(strInput.Length - 5);

            return (instruction, modeParam1, modeParam2, modeParam3);

            InstructionMode ParseInstructionAtIndex(int index) =>
                (index < 0 || strInput.Length <= index) ? InstructionMode.PositionMode :
                    strInput[index] == '1' ? InstructionMode.ImmediateMode : InstructionMode.PositionMode;
        }
    }
}
