using System;

namespace SantasToolbox
{
    public interface IIntCodeComputer
    {
        IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM, Func<long>? input = null, Action<long>? output = null);
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
            AdjustRelativeBase = 9,
            EoF = 99
        }

        private enum InstructionMode : int
        {
            PositionMode = 0,
            ImmediateMode = 1,
            RelativeMode = 2,
        }

        public IIntCodeMemory Run(IIntReadOnlyCodeMemory ROM, Func<long>? input = null, Action<long>? output = null)
        {
            var workingMemory = ROM.CloneWriteable();

            int sizeOfInstruction;
            long relativeBase = 0;

            for (long programPosition = 0; programPosition < workingMemory.Length; programPosition += sizeOfInstruction)
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
                    IntInstruction.AdjustRelativeBase => 2,
                    _ => throw new Exception("Unrecognized IntInstruction"),
                };

                if (instruction == IntInstruction.Add || instruction == IntInstruction.Multiply)
                {
                    HandleAdditionOrMultiplication(workingMemory, programPosition, relativeBase, instruction, modeParam1, modeParam2, modeParam3);
                }
                else if (instruction == IntInstruction.Input)
                {
                    long input1 = workingMemory[programPosition + 1];

                    long address1 = modeParam1 switch
                    {
                        InstructionMode.PositionMode => input1,
                        InstructionMode.RelativeMode => relativeBase + input1,
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    if (input == null)
                        throw new Exception("Program is expecting Input to be wired up");

                    workingMemory[address1] = input();
                }
                else if (instruction == IntInstruction.Output)
                {
                    long input1 = workingMemory[programPosition + 1];
                    long param1 = modeParam1 switch {
                        InstructionMode.PositionMode => workingMemory[input1],
                        InstructionMode.ImmediateMode => input1,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input1],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    if (output == null)
                        throw new Exception("Program is expecting Output to be wired up");

                    output(param1);
                }
                else if (instruction == IntInstruction.JumpIfTrue || instruction == IntInstruction.JumpIfFalse)
                {
                    long input1 = workingMemory[programPosition + 1];
                    long param1 = modeParam1 switch
                    {
                        InstructionMode.PositionMode => workingMemory[input1],
                        InstructionMode.ImmediateMode => input1,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input1],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    long input2 = workingMemory[programPosition + 2];
                    long param2 = modeParam2 switch
                    {
                        InstructionMode.PositionMode => workingMemory[input2],
                        InstructionMode.ImmediateMode => input2,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input2],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    if ((instruction == IntInstruction.JumpIfTrue && (param1 != 0)) ||
                        (instruction == IntInstruction.JumpIfFalse && (param1 == 0)))
                    {
                        programPosition = param2;
                        sizeOfInstruction = 0;
                    }
                }
                else if (instruction == IntInstruction.Equals || instruction == IntInstruction.LessThan)
                {
                    long input1 = workingMemory[programPosition + 1];
                    long input2 = workingMemory[programPosition + 2];
                    long input3 = workingMemory[programPosition + 3];

                    long value1 = modeParam1 switch
                    {
                        InstructionMode.PositionMode => workingMemory[input1],
                        InstructionMode.ImmediateMode => input1,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input1],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    long value2 = modeParam2 switch
                    {
                        InstructionMode.PositionMode => workingMemory[input2],
                        InstructionMode.ImmediateMode => input2,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input2],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    var result = instruction switch
                    {
                        IntInstruction.LessThan => value1 < value2 ? 1 : 0,
                        IntInstruction.Equals => value1 == value2 ? 1 : 0,
                        _ => throw new Exception("Unexpected instruction")
                    };

                    long writeTo = modeParam3 switch
                    {
                        InstructionMode.PositionMode => input3,
                        InstructionMode.RelativeMode => relativeBase + input3,
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    workingMemory[writeTo] = result;
                }
                else if (instruction == IntInstruction.AdjustRelativeBase)
                {
                    long input1 = workingMemory[programPosition + 1];

                    long value1 = modeParam1 switch
                    {
                        InstructionMode.PositionMode => workingMemory[input1],
                        InstructionMode.ImmediateMode => input1,
                        InstructionMode.RelativeMode => workingMemory[relativeBase + input1],
                        _ => throw new Exception("Unexpected instruction mode"),
                    };

                    relativeBase += value1;
                }
                else throw new Exception("Unexpected instruction");
            }

            return workingMemory;
        }

        private static void HandleAdditionOrMultiplication(IIntCodeMemory workingMemory, long programPosition, long relativeBase, IntInstruction instruction, InstructionMode modeParam1, InstructionMode modeParam2, InstructionMode modeParam3)
        {
            long input1 = workingMemory[programPosition + 1];
            long input2 = workingMemory[programPosition + 2];
            long input3 = workingMemory[programPosition + 3];

            long value1 = modeParam1 switch
            {
                InstructionMode.PositionMode => workingMemory[input1],
                InstructionMode.ImmediateMode => input1,
                InstructionMode.RelativeMode => workingMemory[relativeBase + input1],
                _ => throw new Exception("Unexpected instruction mode")
            };

            long value2 = modeParam2 switch
            {
                InstructionMode.PositionMode => workingMemory[input2],
                InstructionMode.ImmediateMode => input2,
                InstructionMode.RelativeMode => workingMemory[relativeBase + input2],
                _ => throw new Exception("Unexpected instruction mode")
            };

            long result = instruction switch
            {
                IntInstruction.Add => value1 + value2,
                IntInstruction.Multiply => value1 * value2,
                _ => throw new Exception("Unrecognized IntInstruction"),
            };

            long writeTo = modeParam3 switch
            {
                InstructionMode.PositionMode => input3,
                InstructionMode.RelativeMode => relativeBase + input3,
                _ => throw new Exception("Unexpected instruction mode")
            };

            workingMemory[writeTo] = result;
        }

        private static (IntInstruction, InstructionMode, InstructionMode, InstructionMode) ParseInstruction(long input)
        {
            IntInstruction instruction;
            InstructionMode modeParam1, modeParam2, modeParam3;

            instruction = (IntInstruction)(input % 100);

            var strInput = input.ToString();

            modeParam1 = ParseInstructionAtIndex(strInput.Length - 3);

            modeParam2 = ParseInstructionAtIndex(strInput.Length - 4);

            modeParam3 = ParseInstructionAtIndex(strInput.Length - 5);

            return (instruction, modeParam1, modeParam2, modeParam3);

            InstructionMode ParseInstructionAtIndex(int index)
            {
                if (index < 0 || strInput.Length <= index)
                    return InstructionMode.PositionMode;

                int value = strInput[index] -'0';

                return (InstructionMode)value;
            }
        }
    }
}
